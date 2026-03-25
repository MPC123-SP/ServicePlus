using Microsoft.EntityFrameworkCore;
using Npgsql;
using ServicePlusEXT.Context;
using ServicePlusEXT.Dtos;
using ServicePlusEXT.Shared;
using System.Text;
using System.Text.Json;

namespace ServicePlusEXT.Endpoints
{
    public class Endpoint : IEndpoint
    {
        private const int InsertBatchSize = 500;
        private const int AttributeInsertBatchSize = 2000;

        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/get-service-details", async (
                HttpClient http,
                ServiceDetailsRequest dto,
                AppDbContext db,
                CancellationToken cancellationToken) =>
            {
                http.Timeout = TimeSpan.FromMinutes(60);
                var url = $"https://eservices.punjab.gov.in/v1/application/details/?serviceId={dto.serviceId}&date={dto.date}";

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
                };

                request.Headers.Add("client_id", dto.clientId);
                request.Headers.Add("client_secret", dto.secretId);

                var response = await http.SendAsync(request, cancellationToken);
                var json = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return Results.BadRequest(json);
                }

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Array)
                {
                    return Results.BadRequest("Expected array");
                }

                var records = ParseIncomingApplications(root);
                if (records.Count == 0)
                {
                    return Results.Ok(new
                    {
                        inserted = 0,
                        skipped = 0
                    });
                }

                var incomingCount = root.GetArrayLength();
                var connection = (NpgsqlConnection)db.Database.GetDbConnection();
                await connection.OpenAsync(cancellationToken);

                await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
                var insertedApplicationIds = new Dictionary<(string RefNo, string ApplId), int>(records.Count);

                foreach (var batch in Chunk(records, InsertBatchSize))
                {
                    var insertedBatch = await InsertApplicationsAsync(connection, transaction, batch, cancellationToken);
                    foreach (var inserted in insertedBatch)
                    {
                        insertedApplicationIds[(inserted.ApplRefNo, inserted.ApplId)] = inserted.Id;
                    }
                }

                foreach (var batch in Chunk(records, InsertBatchSize))
                {
                    await InsertAttributesAsync(connection, transaction, batch, insertedApplicationIds, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

                var insertedCount = insertedApplicationIds.Count;
                return Results.Ok(new
                {
                    inserted = insertedCount,
                    skipped = incomingCount - insertedCount
                });
            });
        }

        private static List<IncomingApplicationRecord> ParseIncomingApplications(JsonElement root)
        {
            var seenKeys = new HashSet<(string RefNo, string ApplId)>();
            var records = new List<IncomingApplicationRecord>();

            foreach (var item in root.EnumerateArray())
            {
                var applRefNo = item.GetProperty("appl_ref_no").GetString() ?? string.Empty;
                var applId = item.GetProperty("appl_id").GetString() ?? string.Empty;
                var key = (applRefNo, applId);

                if (!seenKeys.Add(key))
                {
                    continue;
                }

                var attributes = new List<ApplicationAttributeRecord>();
                if (item.TryGetProperty("application_form_attributes", out var attrs) &&
                    attrs.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in attrs.EnumerateObject())
                    {
                        attributes.Add(new ApplicationAttributeRecord(prop.Name, prop.Value.ToString()));
                    }
                }

                records.Add(new IncomingApplicationRecord(
                    applRefNo,
                    applId,
                    item.GetProperty("applied_by").GetString() ?? string.Empty,
                    attributes));
            }

            return records;
        }

        private static async Task<List<InsertedApplicationRecord>> InsertApplicationsAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            IReadOnlyList<IncomingApplicationRecord> applications,
            CancellationToken cancellationToken)
        {
            if (applications.Count == 0)
            {
                return [];
            }

            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("INSERT INTO \"ServiceApplications\" (\"ApplRefNo\", \"ApplId\", \"AppliedBy\") VALUES ");

            await using var command = connection.CreateCommand();
            command.Transaction = transaction;

            for (var i = 0; i < applications.Count; i++)
            {
                if (i > 0)
                {
                    sqlBuilder.Append(", ");
                }

                sqlBuilder.Append($"(@refNo{i}, @applId{i}, @appliedBy{i})");
                command.Parameters.AddWithValue($"refNo{i}", applications[i].ApplRefNo);
                command.Parameters.AddWithValue($"applId{i}", applications[i].ApplId);
                command.Parameters.AddWithValue($"appliedBy{i}", applications[i].AppliedBy);
            }

            sqlBuilder.Append(" ON CONFLICT (\"ApplRefNo\", \"ApplId\") DO NOTHING");
            sqlBuilder.Append(" RETURNING \"Id\", \"ApplRefNo\", \"ApplId\";");
            command.CommandText = sqlBuilder.ToString();

            var inserted = new List<InsertedApplicationRecord>(applications.Count);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                inserted.Add(new InsertedApplicationRecord(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2)));
            }

            return inserted;
        }

        private static async Task InsertAttributesAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            IReadOnlyList<IncomingApplicationRecord> applications,
            IReadOnlyDictionary<(string RefNo, string ApplId), int> insertedApplicationIds,
            CancellationToken cancellationToken)
        {
            var attributesToInsert = new List<(int ServiceApplicationId, string Key, string Value)>();

            foreach (var application in applications)
            {
                if (!insertedApplicationIds.TryGetValue((application.ApplRefNo, application.ApplId), out var applicationId))
                {
                    continue;
                }

                foreach (var attribute in application.Attributes)
                {
                    attributesToInsert.Add((applicationId, attribute.Key, attribute.Value));
                }
            }

            if (attributesToInsert.Count == 0)
            {
                return;
            }

            foreach (var attributeBatch in Chunk(attributesToInsert, AttributeInsertBatchSize))
            {
                var sqlBuilder = new StringBuilder();
                sqlBuilder.Append("INSERT INTO \"ApplicationAttribute\" (\"ServiceApplicationId\", \"Key\", \"Value\") VALUES ");

                await using var command = connection.CreateCommand();
                command.Transaction = transaction;

                for (var i = 0; i < attributeBatch.Count; i++)
                {
                    if (i > 0)
                    {
                        sqlBuilder.Append(", ");
                    }

                    sqlBuilder.Append($"(@serviceApplicationId{i}, @key{i}, @value{i})");
                    command.Parameters.AddWithValue($"serviceApplicationId{i}", attributeBatch[i].ServiceApplicationId);
                    command.Parameters.AddWithValue($"key{i}", attributeBatch[i].Key);
                    command.Parameters.AddWithValue($"value{i}", attributeBatch[i].Value);
                }

                sqlBuilder.Append(';');
                command.CommandText = sqlBuilder.ToString();

                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        private static IEnumerable<IReadOnlyList<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (var i = 0; i < source.Count; i += size)
            {
                yield return source.Skip(i).Take(size).ToArray();
            }
        }

        private sealed record IncomingApplicationRecord(
            string ApplRefNo,
            string ApplId,
            string AppliedBy,
            IReadOnlyList<ApplicationAttributeRecord> Attributes);

        private sealed record ApplicationAttributeRecord(string Key, string Value);

        private sealed record InsertedApplicationRecord(int Id, string ApplRefNo, string ApplId);
    }
}

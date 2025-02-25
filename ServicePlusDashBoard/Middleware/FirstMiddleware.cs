using System.Net;

public class FirstMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FirstMiddleware> _logger;
    public FirstMiddleware(RequestDelegate next, ILogger<FirstMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Authorization header is missing or empty.");
        }
        else
        {
            _logger.LogInformation($"Authorization header: {token}");
        }


        await _next(context);

         
    }
}

using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Google.Cloud.Translation.V2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using ServicePlusAPIs.AuthenticateModels;
using ServicePlusAPIs.Context;
using ServicePlusAPIs.ExternalAPIs;
using ServicePlusAPIs.HelperModels;
using ServicePlusAPIs.HelperViewModel;
using ServicePlusAPIs.Models;
using ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel;
using ServicePlusAPIs.Models.EnclouserDetails;
using ServicePlusAPIs.Models.ExecutionModel;
using ServicePlusAPIs.Models.InitiatedModel;
using ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Execution_OfficialFormDetails;
using ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Initiated_AttributeDetails;
using ServicePlusAPIs.Models.SportsModel;
using ServicePlusAPIs.ReportsModel;
using ServicePlusAPIs.ReportsViewModel;
using ServicePlusAPIs.ViewModels;
using ServicePlusAPIs.ViewModels.PublicModel;
using ServicePlusAPIs.ViewModels.SportsModel;
using System.Buffers;
using System.Data;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using MediaType = PuppeteerSharp.Media.MediaType;

namespace ServicePlusAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicePlusController : ControllerBase
    {
        private readonly PostgresDbContext _postgresDbContext;
        private readonly ServicePlusContext _servicePlusContext;
        private readonly IMapper _mapper;
        private readonly ILogger<ServicePlusController> _logger;
        // Outside the action method, possibly in the controller or a service class.
        private readonly Dictionary<string, string> districtNameMap = new Dictionary<string, string>();
        private string text;

        public ServicePlusController(IMapper mapper, ILogger<ServicePlusController> logger, PostgresDbContext postgresDbContext, ServicePlusContext servicePlusContext)
        {
            _mapper = mapper;
            _logger = logger;
            _postgresDbContext = postgresDbContext;
            _servicePlusContext = servicePlusContext;
        }

        #region IncomeAdd
        [AllowAnonymous]
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> IncomeAdd([FromBody] ServiceViewModel serviceViewModel)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(serviceViewModel, new JsonSerializerOptions
            {
                WriteIndented = true // Makes the JSON output formatted and easier to read
            });

            _logger.LogInformation($"JSON RECEIVED-IncomeService: {json}");

            //map from ServiceViewModel to ServiceModel
            var mappedData = _mapper.Map<ServiceViewModel, ServiceModel>(serviceViewModel);
            var initiatedList = mappedData.InitiatedData.Count();
            var executionList = mappedData.ExecutionData.Count();
            //Here we Use JSONReceived Method

            if (initiatedList != 0 || executionList != 0)
            {
                //Initiated Set
                List<InitiatedData> initiatedDataList = mappedData.InitiatedData.ToList();
                List<List<EnclosureDetail>> enclosureDetailLists = initiatedDataList.Where(d => d.EnclosureDetails != null).Select(d => d.EnclosureDetails).ToList();
                List<EnclosureDetail> enclosureDetailsList = enclosureDetailLists.SelectMany(x => x).ToList();
                List<AttributeDetail> attributeDetailList = initiatedDataList.Where(d => d.AttributeDetail != null).SelectMany(d => d.AttributeDetail).ToList();
                //Exection Set
                List<ExecutionData> executionDataList = mappedData.ExecutionData.ToList();
                List<TaskDetail> taskDetailsList = executionDataList.Where(d => d.TaskDetails != null).Select(d => d.TaskDetails).ToList();
                List<UserDetail> userDetails = taskDetailsList.Select(x => x.UserDetail).ToList();
                List<OfficialFormDetail> officialFormDetails = executionDataList.Where(d => d.OfficialFormDetail != null).SelectMany(d => d.OfficialFormDetail).ToList();
                var onebacthSubmissionDate = initiatedDataList.Select(d => d.SubmissionDate).FirstOrDefault();
                var onebacthTaskDetailList = taskDetailsList.Select(d => d.ReceivedTime).FirstOrDefault();
                _logger.LogInformation("Api || Hit || Testing-2 stage Mapping Done" + " initiatedList " + initiatedList + " executionList " + executionList + " InitiatedOneRecordDate " + onebacthSubmissionDate + " ExecutionDate " + onebacthTaskDetailList);
                //Declare due to assign custom flag and insertion time
                List<ExecutionData> executionData = new List<ExecutionData>() { };
                List<InitiatedData> initiatedData = new List<InitiatedData>();

                //To get Application ids for both dataset 
                var applIds = mappedData.InitiatedData.Select(d => d.ApplId).ToList();
                var applIdsExecution = mappedData.ExecutionData.Select(d => d.TaskDetails.ApplId).ToList();

                DateTime dateTime = DateTime.Now;
                DateTime utcDateTime = dateTime.ToUniversalTime(); // Convert to UTC
                CultureInfo hiIN = new CultureInfo("en-IN");
                DateTime hiINDateTime = Convert.ToDateTime(utcDateTime, hiIN);
                // Fetch all existing InitiatedRecordInsertionFlags for the relevant ApplIds
                var existingInitiatedRecords = await _postgresDbContext.InitiatedDatas
                    .Where(x => applIds.Contains(x.ApplId))
                    .GroupBy(x => x.ApplId)
                    .Select(group => new
                    {
                        ApplId = group.Key,
                        MaxInitiatedRecordInsertionFlag = group.Max(x => x.InitiatedRecordInsertionFlag)
                    })
                    .ToListAsync();

                // Fetch all existing ExecutionDataRecordInsertionFlags for the relevant ApplIds
                var existingExecutionRecords = await _postgresDbContext.ExecutionDatas
                    .Where(x => applIdsExecution.Contains(x.TaskDetails.ApplId))
                    .GroupBy(x => x.TaskDetails.ApplId)
                    .Select(group => new
                    {
                        ApplId = group.Key,
                        MaxExecutionDataRecordInsertionFlag = group.Max(x => x.ExecutionDataRecordInsertionFlag)
                    })
                    .ToListAsync();

                foreach (var initiated in initiatedDataList)
                {
                    var existingInitiatedRecord = existingInitiatedRecords.FirstOrDefault(x => x.ApplId == initiated.ApplId);
                    int nextInitiatedFlag = (existingInitiatedRecord?.MaxInitiatedRecordInsertionFlag ?? 0) + 1;

                    initiated.InitiatedRecordInsertionFlag = nextInitiatedFlag;
                    initiated.InitiatedRecordInsertionTime = hiINDateTime;
                    initiatedData.Add(initiated);
                }

                foreach (var exec in executionDataList)
                {
                    var existingExecutionRecord = existingExecutionRecords.FirstOrDefault(x => x.ApplId == exec.TaskDetails.ApplId);
                    int nextExecutionFlag = (existingExecutionRecord?.MaxExecutionDataRecordInsertionFlag ?? 0) + 1;

                    exec.ExecutionDataRecordInsertionFlag = nextExecutionFlag;
                    exec.ExecutionDataRecordInsertionTime = hiINDateTime;
                    executionData.Add(exec);
                }

                try
                {

                    _logger.LogInformation("Data Insertion Start from Here");


                    // Use AddRangeAsync to add the collection of initiatedDataList in batches
                    if (initiatedData != null)
                    {
                        int batchSize = 500;
                        int totalRecords = initiatedData.Count;
                        int iterations = (int)Math.Ceiling((double)totalRecords / batchSize);

                        for (int i = 0; i < iterations; i++)
                        {
                            var currentRecords = initiatedData.Skip(i * batchSize).Take(batchSize);
                            if (currentRecords != null)

                                await _postgresDbContext.InitiatedDatas.AddRangeAsync(currentRecords);
                            else
                                _logger.LogInformation("Null Check Insertion initiatedData  " + currentRecords);

                        }

                    }

                    // Use AddRangeAsync to add the collection of enclosureDetailsList in batches
                    if (enclosureDetailsList != null)
                    {
                        int batchSize = 500;
                        int totalRecords = enclosureDetailsList.Count;
                        int iterations = (int)Math.Ceiling((double)totalRecords / batchSize);

                        for (int i = 0; i < iterations; i++)
                        {
                            var currentRecords = enclosureDetailsList.Skip(i * batchSize).Take(batchSize);
                            if (currentRecords != null)
                                await _postgresDbContext.EnclosureDetails.AddRangeAsync(currentRecords);
                            else
                                _logger.LogInformation("Null Check Insertion enclosureDetailsList  " + currentRecords);

                        }

                    }

                    // Use AddRangeAsync to add the collection of attributeDetailList in batches
                    if (attributeDetailList != null)
                    {
                        int batchSize = 500;
                        int totalRecords = attributeDetailList.Count;
                        int iterations = (int)Math.Ceiling((double)totalRecords / batchSize);

                        for (int i = 0; i < iterations; i++)
                        {
                            var currentRecords = attributeDetailList.Skip(i * batchSize).Take(batchSize);
                            if (currentRecords != null)
                                await _postgresDbContext.AttributeDetails.AddRangeAsync(currentRecords);
                            else
                                _logger.LogInformation("Null Check Insertion attributeDetailList  " + currentRecords);

                        }

                    }

                    // Use AddRangeAsync to add the collection of executionDataList in batches
                    if (executionData != null)
                    {
                        int batchSize = 500;
                        int totalRecords = executionData.Count;
                        int iterations = (int)Math.Ceiling((double)totalRecords / batchSize);

                        for (int i = 0; i < iterations; i++)
                        {
                            var currentRecords = executionData.Skip(i * batchSize).Take(batchSize);
                            if (currentRecords != null)
                                await _postgresDbContext.ExecutionDatas.AddRangeAsync(currentRecords);
                            else
                                _logger.LogInformation("Null Check Insertion executionData  " + currentRecords);

                        }

                    }

                    // Use AddRangeAsync to add the collection of taskDetailsList in batches
                    if (taskDetailsList != null)
                    {
                        int batchSize = 500;
                        int totalRecords = taskDetailsList.Count;
                        int iterations = (int)Math.Ceiling((double)totalRecords / batchSize);

                        for (int i = 0; i < iterations; i++)
                        {
                            var currentRecords = taskDetailsList.Skip(i * batchSize).Take(batchSize);
                            if (currentRecords != null)
                                await _postgresDbContext.TaskDetails.AddRangeAsync(currentRecords);

                            else
                                _logger.LogInformation("Null Check Insertion taskDetailsList  " + currentRecords);
                        }

                    }

                    // Use AddRangeAsync to add the collection of userDetails in batches
                    if (userDetails != null)
                    {
                        int batchSize = 500;
                        int totalRecords = userDetails.Count;
                        int iterations = (int)Math.Ceiling((double)totalRecords / batchSize);

                        int totalRecordTaskDetail = taskDetailsList.Count;
                        int taskDetailIteration = (int)Math.Ceiling((double)totalRecordTaskDetail / batchSize);


                        for (int i = 0; i < iterations; i++)
                        {


                            var currentRecords = userDetails.Skip(i * batchSize).Take(batchSize).ToList();
                            List<TaskDetail> currentTaskDetaikRecords = taskDetailsList.Skip(i * batchSize).Take(batchSize).ToList();
                            var modifiedRecords = new List<UserDetail>();

                            int j = 0;
                            foreach (var userDetail in currentRecords)
                            {

                                UserDetail userDetail1;
                                if (userDetail == null)
                                {
                                    // Assign custom values when UserDetail is null
                                    var tempUserDetail = new UserDetail
                                    {
                                        UserName = "NoDataFlag",
                                        DepartmentLevel = "NoDataFlag",
                                        CurrentProcessId = 1,
                                        Designation = "NoDataFlag",
                                        LocationId = "1",
                                        LocationName = "NoDataFlag",
                                        LocationTypeId = "1",
                                        PullUserId = 1
                                    };

                                    // Replace the null user_detail with the custom value
                                    userDetail1 = tempUserDetail;
                                }
                                else
                                {
                                    userDetail1 = userDetail;
                                }

                                // Refer the corresponding TaskDetail to UserDetails
                                if (j < currentTaskDetaikRecords.Count && currentTaskDetaikRecords[j] != null)
                                {
                                    userDetail1.TaskDetail = currentTaskDetaikRecords[j];
                                }

                                modifiedRecords.Add(userDetail1);
                                j++;
                            }

                            await _postgresDbContext.UserDetails.AddRangeAsync(modifiedRecords);

                        }


                    }

                    // Use AddRangeAsync to add the collection of officialFormDetails in batches
                    if (officialFormDetails != null)
                    {
                        int batchSize = 500;
                        int totalRecords = officialFormDetails.Count;
                        int iterations = (int)Math.Ceiling((double)totalRecords / batchSize);

                        for (int i = 0; i < iterations; i++)
                        {

                            var currentRecords = officialFormDetails.Skip(i * batchSize).Take(batchSize);
                            if (currentRecords != null)
                                await _postgresDbContext.OfficialFormDetails.AddRangeAsync(currentRecords);


                            else
                                _logger.LogInformation("Null Check Insertion officialFormDetails  " + currentRecords);

                        }

                    }


                    DateTime jsonRecievedDateTime = DateTime.Now;
                    DateTime jsonUTCDateTime = jsonRecievedDateTime.ToUniversalTime(); // Convert to UTC
                    CultureInfo jsonHiIN = new CultureInfo("en-IN");
                    DateTime jsonHiINDateTime = Convert.ToDateTime(utcDateTime, hiIN);

                    var dateOfJsonReceived = jsonHiINDateTime;

                    var jsonReceived = JSONReceivedMapping(dateOfJsonReceived, initiatedList, executionList);

                    await _postgresDbContext.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    _logger.LogInformation("Logs info---Exception" + ex);

                    return BadRequest(ex);
                }


                return Ok("Initiated Record " + initiatedList + " " + "Execution Record " + executionList + " " + "Records Succesfully Added in DataBase");

            }
            else
            {
                return BadRequest("Initiated Record " + initiatedList + " " + "Execution Record " + executionList + " " + "Records Succesfully Added in DataBase");

            }
        }
        #endregion

        #region AddServicePlusData
        [AllowAnonymous]
        [HttpPost]
        [Route("AddServicePlusData")]
        public async Task<IActionResult> AddServicePlusData([FromBody] ServiceViewModel serviceViewModel)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(serviceViewModel, new JsonSerializerOptions
            {
                WriteIndented = true // Makes the JSON output formatted and easier to read
            });

            _logger.LogInformation($"JSON RECEIVED-All Service: {json}");

            //map from ServiceViewModel to ServiceModel
            var mappedData = _mapper.Map<ServiceViewModel, ServiceModel>(serviceViewModel);
            var initiatedList = mappedData.InitiatedData.Count();
            var executionList = mappedData.ExecutionData.Count();
            //Here we Use JSONReceived Method

            if (initiatedList != 0 || executionList != 0)
            {
                //Initiated Set
                List<InitiatedData> initiatedDataList = mappedData.InitiatedData.ToList();
                List<List<EnclosureDetail>> enclosureDetailLists = initiatedDataList.Where(d => d.EnclosureDetails != null).Select(d => d.EnclosureDetails).ToList();
                List<EnclosureDetail> enclosureDetailsList = enclosureDetailLists.SelectMany(x => x).ToList();
                List<AttributeDetail> attributeDetailList = initiatedDataList.Where(d => d.AttributeDetail != null).SelectMany(d => d.AttributeDetail).ToList();
                //Exection Set
                List<ExecutionData> executionDataList = mappedData.ExecutionData.ToList();
                List<TaskDetail> taskDetailsList = executionDataList.Where(d => d.TaskDetails != null).Select(d => d.TaskDetails).ToList();
                List<UserDetail> userDetails = taskDetailsList.Select(x => x.UserDetail).ToList();
                List<OfficialFormDetail> officialFormDetails = executionDataList.Where(d => d.OfficialFormDetail != null).SelectMany(d => d.OfficialFormDetail).ToList();
                var onebacthSubmissionDate = initiatedDataList.Select(d => d.SubmissionDate).FirstOrDefault();
                var onebacthTaskDetailList = taskDetailsList.Select(d => d.ReceivedTime).FirstOrDefault();
                //Declare due to assign custom flag and insertion time
                List<ExecutionData> executionData = new List<ExecutionData>() { };
                List<InitiatedData> initiatedData = new List<InitiatedData>();

                //To get Application ids for both dataset 
                var applIds = mappedData.InitiatedData.Select(d => d.ApplId).ToList();
                var applIdsExecution = mappedData.ExecutionData.Select(d => d.TaskDetails.ApplId).ToList();

                DateTime dateTime = DateTime.Now;
                DateTime utcDateTime = dateTime.ToUniversalTime(); // Convert to UTC
                CultureInfo hiIN = new CultureInfo("en-IN");
                DateTime hiINDateTime = Convert.ToDateTime(utcDateTime, hiIN);
                // Fetch all existing InitiatedRecordInsertionFlags for the relevant ApplIds
                var existingInitiatedRecords = await _servicePlusContext.InitiatedDatas
                    .Where(x => applIds.Contains(x.ApplId))
                    .GroupBy(x => x.ApplId)
                    .Select(group => new
                    {
                        ApplId = group.Key,
                        MaxInitiatedRecordInsertionFlag = group.Max(x => x.InitiatedRecordInsertionFlag)
                    })
                    .ToListAsync();

                // Fetch all existing ExecutionDataRecordInsertionFlags for the relevant ApplIds
                var existingExecutionRecords = await _servicePlusContext.ExecutionDatas
                    .Where(x => applIdsExecution.Contains(x.TaskDetails.ApplId))
                    .GroupBy(x => x.TaskDetails.ApplId)
                    .Select(group => new
                    {
                        ApplId = group.Key,
                        MaxExecutionDataRecordInsertionFlag = group.Max(x => x.ExecutionDataRecordInsertionFlag)
                    })
                    .ToListAsync();

                foreach (var initiated in initiatedDataList)
                {
                    var existingInitiatedRecord = existingInitiatedRecords.FirstOrDefault(x => x.ApplId == initiated.ApplId);
                    int nextInitiatedFlag = (existingInitiatedRecord?.MaxInitiatedRecordInsertionFlag ?? 0) + 1;

                    initiated.InitiatedRecordInsertionFlag = nextInitiatedFlag;
                    initiated.InitiatedRecordInsertionTime = hiINDateTime;
                    initiatedData.Add(initiated);
                }

                foreach (var exec in executionDataList)
                {
                    var existingExecutionRecord = existingExecutionRecords.FirstOrDefault(x => x.ApplId == exec.TaskDetails.ApplId);
                    int nextExecutionFlag = (existingExecutionRecord?.MaxExecutionDataRecordInsertionFlag ?? 0) + 1;

                    exec.ExecutionDataRecordInsertionFlag = nextExecutionFlag;
                    exec.ExecutionDataRecordInsertionTime = hiINDateTime;
                    executionData.Add(exec);
                }

                try
                {

                    _logger.LogInformation("Data Insertion Start from Here");


                    // Use AddRangeAsync to add the collection of initiatedDataList in batches
                    if (initiatedData != null)
                    {
                        int batchSize = 500;
                        int totalRecords = initiatedData.Count;
                        int iterations = (int)Math.Ceiling((double)totalRecords / batchSize);

                        for (int i = 0; i < iterations; i++)
                        {
                            var currentRecords = initiatedData.Skip(i * batchSize).Take(batchSize);
                            if (currentRecords != null)

                                await _servicePlusContext.InitiatedDatas.AddRangeAsync(currentRecords);
                            else
                                _logger.LogInformation("Null Check Insertion initiatedData  " + currentRecords);

                        }

                    }

                    // Use AddRangeAsync to add the collection of enclosureDetailsList in batches
                    if (enclosureDetailsList != null)
                    {
                        int batchSize = 500;
                        int totalRecords = enclosureDetailsList.Count;
                        int iterations = (int)Math.Ceiling((double)totalRecords / batchSize);

                        for (int i = 0; i < iterations; i++)
                        {
                            var currentRecords = enclosureDetailsList.Skip(i * batchSize).Take(batchSize);
                            if (currentRecords != null)
                                await _servicePlusContext.EnclosureDetails.AddRangeAsync(currentRecords);
                            else
                                _logger.LogInformation("Null Check Insertion enclosureDetailsList  " + currentRecords);

                        }

                    }

                    // Use AddRangeAsync to add the collection of attributeDetailList in batches
                    if (attributeDetailList != null)
                    {
                        int batchSize = 500;
                        int totalRecords = attributeDetailList.Count;
                        int iterations = (int)Math.Ceiling((double)totalRecords / batchSize);

                        for (int i = 0; i < iterations; i++)
                        {
                            var currentRecords = attributeDetailList.Skip(i * batchSize).Take(batchSize);
                            if (currentRecords != null)
                                await _servicePlusContext.AttributeDetails.AddRangeAsync(currentRecords);
                            else
                                _logger.LogInformation("Null Check Insertion attributeDetailList  " + currentRecords);

                        }

                    }

                    // Use AddRangeAsync to add the collection of executionDataList in batches
                    if (executionData != null)
                    {
                        int batchSize = 500;
                        int totalRecords = executionData.Count;
                        int iterations = (int)Math.Ceiling((double)totalRecords / batchSize);

                        for (int i = 0; i < iterations; i++)
                        {
                            var currentRecords = executionData.Skip(i * batchSize).Take(batchSize);
                            if (currentRecords != null)
                                await _servicePlusContext.ExecutionDatas.AddRangeAsync(currentRecords);
                            else
                                _logger.LogInformation("Null Check Insertion executionData  " + currentRecords);

                        }

                    }

                    // Use AddRangeAsync to add the collection of taskDetailsList in batches
                    if (taskDetailsList != null)
                    {
                        int batchSize = 500;
                        int totalRecords = taskDetailsList.Count;
                        int iterations = (int)Math.Ceiling((double)totalRecords / batchSize);

                        for (int i = 0; i < iterations; i++)
                        {
                            var currentRecords = taskDetailsList.Skip(i * batchSize).Take(batchSize);
                            if (currentRecords != null)
                                await _servicePlusContext.TaskDetails.AddRangeAsync(currentRecords);

                            else
                                _logger.LogInformation("Null Check Insertion taskDetailsList  " + currentRecords);
                        }

                    }

                    // Use AddRangeAsync to add the collection of userDetails in batches
                    if (userDetails != null)
                    {
                        int batchSize = 500;
                        int totalRecords = userDetails.Count;
                        int iterations = (int)Math.Ceiling((double)totalRecords / batchSize);

                        int totalRecordTaskDetail = taskDetailsList.Count;
                        int taskDetailIteration = (int)Math.Ceiling((double)totalRecordTaskDetail / batchSize);


                        for (int i = 0; i < iterations; i++)
                        {


                            var currentRecords = userDetails.Skip(i * batchSize).Take(batchSize).ToList();
                            List<TaskDetail> currentTaskDetaikRecords = taskDetailsList.Skip(i * batchSize).Take(batchSize).ToList();
                            var modifiedRecords = new List<UserDetail>();

                            int j = 0;
                            foreach (var userDetail in currentRecords)
                            {

                                UserDetail userDetail1;
                                if (userDetail == null)
                                {
                                    // Assign custom values when UserDetail is null
                                    var tempUserDetail = new UserDetail
                                    {
                                        UserName = "NoDataFlag",
                                        DepartmentLevel = "NoDataFlag",
                                        CurrentProcessId = 1,
                                        Designation = "NoDataFlag",
                                        LocationId = "1",
                                        LocationName = "NoDataFlag",
                                        LocationTypeId = "1",
                                        PullUserId = 1
                                    };

                                    // Replace the null user_detail with the custom value
                                    userDetail1 = tempUserDetail;
                                }
                                else
                                {
                                    userDetail1 = userDetail;
                                }

                                // Refer the corresponding TaskDetail to UserDetails
                                if (j < currentTaskDetaikRecords.Count && currentTaskDetaikRecords[j] != null)
                                {
                                    userDetail1.TaskDetail = currentTaskDetaikRecords[j];
                                }

                                modifiedRecords.Add(userDetail1);
                                j++;
                            }

                            await _servicePlusContext.UserDetails.AddRangeAsync(modifiedRecords);

                        }


                    }

                    // Use AddRangeAsync to add the collection of officialFormDetails in batches
                    if (officialFormDetails != null)
                    {
                        int batchSize = 500;
                        int totalRecords = officialFormDetails.Count;
                        int iterations = (int)Math.Ceiling((double)totalRecords / batchSize);

                        for (int i = 0; i < iterations; i++)
                        {

                            var currentRecords = officialFormDetails.Skip(i * batchSize).Take(batchSize);
                            if (currentRecords != null)
                                await _servicePlusContext.OfficialFormDetails.AddRangeAsync(currentRecords);


                            else
                                _logger.LogInformation("Null Check Insertion officialFormDetails  " + currentRecords);

                        }

                    }


                    DateTime jsonRecievedDateTime = DateTime.Now;
                    DateTime jsonUTCDateTime = jsonRecievedDateTime.ToUniversalTime(); // Convert to UTC
                    CultureInfo jsonHiIN = new CultureInfo("en-IN");
                    DateTime jsonHiINDateTime = Convert.ToDateTime(utcDateTime, hiIN);

                    var dateOfJsonReceived = jsonHiINDateTime;

                    var jsonReceived = JSONReceivedMappingServicePlus(dateOfJsonReceived, initiatedList, executionList);

                    await _servicePlusContext.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    _logger.LogInformation("Logs info---Exception" + ex);

                    return BadRequest(ex);
                }


                return Ok("Initiated Record " + initiatedList + " " + "Execution Record " + executionList + " " + "Records Succesfully Added in DataBase");

            }
            else
            {
                return BadRequest("Initiated Record " + initiatedList + " " + "Execution Record " + executionList + " " + "Records Succesfully Added in DataBase");

            }
        }
        #endregion

        #region Income Certificate Verification
        [HttpGet]
        [Route("IncomeCertificateVerification")]
        public async Task<IActionResult> IncomeCertificateVerification(string applRefNo)
        {
            string applicantNameId = "158605";
            string applicantFatherNameId = "158604";
            string applicantMobileNumberId = "158615";
            string applicantDobId = "158614";
            string applicantGenderId = "158608";
            var initiatedAppId = _postgresDbContext.InitiatedDatas.Include(d => d.AttributeDetail)
                .Where(d => d.ApplRefNo == applRefNo)
                .OrderByDescending(d => d.SubmissionDate).Select(d => new
                {
                    d.ApplId,
                    applicantName = d.AttributeDetail.Where(d => d.ApplicationFormFieldID == applicantNameId).Select(d => d.ApplicationFormFieldValue).FirstOrDefault(),
                    applicantFatherName = d.AttributeDetail.Where(d => d.ApplicationFormFieldID == applicantFatherNameId).Select(d => d.ApplicationFormFieldValue).FirstOrDefault(),
                    applicantMobileNumber = d.AttributeDetail.Where(d => d.ApplicationFormFieldID == applicantMobileNumberId).Select(d => d.ApplicationFormFieldValue).FirstOrDefault(),
                    applicantDob = d.AttributeDetail.Where(d => d.ApplicationFormFieldID == applicantDobId).Select(d => d.ApplicationFormFieldValue).FirstOrDefault(),
                    applicantGender = d.AttributeDetail.Where(d => d.ApplicationFormFieldID == applicantGenderId).Select(d => d.ApplicationFormFieldValue).FirstOrDefault(),

                })
                .FirstOrDefault();
            if (initiatedAppId != null)
            {
                var officialForm = _postgresDbContext.ExecutionDatas
                    .Include(d => d.OfficialFormDetail)
                    .Include(d => d.TaskDetails)
                    .Where(d => d.TaskDetails.ApplId == initiatedAppId.ApplId)
                    .OrderByDescending(d => d.TaskDetails.ReceivedTime)
                    .Select(d => d.OfficialFormDetail)
                    .FirstOrDefault();

                bool isDelivered = false;

                if (officialForm != null)
                {
                    foreach (var item in officialForm)
                    {
                        if (item.OfficalFormValue.Contains("11~"))
                        {
                            isDelivered = true;
                            break; // Exit the loop if "11~" is found in any item.
                        }
                    }
                }
                if (isDelivered)
                {
                    string genderVal = initiatedAppId.applicantGender?.Split('/').FirstOrDefault()?.Trim();
                    string gender = genderVal?.Split('~').LastOrDefault();

                    var response = new
                    {
                        applicantRecord = new
                        {
                            ApplicantName = initiatedAppId.applicantName,
                            ApplicantFatherName = initiatedAppId.applicantFatherName,
                            ApplicantMobileNumber = initiatedAppId.applicantMobileNumber,
                            ApplicantDob = initiatedAppId.applicantDob,
                            ApplicantGender = gender, // Use the processed 'gender' value.
                            CertificateValid = isDelivered
                        }
                    };
                    return Ok(response);
                }
                else
                {
                    return NotFound(new Response { Status = "Resource Not Found", Message = "No data found for the provided AppRefNo" });
                }
            }
            else
            {
                return NotFound(new Response { Status = "Resource Not Found", Message = "No data found for the provided AppRefNo" });

            }
        }

        #endregion

        #region UpdatePendencyReport
        [CustomAuthorizeAttribute]
        [HttpPost]
        [Route("UpdatePendencyReport")]
        public async Task<IActionResult> UpdatePendencyReport()
        {
            var pageSize = 5000; // Adjust the batch size as needed
            var appIds = await _postgresDbContext.TaskDetails.Select(d => d.ApplId).Distinct().ToListAsync();
            _logger.LogInformation("Total Applications " + appIds);
            var latestRecords = new List<ExecutionData>();
            LoadDistrictNameMap();

            for (int page = 0; page < Math.Ceiling((double)appIds.Count / pageSize); page++)
            {
                var pageAppIDs = appIds.Skip(page * pageSize).Take(pageSize);
                var pageCount = Math.Ceiling((double)appIds.Count / pageSize);
                var currentPage = page;
                var latestRecord = await _postgresDbContext.ExecutionDatas
                                   .Include(d => d.TaskDetails.UserDetail)
                                   .Include(d => d.OfficialFormDetail)
                                   .Where(d => pageAppIDs.Contains(d.TaskDetails.ApplId))
                                   .GroupBy(d => d.TaskDetails.ApplId)
                                   .Select(group => group.OrderByDescending(d => d.TaskDetails.ReceivedTime)
                                   .FirstOrDefault()).ToListAsync();
                if (latestRecord != null)
                {
                    latestRecords.AddRange(latestRecord);
                }
                else
                {
                    _logger.LogInformation("Record Null " + latestRecord);
                }


            }

            var pendencyReport = new List<PendencyReportViewModel>();

            foreach (var group in latestRecords
                .Where(record => record.TaskDetails.UserDetail.LocationId != "1")
                .GroupBy(record =>
                {
                    var customDistrictIdParts = record.TaskDetails.UserDetail.LocationId.Split(',');
                    var firstCustomDistrictValue = customDistrictIdParts[0];

                    if (districtNameMap.TryGetValue(firstCustomDistrictValue, out var customDistrictName))
                    {
                        return customDistrictName;
                    }

                    return "Unknown District";
                }))
            {
                var report = CalculatePendencyReport(group);
                pendencyReport.Add(report);
            }

            var mappedPendingReport = _mapper.Map<List<PendencyReportViewModel>, List<PendencyReport>>(pendencyReport);

            foreach (var item in mappedPendingReport)
            {
                await UpdateOrAddPendencyReportAsync(item);
            }

            return Ok(mappedPendingReport);
        }


        #region PendncyReport Calculations
        private PendencyReportViewModel CalculatePendencyReport(IGrouping<string, ExecutionData> group)
        {
            int deliveredCount = 0;
            int rejectedCount = 0;
            int inProcessCount = 0;
            int day1to5Count = 0;
            int day6to30Count = 0;
            int day31to60Count = 0;
            int day61to90Count = 0;
            int day91toAboveCount = 0;
            int sendBackCount = 0;
            int totalPendingDays = 0;

            foreach (var record in group)
            {
                if (record.TaskDetails != null)
                {
                    DateTime currentDate = DateTime.Today;
                    DateTime receivedDate = (DateTime)record.TaskDetails.ReceivedTime;

                    DateTime dueDate = receivedDate.AddDays(3);
                    TimeSpan difference = currentDate - dueDate;
                    int daysDifference = (int)difference.TotalDays;
                    bool isDelivered = record.OfficialFormDetail.Any(d => IsDeliverd(d.OfficalFormValue));
                    bool isRejected = record.OfficialFormDetail.Any(d => IsRejected(d.OfficalFormValue));
                    bool isInProcess = record.OfficialFormDetail.Any(d => IsInProcess(d.OfficalFormValue));
                    bool isSendBack = record.OfficialFormDetail.Any(d => IsSendBack(d.OfficalFormValue));

                    if (isDelivered)
                    {
                        deliveredCount++;
                    }
                    else if (isRejected)
                    {
                        rejectedCount++;
                    }
                    else if (isInProcess)
                    {
                        inProcessCount++;

                        if (daysDifference >= 1 && daysDifference <= 5)
                        {
                            day1to5Count++;
                        }
                        else if (daysDifference >= 6 && daysDifference <= 30)
                        {
                            day6to30Count++;
                        }
                        else if (daysDifference >= 31 && daysDifference <= 60)
                        {
                            day31to60Count++;
                        }
                        else if (daysDifference >= 61 && daysDifference <= 90)
                        {
                            day61to90Count++;
                        }
                        else if (daysDifference >= 91)
                        {
                            day91toAboveCount++;
                        }
                    }
                    else if (isSendBack)
                    {
                        sendBackCount++;
                    }
                    else
                    {
                        _logger.LogInformation("Not Exist with ExecutionDataId: " + record.ExecutionDataId);
                    }
                }
                else
                {
                    _logger.LogInformation("Error occurred for record with ExecutionDataId: " + record.ExecutionDataId);
                }
            }

            totalPendingDays = day1to5Count + day6to30Count + day31to60Count + day61to90Count + day91toAboveCount;

            int totalApplicationsReceived = group.Count();
            double pendencyPercentage = totalApplicationsReceived > 0 ? (double)totalPendingDays / totalApplicationsReceived * 100 : 0;

            if (pendencyPercentage - Math.Floor(pendencyPercentage) > 0.05)
            {
                pendencyPercentage = Math.Ceiling(pendencyPercentage * 100 + 1) / 100;
            }
            else
            {
                pendencyPercentage = Math.Floor(pendencyPercentage * 100) / 100;
            }
            return new PendencyReportViewModel
            {
                DistrictName = group.Key,
                ApplicationRecieved = totalApplicationsReceived,
                Deliverd = deliveredCount,
                Rejected = rejectedCount,
                InProcess = inProcessCount,
                Day1to5 = day1to5Count,
                Day6to30 = day6to30Count,
                Day31to60 = day31to60Count,
                Day61to90 = day61to90Count,
                Day91toAbove = day91toAboveCount,
                SendBack = sendBackCount,
                TotalPendingDays = totalPendingDays,
                PendencyPercentage = pendencyPercentage
            };
        }

        private bool IsDeliverd(string officalFormValue)
        {
            return officalFormValue.Contains("11~");
        }

        private bool IsRejected(string officalFormValue)
        {
            return officalFormValue.Contains("10~");
        }

        private bool IsInProcess(string officalFormValue)
        {
            return officalFormValue.Contains("9~") || officalFormValue.Contains("22544~") || officalFormValue.Contains("22340~");
        }


        private bool IsSendBack(string officalFormValue)
        {
            return officalFormValue.Contains("34~") || officalFormValue.Contains("20~");
        }

        #endregion

        #region Pendency Report Updation
        private async Task UpdateOrAddPendencyReportAsync(PendencyReport pendencyReport)
        {
            var existingPendencyReport = await _postgresDbContext.PendencyReport.FirstOrDefaultAsync(d => d.DistrictName == pendencyReport.DistrictName);
            if (existingPendencyReport != null)
            {
                existingPendencyReport.ApplicationRecieved = pendencyReport.ApplicationRecieved;
                existingPendencyReport.Deliverd = pendencyReport.Deliverd;
                existingPendencyReport.Rejected = pendencyReport.Rejected;
                existingPendencyReport.InProcess = pendencyReport.InProcess;
                existingPendencyReport.Day1to5 = pendencyReport.Day1to5;
                existingPendencyReport.Day6to30 = pendencyReport.Day6to30;
                existingPendencyReport.Day31to60 = pendencyReport.Day31to60;
                existingPendencyReport.Day61to90 = pendencyReport.Day61to90;
                existingPendencyReport.Day91toAbove = pendencyReport.Day91toAbove;
                existingPendencyReport.SendBack = pendencyReport.SendBack;
                existingPendencyReport.TotalPendingDays = pendencyReport.TotalPendingDays;
                existingPendencyReport.PendencyPercentage = pendencyReport.PendencyPercentage;

                _postgresDbContext.PendencyReport.Update(existingPendencyReport);
            }
            else
            {
                _postgresDbContext.PendencyReport.Add(pendencyReport);
            }
            _postgresDbContext.SaveChanges();
        }

        #endregion

        #endregion

        #region LoadDistrictNameMap
        private void LoadDistrictNameMap()
        {
            var locationIds = _postgresDbContext.CustomLGDTehsilSubTehsils.Select(d => d.CustomLGDTehsilSubTehsilCode).Distinct().ToList();
            foreach (var locationId in locationIds)
            {
                var customDistrictId = _postgresDbContext.CustomLGDTehsilSubTehsils
                    .Where(d => d.CustomLGDTehsilSubTehsilCode == locationId)
                    .Select(d => d.CustomLGDDistrictId)
                    .FirstOrDefault();

                var customDistrictName = _postgresDbContext.CustomLGDDistricts
                    .Where(d => d.CustomLGDDistrictId == customDistrictId)
                    .Select(d => d.CustomLGDDDistrictName)
                    .FirstOrDefault();

                districtNameMap[locationId] = customDistrictName;
            }
        }
        #endregion         

        #region Get Pendency Report
        [CustomAuthorizeAttribute]
        [HttpGet]
        [Route("PendencyReport")]
        public async Task<IActionResult> PendencyReport()
        {
            var pendencyReport = await _postgresDbContext.PendencyReport.OrderBy(d => d.DistrictName).ToListAsync();
            return Ok(pendencyReport);
        }
        #endregion

        #region JSONReceivedDates
        [CustomAuthorizeAttribute]
        [HttpGet]
        [Route("JSONReceivedDates")]
        public async Task<IActionResult> JSONReceivedDates(int page, int pageSize)
        {
            var totalCount = await _postgresDbContext.JSONReceived.CountAsync();

            var jsonReceived = await _postgresDbContext.JSONReceived.OrderByDescending(d => d.JsonReceivedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var mappedJSONReceived = _mapper.Map<List<JSONReceived>, List<JSONReceivedViewModel>>(jsonReceived);


            var response = new
            {
                recordsTotal = totalCount,
                recordsFiltered = totalCount,
                data = mappedJSONReceived
            };

            return Ok(response);
        }

        private async Task<JSONReceived> JSONReceivedMapping(DateTime dateOfJsonReceived, int initiated, int execution)
        {
            JSONReceived jsonReceived = new JSONReceived()
            {
                JsonReceivedDate = dateOfJsonReceived,
                ReceivedInititatedRecord = initiated,
                ReceivedExecutionRecord = execution

            };
            _postgresDbContext.JSONReceived.Add(jsonReceived);

            return jsonReceived;
        }
        private async Task<JSONReceived> JSONReceivedMappingServicePlus(DateTime dateOfJsonReceived, int initiated, int execution)
        {
            JSONReceived jsonReceived = new JSONReceived()
            {
                JsonReceivedDate = dateOfJsonReceived,
                ReceivedInititatedRecord = initiated,
                ReceivedExecutionRecord = execution

            };
            _servicePlusContext.JSONReceived.Add(jsonReceived);

            return jsonReceived;
        }


        #endregion

        #region DeleteInitExecById
        [CustomAuthorizeAttribute]
        [Route("DeleteInitExecById")]
        [HttpDelete]
        public async Task<IActionResult> DeleteInitExecRecord(int appId)
        {
            var deleteInitiatedRecord = _servicePlusContext.InitiatedDatas.Include(d => d.EnclosureDetails).Include(d => d.AttributeDetail).Where(d => d.ApplId == appId).FirstOrDefault();
            var deleteExecutionRecord = _servicePlusContext.ExecutionDatas.Include(d => d.OfficialFormDetail).Include(d => d.TaskDetails).Include(d => d.TaskDetails.UserDetail).Where(d => d.TaskDetails.ApplId == appId).FirstOrDefault();
            if (deleteInitiatedRecord != null)
            {
                _servicePlusContext.EnclosureDetails.RemoveRange(deleteInitiatedRecord.EnclosureDetails);
                _servicePlusContext.AttributeDetails.RemoveRange(deleteInitiatedRecord.AttributeDetail);

                _servicePlusContext.InitiatedDatas.Remove(deleteInitiatedRecord);

                await _servicePlusContext.SaveChangesAsync();
            }
            if (deleteExecutionRecord != null)
            {
                _servicePlusContext.OfficialFormDetails.RemoveRange(deleteExecutionRecord.OfficialFormDetail);
                _servicePlusContext.TaskDetails.RemoveRange(deleteExecutionRecord.TaskDetails);
                _servicePlusContext.UserDetails.RemoveRange(deleteExecutionRecord.TaskDetails.UserDetail);

                _servicePlusContext.ExecutionDatas.Remove(deleteExecutionRecord);

                await _servicePlusContext.SaveChangesAsync();
            }

            return Ok();
        }

        #endregion

        #region Get Sewa Kendra Wise Report
        [CustomAuthorizeAttribute]
        [HttpGet]
        [Route("GetSewaKendraWiseReport")]
        public async Task<IActionResult> GetSewaKendraWiseReport(int draw, int start, int length, string? searchValue, string? sortOrder, DateTime? fromDate, DateTime? toDate)
        {
            // Ensure that 'start' and 'length' values are within reasonable limits
            if (start < 1)
            {
                start = 1;
            }

            if (length < 1)
            {
                length = 10; // Set a default page size
            }
            var fromDateParsed = ConvertToUtc(fromDate);
            var toDateParsed = ConvertToUtc(toDate);
            List<int?> initAppIds = new List<int?>();
            int initAppIdsTotalCount;


            // Adjust the batch size as needed
            if (fromDate != null && toDate != null)
            {
                initAppIdsTotalCount = _postgresDbContext.InitiatedDatas
                .Where(d => d.SubmissionMode == "kiosk" && d.VersionNo != "1" && d.VersionNo != "2" && d.PaymentDate >= fromDateParsed && d.PaymentDate <= toDateParsed)
                .Select(d => d.ApplId).Distinct().Count();

                initAppIds = _postgresDbContext.InitiatedDatas
                .Where(d => d.SubmissionMode == "kiosk" && d.VersionNo != "1" && d.VersionNo != "2" && d.PaymentDate >= fromDateParsed && d.PaymentDate <= toDateParsed)
                .Select(d => d.ApplId).Distinct().Skip((start - 1) * length)
                .Take(length).ToList();

            }
            else
            {
                initAppIdsTotalCount = _postgresDbContext.InitiatedDatas
                .Where(d => d.SubmissionMode == "kiosk" && d.VersionNo != "1" && d.VersionNo != "2").Select(d => d.ApplId).Distinct().Count();

                initAppIds = _postgresDbContext.InitiatedDatas
                .Where(d => d.SubmissionMode == "kiosk" && d.VersionNo != "1" && d.VersionNo != "2")
                .Select(d => d.ApplId).Distinct().Skip((start - 1) * length)
                .Take(length).ToList();
            }

            var initiatedRecords = await _postgresDbContext.InitiatedDatas.Where(d => initAppIds.Contains(d.ApplId))
                                    .Include(d => d.AttributeDetail).GroupBy(d => d.ApplId)
                                    .Select(group => group.OrderByDescending(d => d.SubmissionDate).FirstOrDefault())
                                    .ToListAsync();

            var customAttributeDetail = _postgresDbContext.CustomAttributeLabel.ToList();

            List<SewaKendraWiseReport> sewaKendraWiseReports = GenerateSewaKendraReports(initiatedRecords, customAttributeDetail, searchValue, sortOrder);
            // Assuming you want to do something with the 'reports' list here
            var response = new
            {
                draw = draw,
                recordsTotal = initAppIdsTotalCount,
                recordsFiltered = initAppIdsTotalCount, // Initially, you can consider all records as filtered
                data = sewaKendraWiseReports
            };

            return Ok(response);
        }

        private List<SewaKendraWiseReport> GenerateSewaKendraReports(List<InitiatedData> initDataList, List<CustomAttributeLabel> customAttributeLabels, string searchValue, string sortOrder)
        {
            List<SewaKendraWiseReport> reports = new List<SewaKendraWiseReport>();

            foreach (InitiatedData initData in initDataList)
            {
                SewaKendraWiseReport report = GenerateSewaKendraReport(initData, customAttributeLabels);
                reports.Add(report);
            }

            // Apply sorting based on sortOrder
            switch (sortOrder)
            {
                case "zone_asc":
                    reports = reports.OrderBy(r => r.Zone).ToList();
                    break;
                case "zone_desc":
                    reports = reports.OrderByDescending(r => r.Zone).ToList();
                    break;
                case "district_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "district_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "SewakendraType_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "SewakendraType_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "DepartmentName_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "DepartmentName_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "ApplicationRefNumber_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "ApplicationRefNumber_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "ServiceName_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "ServiceName_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "CitizenName_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "CitizenName_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "CitizenContactNumber_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "CitizenContactNumber_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "FaciliationCharges_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "FaciliationCharges_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "PaymentDateTime_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "PaymentDateTime_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;


                default:
                    // Default sorting by a specific column or keep the original order.
                    break;
            }

            // Apply search filtering
            if (!string.IsNullOrEmpty(searchValue))
            {
                searchValue = searchValue.ToLower(); // Convert to lowercase for case-insensitive search
                reports = reports.Where(d =>
                    d.Zone.ToLower().Contains(searchValue) ||
                    d.District.ToLower().Contains(searchValue) ||
                    d.CitizenContactNumber.ToLower().Contains(searchValue) ||
                    d.ApplicationRefNumber.ToLower().Contains(searchValue) ||
                    d.SewakendraType.ToLower().Contains(searchValue) ||
                    d.SewaKendraName.ToLower().Contains(searchValue) ||
                    d.SewaKendraCode.ToLower().Contains(searchValue) ||
                    d.CitizenName.ToLower().Contains(searchValue)
                    )
                    .ToList();
            }

            return reports;
        }


        private SewaKendraWiseReport GenerateSewaKendraReport(InitiatedData initData, List<CustomAttributeLabel> customAttributeLabels)
        {
            SewaKendraWiseReport report = new SewaKendraWiseReport
            {
                Zone = GetAttributeValue(initData.AttributeDetail, customAttributeLabels, "Zone"),
                District = GetAttributeValue(initData.AttributeDetail, customAttributeLabels, "District"),
                SewakendraType = GetAttributeValue(initData.AttributeDetail, customAttributeLabels, "Sewa Kendra Type"),
                SewaKendraName = GetAttributeValue(initData.AttributeDetail, customAttributeLabels, "Sewa Kendra Name"),
                SewaKendraCode = GetAttributeValue(initData.AttributeDetail, customAttributeLabels, "Sewa Kendra Code"),
                DepartmentName = initData.DepartmentName,
                ServiceName = initData.ServiceName,
                ApplicationRefNumber = initData.ApplRefNo,
                CitizenName = GetAttributeValue(initData.AttributeDetail, customAttributeLabels, "Applicant Name"),
                CitizenContactNumber = GetAttributeValue(initData.AttributeDetail, customAttributeLabels, "Mobile Number"),
                FaciliationCharges = initData.Amount,
                PaymentDateTime = initData.PaymentDate


                // Other mappings...
            };

            return report;
        }

        private string? GetAttributeValue(List<AttributeDetail> attributeDetails, List<CustomAttributeLabel> customAttributeLabels, string targetAttributeName)
        {
            var applicationFormId = customAttributeLabels.Where(d => d.ApplicationFormLabel == targetAttributeName).FirstOrDefault();

            if (applicationFormId == null)
            {
                return null; // Attribute not found
            }

            foreach (AttributeDetail attributeDetail in attributeDetails)
            {
                if (attributeDetail.ApplicationFormFieldID == applicationFormId.ApplicationFormId.ToString())
                {
                    return attributeDetail.ApplicationFormFieldValue;
                }
            }

            return null; // Return null if no match is found
        }

        #endregion

        #region Get Sewa Kendra Zone Wise Report
        [CustomAuthorizeAttribute]
        [HttpGet]
        [Route("GetSewaKendraZoneWiseReport")]
        public async Task<IActionResult> GetSewaKendraZoneWiseReport(int draw, int start, int length, string? searchValue, string? sortOrder, DateTime? fromDate, DateTime? toDate, string? zoneType)
        {
            // Ensure that 'start' and 'length' values are within reasonable limits
            if (start < 1)
            {
                start = 1;
            }

            if (length < 1)
            {
                length = 10; // Set a default page size
            }
            var fromDateParsed = ConvertToUtc(fromDate);
            var toDateParsed = ConvertToUtc(toDate);
            List<int?> initAppIds = new List<int?>();
            int initAppIdsTotalCount;
            // Adjust the batch size as needed
            if (fromDate != null && toDate != null)
            {

                initAppIdsTotalCount = _postgresDbContext.InitiatedDatas
                .Where(d => d.SubmissionMode == "kiosk" && d.VersionNo != "1" && d.VersionNo != "2" && d.PaymentDate >= fromDateParsed && d.PaymentDate <= toDateParsed && d.AttributeDetail.Any(d => d.ApplicationFormFieldValue == zoneType)).Include(d => d.AttributeDetail).Select(d => d.ApplId).Distinct().Count();

                initAppIds = _postgresDbContext.InitiatedDatas
                .Where(d => d.SubmissionMode == "kiosk" && d.VersionNo != "1" && d.VersionNo != "2" && d.PaymentDate >= fromDateParsed && d.PaymentDate <= toDateParsed && d.AttributeDetail.Any(d => d.ApplicationFormFieldValue == zoneType)).Include(d => d.AttributeDetail)
                .Select(d => d.ApplId).Distinct().Skip((start - 1) * length)
                .Take(length).ToList();

            }
            else
            {
                initAppIdsTotalCount = _postgresDbContext.InitiatedDatas
                .Where(d => d.SubmissionMode == "kiosk" && d.VersionNo != "1" && d.VersionNo != "2" && d.AttributeDetail.Any(d => d.ApplicationFormFieldValue == zoneType)).Include(d => d.AttributeDetail).Select(d => d.ApplId).Distinct().Count();

                initAppIds = _postgresDbContext.InitiatedDatas
                .Where(d => d.SubmissionMode == "kiosk" && d.VersionNo != "1" && d.VersionNo != "2" && d.AttributeDetail.Any(d => d.ApplicationFormFieldValue == zoneType)).Include(d => d.AttributeDetail)
                .Select(d => d.ApplId).Distinct().Skip((start - 1) * length)
                .Take(length).ToList();
            }

            var initiatedRecords = await _postgresDbContext.InitiatedDatas.OrderByDescending(d => d.SubmissionDate).Where(d => initAppIds.Contains(d.ApplId))
                                    .Include(d => d.AttributeDetail).GroupBy(d => d.ApplId)
                                    .Select(group => group.OrderByDescending(d => d.SubmissionDate).FirstOrDefault())
                                    .ToListAsync();

            var customAttributeDetail = _postgresDbContext.CustomAttributeLabel.ToList();

            List<SewaKendraZoneWiseReport> sewaKendraWiseReports = GenerateSewaKendraZoneWiseReports(initiatedRecords, customAttributeDetail, searchValue, sortOrder);
            // Assuming you want to do something with the 'reports' list here
            var response = new
            {
                draw = draw,
                recordsTotal = initAppIdsTotalCount,
                recordsFiltered = initAppIdsTotalCount, // Initially, you can consider all records as filtered
                data = sewaKendraWiseReports
            };

            return Ok(response);
        }

        private List<SewaKendraZoneWiseReport> GenerateSewaKendraZoneWiseReports(List<InitiatedData> initDataList, List<CustomAttributeLabel> customAttributeLabels, string searchValue, string sortOrder)
        {
            List<SewaKendraZoneWiseReport> reports = new List<SewaKendraZoneWiseReport>();

            foreach (InitiatedData initData in initDataList)
            {
                SewaKendraZoneWiseReport report = GenerateSewaKendraZoneWiseReport(initData, customAttributeLabels);
                reports.Add(report);
            }

            // Apply sorting based on sortOrder
            switch (sortOrder)
            {
                case "zone_asc":
                    reports = reports.OrderBy(r => r.Zone).ToList();
                    break;
                case "zone_desc":
                    reports = reports.OrderByDescending(r => r.Zone).ToList();
                    break;
                case "district_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "district_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "SewakendraType_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "SewakendraType_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "DepartmentName_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "DepartmentName_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "ApplicationRefNumber_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "ApplicationRefNumber_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "ServiceName_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "ServiceName_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "CitizenName_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "CitizenName_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "CitizenContactNumber_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "CitizenContactNumber_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "FaciliationCharges_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "FaciliationCharges_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;
                case "PaymentDateTime_asc":
                    reports = reports.OrderBy(r => r.District).ToList();
                    break;
                case "PaymentDateTime_desc":
                    reports = reports.OrderByDescending(r => r.District).ToList();
                    break;


                default:
                    // Default sorting by a specific column or keep the original order.
                    break;
            }

            // Apply search filtering
            if (!string.IsNullOrEmpty(searchValue))
            {
                searchValue = searchValue.ToLower(); // Convert to lowercase for case-insensitive search
                reports = reports.Where(d =>
                    d.Zone.ToLower().Contains(searchValue) ||
                    d.District.ToLower().Contains(searchValue) ||
                    d.SewaKendraOperatorEmail.ToLower().Contains(searchValue) ||
                    d.ApplicationRefNumber.ToLower().Contains(searchValue) ||
                    d.SewakendraOperatorName.ToLower().Contains(searchValue) ||
                    d.SewaKendraName.ToLower().Contains(searchValue) ||
                    d.SewaKendraOperatorMobile.ToLower().Contains(searchValue)
                    )
                    .ToList();
            }

            return reports;
        }

        private SewaKendraZoneWiseReport GenerateSewaKendraZoneWiseReport(InitiatedData initData, List<CustomAttributeLabel> customAttributeLabels)
        {
            SewaKendraZoneWiseReport report = new SewaKendraZoneWiseReport
            {
                ApplicationRefNumber = initData.ApplRefNo,
                Zone = GetAttributeValue(initData.AttributeDetail, customAttributeLabels, "Zone"),
                District = GetAttributeValue(initData.AttributeDetail, customAttributeLabels, "District"),
                SewaKendraName = GetAttributeValue(initData.AttributeDetail, customAttributeLabels, "Sewa Kendra Name"),
                SewakendraOperatorName = GetAttributeValue(initData.AttributeDetail, customAttributeLabels, "Sewa Kendra Operator Name"),
                SewaKendraOperatorEmail = GetAttributeValue(initData.AttributeDetail, customAttributeLabels, "Sewa Kendra Operator Email"),
                SewaKendraOperatorMobile = GetAttributeValue(initData.AttributeDetail, customAttributeLabels, "Sewa Kendra Operator Mobile"),
                FaciliationCharges = initData.Amount


                // Other mappings...
            };

            return report;
        }

        #endregion

        #region Map ApplicationForms

        [Route("MapApplicationFormFields")]
        [HttpPost]
        public async Task<IActionResult> MapApplicationFormFields(List<CustomAttributeLabel> customAttributeLabel)
        {
            // Convert the incoming list to a separate list to work with.
            var listOfCustomAttributes = customAttributeLabel.ToList();

            foreach (var customAttribute in listOfCustomAttributes)
            {
                // Check if a record with the same CustomAttributLabelId exists in the database.
                var existingCustomAttribute = _postgresDbContext.CustomAttributeLabel.Where(d => d.ApplicationFormId == customAttribute.ApplicationFormId).FirstOrDefault();

                if (existingCustomAttribute == null)
                {
                    // The record does not exist, so add it.
                    _postgresDbContext.CustomAttributeLabel.Add(customAttribute);
                }
                else
                {
                    // The record already exists, you can choose to update it or handle it as needed.
                    // For now, we skip adding a duplicate record.
                    return StatusCode(StatusCodes.Status302Found, new Response { Status = "Aleady Exist", Message = "Attribute Id already Exist please enter different Attribute Id" });
                }
            }

            // Save changes to the database.
            _postgresDbContext.SaveChanges();

            return Ok();
        }


        #endregion

        #region MonthWiseData
        // [CustomAuthorizeAttribute]
        [Route("MonthWiseData")]
        [HttpPost]
        public async Task<IActionResult> MonthWiseData(DateTime fromDate, DateTime toDate, string serviceName)
        {
            // Convert the input dates to UTC format
            var utcFromDate = fromDate.ToUniversalTime();
            var utcToDate = toDate.ToUniversalTime();

            var filteredInitiatedData = _postgresDbContext.InitiatedDatas
                .Where(d => d.SubmissionDate >= utcFromDate && d.SubmissionDate <= utcToDate && d.ServiceName == serviceName)
                .GroupBy(d => d.ApplId).Select(group => group.Key)
                .ToList();

            var filteredExecutionData = _postgresDbContext.TaskDetails
               .Where(d => d.ReceivedTime >= utcFromDate && d.ReceivedTime <= utcToDate)
               .GroupBy(d => d.ApplId).Select(group => group.Key)
               .ToList();

            return Ok("initiatedData " + filteredInitiatedData.Count + " Execution " + filteredExecutionData.Count);
        }


        #endregion

        #region Update API Names & Get API Names
        [CustomAuthorizeAttribute]
        [Route("UpdateApiNames")]
        [HttpGet]
        public async Task<IActionResult> UpdateApiNames()
        {
            // Step 1: Delete all existing records related to API names

            var assembly = Assembly.GetExecutingAssembly();
            var controllerTypes = assembly.GetTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t));

            List<ApiNames> apiNames = new List<ApiNames>();

            foreach (var controllerType in controllerTypes)
            {
                var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                foreach (var method in methods)
                {
                    // Check if the method has an actual implementation by examining its IL code
                    if (method.DeclaringType == controllerType)
                    {
                        var controllerName = controllerType.Name;
                        if (controllerName == "ServicePlusController")
                        {


                            // Check if the apiName already exists in the database
                            var existingApiName = _servicePlusContext.ApiNames.FirstOrDefault(an => an.ApiName == method.Name);
                            if (existingApiName == null)
                            {
                                ApiNames apiName = new ApiNames()
                                {
                                    ApiName = method.Name
                                };
                                apiNames.Add(apiName);
                            }
                        }
                    }
                }
            }
            if (apiNames.Count <= 0)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "No Content", Message = "No New API Found" });

            }
            else
            {
                // Step 3: Add only the new apiNames to the database
                _servicePlusContext.ApiNames.AddRange(apiNames);
                _servicePlusContext.SaveChanges();
            }
            return Ok();
        }



        [CustomAuthorizeAttribute]
        [Route("GetApiNames")]
        [HttpGet]
        public async Task<IActionResult> GetApiNames()
        {
            var getApiNames = await _servicePlusContext.ApiNames.ToListAsync();
            return Ok(getApiNames);
        }

        [CustomAuthorizeAttribute]
        [Route("AddEditApiDescription")]
        [HttpPost]
        public async Task<IActionResult> AddApiDescription(ApiNameViewModel apiViewModel)
        {
            try
            {
                // Check if the model state is valid
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Map the view model to the entity model
                var mappedData = _mapper.Map<ApiNameViewModel, ApiNames>(apiViewModel);

                // Check if an entry with the same ApiName exists in the database
                var existingApiName = _servicePlusContext.ApiNames.FirstOrDefault(d => d.ApiName == mappedData.ApiName);

                if (existingApiName != null)
                {
                    // Update the existing entry with the new data
                    existingApiName.ApiDescription = apiViewModel.ApiDescription; // Assuming ApiDescription is a property in ApiNames
                    _servicePlusContext.ApiNames.Update(existingApiName);
                    await _servicePlusContext.SaveChangesAsync();
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Updated", Message = "API Description Updated" });

                }
                else
                {
                    // Entry with the given ApiName doesn't exist, return an error response
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "ApiName Not Found" });
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the update process
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = ex.Message });
            }
        }

        #endregion

        #region Common ConvertToUtc
        private DateTime? ConvertToUtc(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);
            }
            return null;
        }
        #endregion

        #region Get District 
        [CustomAuthorizeAttribute]
        [Route("GetDistricts")]
        [HttpGet]
        public async Task<IActionResult> GetDistrict()
        {
            var district = await _postgresDbContext.CustomLGDDistricts.ToListAsync();
            return Ok(district);
        }
        #endregion

        #region Get Tehsil
        [CustomAuthorizeAttribute]
        [Route("GetTehsils")]
        [HttpGet]
        public async Task<IActionResult> GetTehsil()
        {
            var tehsil = await _postgresDbContext.CustomLGDTehsilSubTehsils.ToListAsync();
            return Ok(tehsil);
        }
        #endregion

        #region Get Department List
        [CustomAuthorizeAttribute]
        [Route("GetDepartments")]
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var servicePlusDep = _servicePlusContext.InitiatedDatas.Select(d => d.DepartmentName).Distinct().ToList();
            var incomeServiceDep = _postgresDbContext.InitiatedDatas.Select(d => d.DepartmentName).Distinct().ToList();

            // Concatenate both lists
            var combinedDepartments = servicePlusDep.Concat(incomeServiceDep).Distinct().ToList();

            return Ok(combinedDepartments);
        }
        #endregion

        #region Get Services Name && User Services Name
        [AllowAnonymous]
        [HttpGet]
        [Route("GetServicesName")]

        public async Task<IActionResult> GetServiceName()
        {
            var servicePlusName = _servicePlusContext.InitiatedDatas.Select(d => d.ServiceName).Distinct().ToList();
            var incomeServiceName = _postgresDbContext.InitiatedDatas.Select(d => d.ServiceName).Distinct().ToList();

            // Concatenate both lists
            var combinedServiceName = servicePlusName.Concat(incomeServiceName).Distinct().ToList();

            return Ok(combinedServiceName);

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetuserServicesName")]
        public async Task<IActionResult> GetuserServicesName()
        {
            return Ok();

        }
        #endregion

        #region Get Service Wise Records with Service Name
        [CustomAuthorizeAttribute]
        [HttpGet]
        [Route("GetServicesWiseCount")]
        public async Task<IActionResult> GetServicesWiseCount(string serviceName)
        {
            if (serviceName == "Income Certificate")
            {
                var incomeServiceCount = _postgresDbContext.TaskDetails.Select(x => x.ApplId)
                                           .Distinct().Count();
                return Ok(incomeServiceCount);
            }
            else
            {
                var allServiceCount = _servicePlusContext.InitiatedDatas.Where(d => d.ServiceName == serviceName).Count();
                return Ok(allServiceCount);
            }
        }
        #endregion

        #region Consolidate Report
        [CustomAuthorizeAttribute]
        [Route("ConsolidateReport")]
        [HttpGet]
        public async Task<IActionResult> ConsolidateReport(int page, int pageSize)
        {
            var totalCount = await _servicePlusContext.InitiatedDatas.CountAsync();

            var initiatedRecords = await _servicePlusContext.InitiatedDatas.OrderByDescending(d => d.SubmissionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize).ToListAsync();

            var result = new
            {

                recordsTotal = totalCount,
                recordsFiltered = totalCount,
                data = initiatedRecords.Select(initiatedRecord => new
                {
                    initiatedRecord,
                    ExecutionRecord = GetExecutionRecord(initiatedRecord.ApplId ?? 0).Any()
                            ? GetExecutionRecord(initiatedRecord.ApplId ?? 0)
                            : null
                })

            };


            return Ok(result);
        }

        private List<ExecutionData> GetExecutionRecord(int applId)
        {
            return _servicePlusContext.ExecutionDatas
                .Include(d => d.TaskDetails)
                    .ThenInclude(task => task.UserDetail)
                .Include(d => d.OfficialFormDetail)
                .Where(d => d.TaskDetails.ApplId == applId)
                .Select(d => new ExecutionData
                {
                    // Include properties you want here
                    TaskDetails = new TaskDetail
                    {
                        Amount = d.TaskDetails.Amount,
                        ApplId = d.TaskDetails.ApplId,
                        Remarks = d.TaskDetails.Remarks,
                        TaskId = d.TaskDetails.TaskId,
                        ActionNo = d.TaskDetails.ActionNo,
                        TaskName = d.TaskDetails.TaskName,
                        TaskType = d.TaskDetails.TaskType,
                        UserName = d.TaskDetails.UserName,
                        ServiceId = d.TaskDetails.ServiceId,
                        UserDetail = new UserDetail
                        {
                            UserName = d.TaskDetails.UserDetail.UserName,
                            Designation = d.TaskDetails.UserDetail.Designation,
                            LocationId = d.TaskDetails.UserDetail.LocationId,
                            PullUserId = d.TaskDetails.UserDetail.PullUserId,
                            LocationName = d.TaskDetails.UserDetail.LocationName,
                            DepartmentLevel = d.TaskDetails.UserDetail.DepartmentLevel,
                            LocationTypeId = d.TaskDetails.UserDetail.LocationTypeId,
                            CurrentProcessId = d.TaskDetails.UserDetail.CurrentProcessId,

                        },
                        ActionTaken = d.TaskDetails.ActionTaken,
                        PaymentDate = d.TaskDetails.PaymentDate,
                        PaymentMode = d.TaskDetails.PaymentMode,
                        PullUserId = d.TaskDetails.PullUserId,
                        ExecutedTime = d.TaskDetails.ExecutedTime,
                        ReceivedTime = d.TaskDetails.ReceivedTime,
                        PaymentRefNo = d.TaskDetails.PaymentRefNo,
                        CurrentProcessId = d.TaskDetails.CurrentProcessId,
                        CallbackCurrProcId = d.TaskDetails.CallbackCurrProcId

                    },
                    OfficialFormDetail = _servicePlusContext.OfficialFormDetails
                .Where(ofd => ofd.ExecutionDataId == d.ExecutionDataId)
                .Select(ofd => new OfficialFormDetail
                {
                    OfficalFormID = ofd.OfficalFormID,
                    OfficalFormValue = ofd.OfficalFormValue,
                })
                .ToList()
                })
                .ToList();
        }


        #endregion

        #region CalculateConsolidatePendencyReport
        [CustomAuthorizeAttribute]
        [Route("ConsolidatePendencyReport")]
        [HttpGet]
        public async Task<IActionResult> ConsolidatePendencyReport()
        {
            var groupedInitiatedRecords = _servicePlusContext.InitiatedDatas
                                           .GroupBy(initiated => initiated.ServiceName)
                                           .Select(group => new
                                           {
                                               ServiceName = group.Key,
                                               ApplIds = group.GroupBy(initiated => initiated.ApplId)
                                                             .Select(subGroup => subGroup.Key)
                                                             .ToList()
                                           })
                                           .ToList();

            List<ConsolidatePendencyReport> pendencyReports = new List<ConsolidatePendencyReport>();

            foreach (var item in groupedInitiatedRecords)
            {
                var topExecutionRecords = _servicePlusContext.ExecutionDatas
                                           .Include(d => d.TaskDetails)
                                           .Include(d => d.OfficialFormDetail)
                                           .Where(d => item.ApplIds.Contains(d.TaskDetails.ApplId))
                                           .GroupBy(d => d.TaskDetails.ApplId)
                                           .Select(group => group.OrderByDescending(e => e.TaskDetails.ReceivedTime).FirstOrDefault())
                                           .ToList();

                var pendencyReport = CalculateConsolidatePendencyReport(item, topExecutionRecords);

                if (pendencyReport.ApplicationRecieved > 0)
                {
                    pendencyReports.Add(pendencyReport);
                }
            }

            return Ok(pendencyReports);
        }

        private ConsolidatePendencyReport CalculateConsolidatePendencyReport(dynamic groupedInitiatedRecord, List<ExecutionData> group)
        {
            var serviceName = groupedInitiatedRecord.ServiceName;
            var totalApplicationRecieved = groupedInitiatedRecord.ApplIds;
            var totalApplicationRecievedCount = totalApplicationRecieved.Count;


            int deliveredCount = 0;
            int rejectedCount = 0;
            int inProcessCount = 0;
            int day1to5Count = 0;
            int day6to30Count = 0;
            int day31to60Count = 0;
            int day61to90Count = 0;
            int day91toAboveCount = 0;
            int sendBackCount = 0;
            int totalPendingDays = 0;

            foreach (var record in group)
            {
                if (record.TaskDetails != null)
                {
                    DateTime currentDate = DateTime.Today;
                    DateTime receivedDate = (DateTime)record.TaskDetails.ReceivedTime;

                    DateTime dueDate = receivedDate.AddDays(3);
                    TimeSpan difference = currentDate - dueDate;
                    int daysDifference = (int)difference.TotalDays;
                    bool isDelivered = record.OfficialFormDetail.Any(d => IsDeliverd(d.OfficalFormValue));
                    bool isRejected = record.OfficialFormDetail.Any(d => IsRejected(d.OfficalFormValue));
                    bool isInProcess = record.OfficialFormDetail.Any(d => IsInProcess(d.OfficalFormValue));
                    bool isSendBack = record.OfficialFormDetail.Any(d => IsSendBack(d.OfficalFormValue));

                    if (isDelivered)
                    {
                        deliveredCount++;
                    }
                    else if (isRejected)
                    {
                        rejectedCount++;
                    }
                    else if (isInProcess)
                    {
                        inProcessCount++;

                        if (daysDifference >= 1 && daysDifference <= 5)
                        {
                            day1to5Count++;
                        }
                        else if (daysDifference >= 6 && daysDifference <= 30)
                        {
                            day6to30Count++;
                        }
                        else if (daysDifference >= 31 && daysDifference <= 60)
                        {
                            day31to60Count++;
                        }
                        else if (daysDifference >= 61 && daysDifference <= 90)
                        {
                            day61to90Count++;
                        }
                        else if (daysDifference >= 91)
                        {
                            day91toAboveCount++;
                        }
                    }
                    else if (isSendBack)
                    {
                        sendBackCount++;
                    }
                    else
                    {
                        _logger.LogInformation("Not Exist with ExecutionDataId: " + record.ExecutionDataId);
                    }
                }
                else
                {
                    _logger.LogInformation("Error occurred for record with ExecutionDataId: " + record.ExecutionDataId);
                }
            }

            totalPendingDays = day1to5Count + day6to30Count + day31to60Count + day61to90Count + day91toAboveCount;

            int totalApplicationsReceived = group.Count();
            double pendencyPercentage = totalApplicationsReceived > 0 ? (double)totalPendingDays / totalApplicationsReceived * 100 : 0;

            if (pendencyPercentage - Math.Floor(pendencyPercentage) > 0.05)
            {
                pendencyPercentage = Math.Ceiling(pendencyPercentage * 100 + 1) / 100;
            }
            else
            {
                pendencyPercentage = Math.Floor(pendencyPercentage * 100) / 100;
            }
            return new ConsolidatePendencyReport
            {
                ServiceNane = serviceName,
                ApplicationRecieved = totalApplicationRecievedCount,
                Deliverd = deliveredCount,
                Rejected = rejectedCount,
                InProcess = inProcessCount,
                Day1to5 = day1to5Count,
                Day6to30 = day6to30Count,
                Day31to60 = day31to60Count,
                Day61to90 = day61to90Count,
                Day91toAbove = day91toAboveCount,
                SendBack = sendBackCount,
                TotalPendingDays = totalPendingDays,
                PendencyPercentage = pendencyPercentage
            };

        }

        #endregion

        #region Consolidate Service Wise  Report
        [CustomAuthorizeAttribute]
        [Route("ConsolidateServiceWiseReport")]
        [HttpGet]
        public async Task<IActionResult> ConsolidateServiceWiseReport(DateTime fromDate, DateTime toDate, string serviceName)
        {
            // Convert the input dates to UTC format
            var utcFromDate = fromDate.ToUniversalTime();
            var utcToDate = toDate.ToUniversalTime();
            var totalCount = await _servicePlusContext.InitiatedDatas.CountAsync();

            var initiatedRecords = await _servicePlusContext.InitiatedDatas.Include(d => d.AttributeDetail).Include(d => d.EnclosureDetails).OrderByDescending(d => d.SubmissionDate)
                .Where(d => d.SubmissionDate >= utcFromDate && d.SubmissionDate <= utcToDate && d.ServiceName.Contains(serviceName)).Select(d => new
                {
                    d.DepartmentId,
                    d.DepartmentName,
                    d.ServiceId,
                    d.ServiceName,
                    d.ApplId,
                    d.ApplRefNo,
                    d.NoOfAttachment,
                    d.SubmissionMode,
                    d.SubmissionDate,
                    d.AppliedBy,
                    d.SubmissionLocation,
                    d.SubmissionLocationId,
                    d.SubmissionLocationTypeId,
                    d.PaymentMode,
                    d.ReferenceNo,
                    d.PaymentDate,
                    d.Amount,
                    d.RegistrationId,
                    d.BaseServiceId,
                    d.VersionNo,
                    d.SubVersion,
                    AttributeDetail = d.AttributeDetail.Select(attr => new
                    {
                        attr.ApplicationFormFieldID,
                        attr.ApplicationFormFieldValue
                    }),


                }).ToListAsync();

            var result = new
            {

                recordsTotal = totalCount,
                recordsFiltered = totalCount,
                data = initiatedRecords.Select(initiatedRecord => new
                {
                    initiatedRecord,
                    ExecutionRecord = GetExecutionRecord(initiatedRecord.ApplId ?? 0).Any()
                            ? GetExecutionRecord(initiatedRecord.ApplId ?? 0)
                            : null
                })

            };


            return Ok(result);
        }
        #endregion

        #region ChangeServiceParameter
        [HttpPost]
        [Route("ChangeServiceParameter")]
        public async Task<IActionResult> ChangeServiceParameter(string serviceNameExist, string serviceNameChange)
        {
            if (string.IsNullOrEmpty(serviceNameExist) || string.IsNullOrEmpty(serviceNameChange))
            {
                return BadRequest("Both 'serviceNameExist' and 'serviceNameChange' must be provided.");
            }

            // Find the records with the existing service name
            var recordsToUpdate = _servicePlusContext.InitiatedDatas.Where(d => d.ServiceName == serviceNameExist).ToList();

            if (recordsToUpdate.Any())
            {
                // Update the service name for each record
                foreach (var record in recordsToUpdate)
                {
                    record.ServiceName = serviceNameChange;
                }

                // Save changes to the database
                await _servicePlusContext.SaveChangesAsync();

                return Ok("Service name updated successfully.");
            }
            else
            {
                return NotFound("No records found with the existing service name.");
            }
        }

        #endregion


        #region Public Sports Report


        [Route("GetPublicIndividualSportsReport")]
        [HttpPost]
        public async Task<IActionResult> GetPublicIndividualSportsReport([FromBody] FilterParameter filterParameter)
        {
            // Build the base query
            var query = from initiatedData in _servicePlusContext.InitiatedDatas
                        join taskDetails in _servicePlusContext.TaskDetails on initiatedData.ApplId equals taskDetails.ApplId
                        join officialFormDetails in _servicePlusContext.OfficialFormDetails on taskDetails.ExecutionDataId
                        equals officialFormDetails.ExecutionDataId into groupedOfficialFormDetails

                        where initiatedData.ServiceName.Contains("Punjab Sports Events Portal") && taskDetails.TaskId == 23005
                              && groupedOfficialFormDetails.Any(ofd => ofd.OfficalFormID == "171829") == false // Fixed condition
                        orderby initiatedData.InitiatedDataId descending
                        select new
                        {
                            InitiatedDataId = initiatedData.InitiatedDataId,
                            AttributeDetails = initiatedData.AttributeDetail
                                .Where(attr => new[]
                                {
                            "169954", "169955", "169957", "169958", "169964",
                            "170094", "170202", "170203", "170246", "170608",
                            "170091", "170041", "170309", "171427", "170093",
                            "170092"
                                }.Contains(attr.ApplicationFormFieldID))
                                .ToList(),
                            initiatedData.ServiceId,
                            initiatedData.ServiceName,
                            initiatedData.ApplId,
                            initiatedData.ApplRefNo,
                            initiatedData.SubmissionDate,
                            TaskDetail = new
                            {
                                taskDetails.TaskDetailID,
                                taskDetails.ExecutionDataId,
                                taskDetails.TaskName,
                                OfficialFormDetails = groupedOfficialFormDetails
                                    .Where(ofd => ofd.OfficalFormID == "170912" &&
                                          (string.IsNullOrWhiteSpace(filterParameter.SearchValue) ||
                                           ofd.OfficalFormValue.Contains(filterParameter.SearchValue)))
                                    .ToList()
                            }
                        };

            // Apply date filter if both dates are provided
            if (filterParameter.StartDate.HasValue && filterParameter.EndDate.HasValue)
            {
                var startUtc = filterParameter.StartDate.Value.ToUniversalTime();
                var endUtc = filterParameter.EndDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                query = query.Where(data => data.SubmissionDate >= startUtc && data.SubmissionDate <= endUtc);
            }

            // Apply filters only if parameters are not empty
            if (!string.IsNullOrWhiteSpace(filterParameter.Tournament))
            {
                query = query.Where(data => data.AttributeDetails.Any(attr => attr.ApplicationFormFieldID == "171943" && attr.ApplicationFormFieldValue == filterParameter.Tournament));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.Gender))
            {
                query = query.Where(data => data.AttributeDetails.Any(attr =>
                    (attr.ApplicationFormFieldID == "169964" || attr.ApplicationFormFieldID == "171427")
                    && attr.ApplicationFormFieldValue == filterParameter.Gender));
            }

            if (!string.IsNullOrWhiteSpace(filterParameter.Level))
            {
                query = query.Where(data => data.AttributeDetails.Any(attr =>
                    (attr.ApplicationFormFieldID == "170091" || attr.ApplicationFormFieldID == "170041")
                    && attr.ApplicationFormFieldValue == filterParameter.Level));
            }

            if (!string.IsNullOrWhiteSpace(filterParameter.ApplicantGame))
            {
                query = query.Where(data => data.AttributeDetails.Any(attr => attr.ApplicationFormFieldID == "170094" && attr.ApplicationFormFieldValue == filterParameter.ApplicantGame));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.ApplicantAgeGroup))
            {
                query = query.Where(data => data.AttributeDetails.Any(attr => attr.ApplicationFormFieldID == "170202" && attr.ApplicationFormFieldValue == filterParameter.ApplicantAgeGroup));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.ApplicantGameCategory))
            {
                query = query.Where(data => data.AttributeDetails.Any(attr => attr.ApplicationFormFieldID == "170246" && attr.ApplicationFormFieldValue == filterParameter.ApplicantGameCategory));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.ApplicantEvent))
            {
                query = query.Where(data => data.AttributeDetails.Any(attr => attr.ApplicationFormFieldID == "170203" && attr.ApplicationFormFieldValue == filterParameter.ApplicantEvent));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.ApplicationType))
            {
                query = query.Where(data => data.AttributeDetails.Any(attr => attr.ApplicationFormFieldID == "170608" && attr.ApplicationFormFieldValue == filterParameter.ApplicationType));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.IsMedalist))
            {
                query = query.Where(data => data.AttributeDetails.Any(attr => attr.ApplicationFormFieldID == "170309" && attr.ApplicationFormFieldValue == filterParameter.IsMedalist));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.District))
            {
                query = query.Where(data => data.AttributeDetails.Any(attr => attr.ApplicationFormFieldID == "170093" && attr.ApplicationFormFieldValue == filterParameter.District));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.Block))
            {
                query = query.Where(data => data.AttributeDetails.Any(attr => attr.ApplicationFormFieldID == "170092" && attr.ApplicationFormFieldValue == filterParameter.Block));
            }

            // Sorting logic
            //query = sortOrder.ToLower() switch
            //{
            //    "asc" => sortColumn.ToLower() switch
            //    {
            //        "submissiondate" => query.OrderBy(data => data.SubmissionDate),
            //        "applid" => query.OrderBy(data => data.ApplId),
            //        _ => query.OrderBy(data => data.SubmissionDate) // Default case
            //    },
            //    "desc" => sortColumn.ToLower() switch
            //    {
            //        "submissiondate" => query.OrderByDescending(data => data.SubmissionDate),
            //        "applid" => query.OrderByDescending(data => data.ApplId),
            //        _ => query.OrderByDescending(data => data.SubmissionDate) // Default case
            //    },
            //    _ => query.OrderByDescending(data => data.SubmissionDate) // Default case
            //};


            // Count query
            var totalCount = await query.CountAsync();

            // Paginate records
            var paginatedRecords = await query
                .Skip((filterParameter.page - 1) * filterParameter.pageSize)
                .Take(filterParameter.pageSize)
                .ToListAsync();

            // Transform data into ViewModel
            var result = paginatedRecords.Select(data => new PublicSportsViewModel
            {
                InitiatedDataId = data.InitiatedDataId,
                AttributeDetailID = data.AttributeDetails
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170608")?.AttributeDetailID,
                TaskDetailID = data.TaskDetail?.TaskDetailID,
                ExecutionDataId = data.TaskDetail?.ExecutionDataId,
                OfficialFormDetailID = data.TaskDetail?.OfficialFormDetails
                    .FirstOrDefault()?.OfficialFormDetailID,
                ApplId = data.ApplId,
                ApplRefNo = data.ApplRefNo,
                TaskName = data.TaskDetail?.TaskName,
                TaskId = 23005, // Since it's filtered, we know the value
                ServiceId = data.ServiceId,
                ServiceName = data.ServiceName,
                SubmissionDate = data.SubmissionDate,
                ApplicantFirstName = data.AttributeDetails
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "169954")?.ApplicationFormFieldValue,

                ApplicantFatherName = CleanValue(data.AttributeDetails
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "169955")?.ApplicationFormFieldValue),

                ApplicantBloodGroup = CleanValue(data.AttributeDetails
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "169957")?.ApplicationFormFieldValue),

                ApplicantMobileNo = CleanValue(data.AttributeDetails
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "169958")?.ApplicationFormFieldValue),

                ApplicantGender = CleanValue(data.AttributeDetails
    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "169964")?.ApplicationFormFieldValue
    ?? data.AttributeDetails.FirstOrDefault(attr => attr.ApplicationFormFieldID == "171427")?.ApplicationFormFieldValue),

                ApplicantGame = CleanValue(data.AttributeDetails
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170094")?.ApplicationFormFieldValue),

                Level = CleanValue(data.AttributeDetails
    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170091")?.ApplicationFormFieldValue
    ?? data.AttributeDetails.FirstOrDefault(attr => attr.ApplicationFormFieldID == "170041")?.ApplicationFormFieldValue),

                ApplicationType = CleanValue(data.AttributeDetails
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170608")?.ApplicationFormFieldValue),
                ApplicantAgeGroup = CleanValue(data.AttributeDetails
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170202")?.ApplicationFormFieldValue),
                ApplicantEvent = CleanValue(data.AttributeDetails
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170203")?.ApplicationFormFieldValue),
                ApplicantGameCategory = CleanValue(data.AttributeDetails
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170246")?.ApplicationFormFieldValue),
                IsMedalist = CleanValue(data.AttributeDetails
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170309")?.ApplicationFormFieldValue),

                District = CleanValue(data.AttributeDetails
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170093")?.ApplicationFormFieldValue),

                Block = CleanValue(data.AttributeDetails
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170092")?.ApplicationFormFieldValue),

                ApplicantMedal = DeserializeJsonStreamAsync(data.TaskDetail?.OfficialFormDetails
                    .FirstOrDefault()?.OfficalFormValue)?.Split('~').Last()
            }).ToList();

            // Filter by medal if provided
            if (!string.IsNullOrWhiteSpace(filterParameter.SearchValue))
            {
                totalCount = result.Count(d => d.ApplicantMedal != null && d.ApplicantMedal.Equals(filterParameter.SearchValue, StringComparison.OrdinalIgnoreCase));
                result = result.Where(d => d.ApplicantMedal != null && d.ApplicantMedal.Equals(filterParameter.SearchValue, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Ok(new
            {
                TotalCount = totalCount,
                Records = result
            });
        }


        private string CleanValue(string value)
        {
            return string.IsNullOrEmpty(value) ? null : Regex.Replace(value, @"^\d+~", "");
        }

        [Route("GetPublicTeamSportsReport")]
        [HttpPost]
        public async Task<IActionResult> GetPublicTeamSportsReport([FromBody] FilterParameter filterParameter)
        {
            var query = from taskDetails in _servicePlusContext.TaskDetails
                        join initiatedData in _servicePlusContext.InitiatedDatas on taskDetails.ApplId equals initiatedData.ApplId
                        join officialFormDetails in _servicePlusContext.OfficialFormDetails on taskDetails.ExecutionDataId equals officialFormDetails.ExecutionDataId into groupedOfficialFormDetails
                        where taskDetails.TaskId == 23005
                              && initiatedData.ServiceName.Contains("Punjab Sports Events Portal")
                        // && groupedOfficialFormDetails.Any(ofd => ofd.OfficalFormID == "171829") // Check if at least one exists
                        orderby initiatedData.InitiatedDataId descending
                        select new
                        {
                            initiatedData.InitiatedDataId,
                            initiatedData.AttributeDetail,
                            initiatedData.ServiceId,
                            initiatedData.ServiceName,
                            initiatedData.ApplId,
                            initiatedData.ApplRefNo,
                            initiatedData.SubmissionDate,
                            TaskDetail = groupedOfficialFormDetails.Where(ofd =>
                                ofd.OfficalFormID == "171829" &&
                                (string.IsNullOrWhiteSpace(filterParameter.SearchValue) || ofd.OfficalFormValue.Contains(filterParameter.SearchValue))
                            ).Select(ofd => new
                            {
                                taskDetails.TaskDetailID,
                                taskDetails.ExecutionDataId,
                                taskDetails.TaskName,
                                OfficialFormDetail = ofd
                            }).ToList()
                        };


            // Apply date filter
            if (filterParameter.StartDate.HasValue && filterParameter.EndDate.HasValue)
            {
                var startUtc = filterParameter.StartDate.Value.ToUniversalTime();
                var endUtc = filterParameter.EndDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                query = query.Where(data => data.SubmissionDate >= startUtc && data.SubmissionDate <= endUtc);
            }
            // Apply filters only if parameters are not empty
            if (!string.IsNullOrWhiteSpace(filterParameter.Tournament))
            {
                query = query.Where(data => data.AttributeDetail.Any(attr => attr.ApplicationFormFieldID == "171943" && attr.ApplicationFormFieldValue == filterParameter.Tournament));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.Gender))
            {
                query = query.Where(data => data.AttributeDetail.Any(attr =>
                    (attr.ApplicationFormFieldID == "169964" || attr.ApplicationFormFieldID == "171427")
                    && attr.ApplicationFormFieldValue == filterParameter.Gender));
            }

            if (!string.IsNullOrWhiteSpace(filterParameter.Level))
            {
                query = query.Where(data => data.AttributeDetail.Any(attr =>
                    (attr.ApplicationFormFieldID == "170091" || attr.ApplicationFormFieldID == "170041")
                    && attr.ApplicationFormFieldValue == filterParameter.Level));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.ApplicantGame))
            {
                query = query.Where(data => data.AttributeDetail.Any(attr => attr.ApplicationFormFieldID == "170094" && attr.ApplicationFormFieldValue == filterParameter.ApplicantGame));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.ApplicantAgeGroup))
            {
                query = query.Where(data => data.AttributeDetail.Any(attr => attr.ApplicationFormFieldID == "170202" && attr.ApplicationFormFieldValue == filterParameter.ApplicantAgeGroup));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.ApplicantGameCategory))
            {
                query = query.Where(data => data.AttributeDetail.Any(attr => attr.ApplicationFormFieldID == "170246" && attr.ApplicationFormFieldValue == filterParameter.ApplicantGameCategory));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.ApplicantEvent))
            {
                query = query.Where(data => data.AttributeDetail.Any(attr => attr.ApplicationFormFieldID == "170203" && attr.ApplicationFormFieldValue == filterParameter.ApplicantEvent));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.ApplicationType))
            {
                query = query.Where(data => data.AttributeDetail.Any(attr => attr.ApplicationFormFieldID == "170608" && attr.ApplicationFormFieldValue == filterParameter.ApplicationType));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.IsMedalist))
            {
                query = query.Where(data => data.AttributeDetail.Any(attr => attr.ApplicationFormFieldID == "170309" && attr.ApplicationFormFieldValue == filterParameter.IsMedalist));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.District))
            {
                query = query.Where(data => data.AttributeDetail.Any(attr => attr.ApplicationFormFieldID == "170093" && attr.ApplicationFormFieldValue == filterParameter.District));
            }
            if (!string.IsNullOrWhiteSpace(filterParameter.Block))
            {
                query = query.Where(data => data.AttributeDetail.Any(attr => attr.ApplicationFormFieldID == "170092" && attr.ApplicationFormFieldValue == filterParameter.Block));
            }
            // Calculate total count
            var totalCount = await query.CountAsync();

            // Paginate the query
            var paginatedData = await query
                .Skip((filterParameter.page - 1) * filterParameter.pageSize)
                .Take(filterParameter.pageSize)
                .ToListAsync();

            // Process the paginated data
            var result = paginatedData.SelectMany(data => data.TaskDetail.Select(taskDetail =>
            {
                var officialFormData = SportsTeamDeserializeJsonStreamAsync(taskDetail.OfficialFormDetail.OfficalFormValue);

                if (officialFormData == null || !officialFormData.Any())
                    return Enumerable.Empty<PublicSportsViewModel>();

                return officialFormData.Select(form => new PublicSportsViewModel
                {
                    InitiatedDataId = data.InitiatedDataId,
                    AttributeDetailID = data.AttributeDetail
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170608")?.AttributeDetailID,
                    TaskDetailID = taskDetail.TaskDetailID,
                    ExecutionDataId = taskDetail.ExecutionDataId,
                    OfficialFormDetailID = taskDetail.OfficialFormDetail.OfficialFormDetailID,
                    ApplId = data.ApplId,
                    ApplRefNo = form.ApplicationRefNo,
                    TaskName = taskDetail.TaskName,
                    TaskId = 23005,
                    ServiceId = data.ServiceId,
                    ServiceName = data.ServiceName,
                    SubmissionDate = data.SubmissionDate,
                    ApplicantFirstName = form.PlayerName,

                    ApplicantFatherName = CleanValue(data.AttributeDetail
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "169955")?.ApplicationFormFieldValue),

                    ApplicantBloodGroup = CleanValue(data.AttributeDetail
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "169957")?.ApplicationFormFieldValue),

                    ApplicantMobileNo = CleanValue(data.AttributeDetail
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "169958")?.ApplicationFormFieldValue),

                    ApplicantGender = CleanValue(data.AttributeDetail
    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "169964")?.ApplicationFormFieldValue
    ?? data.AttributeDetail.FirstOrDefault(attr => attr.ApplicationFormFieldID == "171427")?.ApplicationFormFieldValue),

                    ApplicantGame = CleanValue(data.AttributeDetail
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170094")?.ApplicationFormFieldValue),

                    Level = CleanValue(data.AttributeDetail
    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170091")?.ApplicationFormFieldValue
    ?? data.AttributeDetail.FirstOrDefault(attr => attr.ApplicationFormFieldID == "170041")?.ApplicationFormFieldValue),

                    ApplicationType = CleanValue(data.AttributeDetail
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170608")?.ApplicationFormFieldValue),

                    ApplicantAgeGroup = CleanValue(data.AttributeDetail
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170202")?.ApplicationFormFieldValue),
                    ApplicantEvent = CleanValue(data.AttributeDetail
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170203")?.ApplicationFormFieldValue),

                    District = CleanValue(data.AttributeDetail
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170093")?.ApplicationFormFieldValue),

                    Block = CleanValue(data.AttributeDetail
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170092")?.ApplicationFormFieldValue),

                    ApplicantGameCategory = CleanValue(data.AttributeDetail
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170246")?.ApplicationFormFieldValue),
                    ApplicantMedal = form.Position?.Split('~').LastOrDefault() ?? string.Empty
                });
            }))
            .Where(x => x != null) // Exclude null projections
            .SelectMany(x => x)
            .ToList();

            // Filter based on searchValue if provided
            if (!string.IsNullOrWhiteSpace(filterParameter.SearchValue))
            {
                result = result
                    .Where(d => !string.IsNullOrEmpty(d.ApplicantMedal) &&
                                d.ApplicantMedal.Equals(filterParameter.SearchValue, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Update totalCount after filtering
                totalCount = result.Count;
            }

            // Return the paginated result
            return Ok(new
            {
                TotalCount = totalCount,
                Records = result.OrderBy(d => d.ApplicantGame).ToList(),
            });
        }

        private List<PlayerDetail> SportsTeamDeserializeJsonStreamAsync(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<PlayerDetail>();

            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            var result = new List<PlayerDetail>();
            var detectedKeys = data.Keys
                .Where(k => k.StartsWith("171830_") && int.TryParse(k.Split('_')[1], out _))
                .Select(k => new { Key = k, Index = int.Parse(k.Split('_')[1]) })
                .ToList();

            int maxIndex = detectedKeys.Select(k => k.Index).DefaultIfEmpty(0).Max();

            for (int i = 1; i <= maxIndex; i++)
            {
                result.Add(new PlayerDetail
                {
                    PlayerName = data.ContainsKey($"171830_{i}") ? data[$"171830_{i}"]?.ToString() : null,
                    DateOfBirth = data.ContainsKey($"171831_{i}") ? data[$"171831_{i}"]?.ToString() : null,
                    MobileNumber = data.ContainsKey($"171832_{i}") ? data[$"171832_{i}"]?.ToString() : null,
                    Email = data.ContainsKey($"171833_{i}") ? data[$"171833_{i}"]?.ToString() : null,
                    ApplicationRefNo = data.ContainsKey($"171834_{i}") ? data[$"171834_{i}"]?.ToString() : null,
                    Position = data.ContainsKey($"171835_{i}") ? data[$"171835_{i}"]?.ToString() : null
                });
            }

            return result;
        }

        /// <summary>
        /// First We are getting Count of rows after choosing Any First Header ,then we are 
        /// replacing Header Ids
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private List<PlayerEducation> PlayerEducationDeserializeJsonStreamAsync(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<PlayerEducation>();

            var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);

            if (!data.ContainsKey("data"))
                return new List<PlayerEducation>();

            var jsonData = data["data"];
            var result = new List<PlayerEducation>();

            var detectedKeys = jsonData.Keys
                .Where(k => k.StartsWith("171648_") && int.TryParse(k.Split('_')[1], out _))
                .Select(k => new { Key = k, Index = int.Parse(k.Split('_')[1]) })
                .ToList();

            int maxIndex = detectedKeys.Select(k => k.Index).DefaultIfEmpty(0).Max();

            for (int i = 1; i <= maxIndex; i++)
            {
                result.Add(new PlayerEducation
                {
                    Qualification = jsonData.ContainsKey($"171648_{i}") && jsonData[$"171648_{i}"] != null
    ? jsonData[$"171648_{i}"].Split('~').ElementAtOrDefault(1)
    : null,
                    InstituteName = jsonData.ContainsKey($"171649_{i}") ? jsonData[$"171649_{i}"]?.ToString() : null,
                    PassingYear = jsonData.ContainsKey($"171650_{i}") ? jsonData[$"171650_{i}"]?.ToString() : null,

                    // Position = GetSafeValue(jsonData, $"171835_{i}") // Uncomment if needed
                });
            }

            return result;
        }

        public static string DeserializeJsonStreamAsync(string? jsonStream)
        {
            if (jsonStream == null)
            {
                return string.Empty;
            }
            var jsonData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStream);

            if (jsonData != null && jsonData.Count > 0)
            {
                // Get the last key-value pair
                var lastKeyValue = jsonData.Last();

                // Store the last value in a variable
                string lastValue = Regex.Replace(lastKeyValue.Value.ToString(), @"^\d+~", "");

                // Output the last value

                return lastValue;
            }
            else
            {
                Console.WriteLine("Invalid JSON or empty data.");
            }

            return string.Empty; // Return empty string if no valid data
        }

        [Route("GetSportSupportDoc")]
        [HttpGet]
        public async Task<IActionResult> GetSportSupportDoc(int applId)
        {
            // Base URL where the documents are stored
            var baseUrl = $"http://10.147.24.36:8082/SSD/";

            // Construct the full file URL
            var fileUrl = $"{baseUrl}{applId}.pdf"; // Assuming files are in PDF format

            try
            {
                // Use HttpClient to check if the file exists and fetch its content
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(fileUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        return NotFound(new
                        {
                            Message = "Document not found.",
                            ApplId = applId
                        });
                    }

                    // Return the file as a response
                    return Ok(new
                    {
                        FileUrl = fileUrl,
                        ApplId = applId
                    });
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed and return an error response
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving the document.",
                    Error = ex.Message
                });
            }
        }


        #region PlayerDetail
        [Route("GetPlayerDetailsByAppRefNo")]
        [HttpGet]
        public async Task<IActionResult> GetPlayerDetailsByAppRefNo(string applRefNo)
        {
            var query = await (from initiatedData in _servicePlusContext.InitiatedDatas
                               where initiatedData.ServiceName.Contains("Punjab Sports Events Portal")
                                     && initiatedData.ApplRefNo == applRefNo
                                     && initiatedData.InitiatedRecordInsertionFlag == 1
                               select new
                               {
                                   initiatedData.InitiatedDataId,
                                   initiatedData.ServiceId,
                                   initiatedData.ServiceName,
                                   initiatedData.ApplId,
                                   initiatedData.ApplRefNo,
                                   initiatedData.SubmissionDate,
                                   AttributeDetails = initiatedData.AttributeDetail
                                       .Where(attr => new[]
                                       {
                                   "169954", "169955", "169957", "169958", "169964",
                                   "170094", "170202", "170203", "170246", "170608",
                                   "170091", "170608", "170041", "170309", "171427",
                                   "170093", "170092", "169965", "169971", "169960",
                                   "169972", "169969", "169970", "169959", "171762",
                                   "169963", "169961", "169980", "169979", "169981",
                                   "169998", "169999", "169990", "169991", "170000",
                                   "169983", "171761", "169987", "169984", "169988",
                                   "169985", "170308", "171647",
                                       }.Contains(attr.ApplicationFormFieldID))
                                       .ToList()
                               }).FirstOrDefaultAsync();

            if (query == null)
            {
                return NotFound(new { message = "No record found" });
            }

            var result = new
            {
                initiatedDataId = query.InitiatedDataId,
                attributeDetailID = query.AttributeDetails
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170608")?.AttributeDetailID,
                executionDataId = (int?)null,
                officialFormDetailID = (int?)null,
                applId = query.ApplId,
                applRefNo = query.ApplRefNo,
                taskId = 23005,
                serviceId = query.ServiceId,
                serviceName = query.ServiceName,
                submissionDate = query.SubmissionDate,
                applicantFirstName = GetValue(query.AttributeDetails, "169954"),
                applicantFatherName = GetValue(query.AttributeDetails, "169955"),
                applicantMotherName = GetValue(query.AttributeDetails, "169965"),
                applicantDOB = GetValue(query.AttributeDetails, "169971"),
                applicantAge = GetValue(query.AttributeDetails, "169960"),
                stateOfBirth = GetValue(query.AttributeDetails, "169972")?.Split('~').Last(),
                districtOfBirth = GetValue(query.AttributeDetails, "169969")?.Split('~').Last(),
                panCardNumber = GetValue(query.AttributeDetails, "169970"),
                applicantBloodGroup = GetValue(query.AttributeDetails, "169957")?.Split('~').Last(),
                applicantMobileNo = GetValue(query.AttributeDetails, "169958"),
                applicantAlternateMobileNumber = GetValue(query.AttributeDetails, "171762"),
                applicantEmail = GetValue(query.AttributeDetails, "169959"),
                applicantGender = GetValue(query.AttributeDetails, "169964")?.Split('~').Last() ?? GetValue(query.AttributeDetails, "171427")?.Split('~').Last(),
                applicantGame = GetValue(query.AttributeDetails, "170094")?.Split('~').Last(),
                level = GetValue(query.AttributeDetails, "170091")?.Split('~').Last() ?? GetValue(query.AttributeDetails, "170041"),
                applicationType = GetValue(query.AttributeDetails, "170608")?.Split('~').Last(),
                applicantAgeGroup = GetValue(query.AttributeDetails, "170202")?.Split('~').Last(),
                applicantEvent = GetValue(query.AttributeDetails, "170203")?.Split('~').Last(),
                applicantGameCategory = GetValue(query.AttributeDetails, "170246"),
                isMedalist = GetValue(query.AttributeDetails, "170309")?.Split('~').Last(),
                district = GetValue(query.AttributeDetails, "170093"),
                block = GetValue(query.AttributeDetails, "170092"),
                physicalDisability = GetValue(query.AttributeDetails, "169963")?.Split('~').Last(),
                maritalStatus = GetValue(query.AttributeDetails, "169961")?.Split('~').Last(),
                spouseName = GetValue(query.AttributeDetails, "169962"),
                isEmployed = GetValue(query.AttributeDetails, "169980")?.Split('~').Last(),
                employmentStatus = GetValue(query.AttributeDetails, "169979"),
                jobDescription = GetValue(query.AttributeDetails, "169981"),
                completeAddress = GetValue(query.AttributeDetails, "169998"),
                region = GetValue(query.AttributeDetails, "169999")?.Split('~').Last(),
                addState = GetValue(query.AttributeDetails, "169990")?.Split('~').Last(),
                addDistrict = GetValue(query.AttributeDetails, "169991")?.Split('~').Last(),
                addPincode = GetValue(query.AttributeDetails, "170000"),
                accountNumber = GetValue(query.AttributeDetails, "169983"),
                accountHolder = GetValue(query.AttributeDetails, "171761")?.Split('~').Last(),
                ifscCode = GetValue(query.AttributeDetails, "169987"),
                nameOnPassbook = GetValue(query.AttributeDetails, "169984"),
                bankAddress = GetValue(query.AttributeDetails, "169988"),
                bankName = GetValue(query.AttributeDetails, "169985"),
                applicationToBeSubmitted = GetValue(query.AttributeDetails, "170308")?.Split('~').Last(),
                playerEducations = PlayerEducationDeserializeJsonStreamAsync(GetValue(query.AttributeDetails, "171647"))
            };

            return Ok(result);
        }

        private string? GetValue(IEnumerable<AttributeDetail> attributes, string fieldId)
        {
            return attributes.FirstOrDefault(attr => attr.ApplicationFormFieldID == fieldId)?.ApplicationFormFieldValue;
        }



        private List<InterNationalAchievements> InterNationalAchievementsDeserializeJsonStreamAsync(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "FieldSetValue")
                return new List<InterNationalAchievements>();

            var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
            var jsonData = data["data"];
            var result = new List<InterNationalAchievements>();
            var detectedKeys = jsonData.Keys
                .Where(k => k.StartsWith("171412_") && int.TryParse(k.Split('_')[1], out _))
                .Select(k => new { Key = k, Index = int.Parse(k.Split('_')[1]) })
                .ToList();

            int maxIndex = detectedKeys.Select(k => k.Index).DefaultIfEmpty(0).Max();

            for (int i = 1; i <= maxIndex; i++)
            {
                result.Add(new InterNationalAchievements
                {
                    Game = GetSafeValueForAchievement(jsonData, $"171412_{i}"),
                    GameCategory = GetSafeValueForAchievement(jsonData, $"171413_{i}"),
                    GameType = GetSafeValueForAchievement(jsonData, $"171414_{i}"),
                    AgeGroup = GetSafeValueForAchievement(jsonData, $"171415_{i}"),
                    GameEvent = GetSafeValueForAchievement(jsonData, $"171416_{i}"),
                    TournamentName = jsonData.ContainsKey($"171417_{i}") ? jsonData[$"171417_{i}"]?.ToString() : null,
                    TournamentFrom = jsonData.ContainsKey($"171418_{i}") ? jsonData[$"171418_{i}"]?.ToString() : null,
                    TournamentTo = jsonData.ContainsKey($"171646_{i}") ? jsonData[$"171646_{i}"]?.ToString() : null,
                    Position = GetSafeValueForAchievement(jsonData, $"171419_{i}")
                });
            }

            return result;
        }

        private List<NationalAchievements> NationalAchievementsDeserializeJsonStreamAsync(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "FieldSetValue")
                return new List<NationalAchievements>();

            var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
            var jsonData = data["data"];
            var result = new List<NationalAchievements>();

            var detectedKeys = jsonData.Keys
                .Where(k => k.StartsWith("171395_") && int.TryParse(k.Split('_')[1], out _))
                .Select(k => int.Parse(k.Split('_')[1]))
                .ToList();

            int maxIndex = detectedKeys.DefaultIfEmpty(0).Max();

            for (int i = 1; i <= maxIndex; i++)
            {
                result.Add(new NationalAchievements
                {
                    Game = GetSafeValueForAchievement(jsonData, $"171395_{i}"),
                    GameCategory = GetSafeValueForAchievement(jsonData, $"171396_{i}"),
                    GameType = GetSafeValueForAchievement(jsonData, $"171397_{i}"),
                    AgeGroup = GetSafeValueForAchievement(jsonData, $"171398_{i}"),
                    GameEvent = GetSafeValueForAchievement(jsonData, $"171399_{i}"),
                    TournamentName = jsonData.ContainsKey($"171400_{i}") ? jsonData[$"171400_{i}"]?.ToString() : null,
                    TournamentFrom = jsonData.ContainsKey($"171401_{i}") ? jsonData[$"171401_{i}"]?.ToString() : null,
                    TournamentTo = jsonData.ContainsKey($"171645_{i}") ? jsonData[$"171645_{i}"]?.ToString() : null,
                    Position = GetSafeValueForAchievement(jsonData, $"171402_{i}")
                });
            }

            return result;
        }

        private List<StateAchievements> StateAchievementsDeserializeJsonStreamAsync(string json)
        {
            if (string.IsNullOrEmpty(json) || json == "FieldSetValue")
                return new List<StateAchievements>();

            var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
            var jsonData = data["data"];
            var result = new List<StateAchievements>();

            var detectedKeys = jsonData.Keys
                .Where(k => k.StartsWith("171384_") && int.TryParse(k.Split('_')[1], out _))
                .Select(k => int.Parse(k.Split('_')[1]))
                .ToList();

            int maxIndex = detectedKeys.DefaultIfEmpty(0).Max();

            for (int i = 1; i <= maxIndex; i++)
            {
                result.Add(new StateAchievements
                {
                    State = GetSafeValueForAchievement(jsonData, $"171384_{i}"),
                    Game = GetSafeValueForAchievement(jsonData, $"171385_{i}"),
                    GameCategory = GetSafeValueForAchievement(jsonData, $"171386_{i}"),
                    GameType = GetSafeValueForAchievement(jsonData, $"171387_{i}"),
                    AgeGroup = GetSafeValueForAchievement(jsonData, $"171388_{i}"),
                    GameEvent = GetSafeValueForAchievement(jsonData, $"171389_{i}"),
                    TournamentName = jsonData.ContainsKey($"171390_{i}") ? jsonData[$"171390_{i}"]?.ToString() : null,
                    TournamentFrom = jsonData.ContainsKey($"171391_{i}") ? jsonData[$"171391_{i}"]?.ToString() : null,
                    TournamentTo = jsonData.ContainsKey($"171644_{i}") ? jsonData[$"171644_{i}"]?.ToString() : null,
                    Position = GetSafeValueForAchievement(jsonData, $"171392_{i}")
                });
            }

            return result;
        }

        private List<DistrictAchievements> DistrictAchievementsDeserializeJsonStreamAsync(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "FieldSetValue")
                return new List<DistrictAchievements>();

            var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
            var jsonData = data["data"];
            var result = new List<DistrictAchievements>();

            var detectedKeys = jsonData.Keys
                .Where(k => k.StartsWith("171374_") && int.TryParse(k.Split('_')[1], out _))
                .Select(k => int.Parse(k.Split('_')[1]))
                .ToList();

            int maxIndex = detectedKeys.DefaultIfEmpty(0).Max();

            for (int i = 1; i <= maxIndex; i++)
            {
                result.Add(new DistrictAchievements
                {
                    District = GetSafeValueForAchievement(jsonData, $"171374_{i}"),
                    Game = GetSafeValueForAchievement(jsonData, $"171375_{i}"),
                    GameCategory = GetSafeValueForAchievement(jsonData, $"171376_{i}"),
                    GameType = GetSafeValueForAchievement(jsonData, $"171377_{i}"),
                    AgeGroup = GetSafeValueForAchievement(jsonData, $"171378_{i}"),
                    GameEvent = GetSafeValueForAchievement(jsonData, $"171379_{i}"),
                    TournamentName = jsonData.ContainsKey($"171380_{i}") ? jsonData[$"171380_{i}"]?.ToString() : null,
                    TournamentFrom = jsonData.ContainsKey($"171381_{i}") ? jsonData[$"171381_{i}"]?.ToString() : null,
                    TournamentTo = jsonData.ContainsKey($"171643_{i}") ? jsonData[$"171643_{i}"]?.ToString() : null,
                    Position = GetSafeValueForAchievement(jsonData, $"171382_{i}")
                });
            }

            return result;
        }

        private List<BlockAchievements> BlockAchievementsDeserializeJsonStreamAsync(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "FieldSetValue")
                return new List<BlockAchievements>();

            var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
            var jsonData = data["data"];
            var result = new List<BlockAchievements>();

            var detectedKeys = jsonData.Keys
                .Where(k => k.StartsWith("171363_") && int.TryParse(k.Split('_')[1], out _))
                .Select(k => int.Parse(k.Split('_')[1]))
                .ToList();

            int maxIndex = detectedKeys.DefaultIfEmpty(0).Max();

            for (int i = 1; i <= maxIndex; i++)
            {
                result.Add(new BlockAchievements
                {
                    District = GetSafeValueForAchievement(jsonData, $"171363_{i}"),
                    Block = GetSafeValueForAchievement(jsonData, $"171364_{i}"),
                    Game = GetSafeValueForAchievement(jsonData, $"171365_{i}"),
                    GameCategory = GetSafeValueForAchievement(jsonData, $"171366_{i}"),
                    GameType = GetSafeValueForAchievement(jsonData, $"171367_{i}"),
                    AgeGroup = GetSafeValueForAchievement(jsonData, $"171368_{i}"),
                    GameEvent = GetSafeValueForAchievement(jsonData, $"171369_{i}"),
                    TournamentName = jsonData.ContainsKey($"171370_{i}") ? jsonData[$"171370_{i}"]?.ToString() : null,
                    TournamentFrom = jsonData.ContainsKey($"171371_{i}") ? jsonData[$"171371_{i}"]?.ToString() : null,
                    TournamentTo = jsonData.ContainsKey($"171641_{i}") ? jsonData[$"171641_{i}"]?.ToString() : null,
                    Position = GetSafeValueForAchievement(jsonData, $"171372_{i}")
                });
            }

            return result;
        }

        private string GetSafeValueForAchievement(Dictionary<string, string> data, string key)
        {
            if (data.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
            {
                var parts = value.Split('~');
                return parts.Length > 1 ? parts[1] : null; // Ensure index [1] exists before accessing it
            }
            return null;
        }

        #endregion

        [Route("GetPlayerAchievementByAppRefNo")]
        [HttpGet]
        public async Task<IActionResult> GetPlayerAchievementByAppRefNo(string applRefNo)
        {
            // Build the base query
            var query = from initiatedData in _servicePlusContext.InitiatedDatas
                        where initiatedData.ServiceName.Contains("Punjab Sports Events Portal")
                              && initiatedData.ApplRefNo == applRefNo
                        //&& initiatedData.InitiatedRecordInsertionFlag == 1

                        select new
                        {
                            InitiatedDataId = initiatedData.InitiatedDataId,
                            AttributeDetails = initiatedData.AttributeDetail
                                .Where(attr => new[]
                                {
                            "170041", "171353", "171373", "171383", "171393", "171403"
                                }.Contains(attr.ApplicationFormFieldID))
                                .ToList(),
                            initiatedData.ServiceId,
                            initiatedData.ServiceName,
                            initiatedData.ApplId,
                            initiatedData.ApplRefNo,
                            initiatedData.SubmissionDate
                        };

            // Execute the query and get a single record
            var data = await query.FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound("No achievement records found.");
            }

            // Transform the single record into ViewModel
            var result = new PlayerAchievements
            {
                CompetitionType = CleanValue(data.AttributeDetails
                    .FirstOrDefault(attr => attr.ApplicationFormFieldID == "170041")?.ApplicationFormFieldValue),

                BlockAchievements = BlockAchievementsDeserializeJsonStreamAsync(CleanValue(data.AttributeDetails
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "171353")?.ApplicationFormFieldValue)),

                DistrictAchievements = DistrictAchievementsDeserializeJsonStreamAsync(CleanValue(data.AttributeDetails
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "171373")?.ApplicationFormFieldValue)),

                StateAchievements = StateAchievementsDeserializeJsonStreamAsync(CleanValue(data.AttributeDetails
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "171383")?.ApplicationFormFieldValue)),

                NationalAchievements = NationalAchievementsDeserializeJsonStreamAsync(CleanValue(data.AttributeDetails
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "171393")?.ApplicationFormFieldValue)),

                InterNationalAchievements = InterNationalAchievementsDeserializeJsonStreamAsync(CleanValue(data.AttributeDetails
                        .FirstOrDefault(attr => attr.ApplicationFormFieldID == "171403")?.ApplicationFormFieldValue))
            };
            return Ok(result); // Returns a single object instead of an array
        }


        [Route("AddSponsorPlayer")]
        [HttpPost]
        public async Task<IActionResult> AddSponsorPlayer(SportSponsorDetailViewModel sportSponsorDetailViewModel)
        {
            if (sportSponsorDetailViewModel is null)
            {
                throw new ArgumentNullException(nameof(sportSponsorDetailViewModel));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { IsSucced = false, Message = "Kindly Download your Slip" });
            }

            SportSponsorDetail sportSponsorDetail = _mapper.Map<SportSponsorDetail>(sportSponsorDetailViewModel);

            if (sportSponsorDetail.Id == 0)
            {
                // Check if Email or PhoneNumber already exist
                bool emailExists = await _servicePlusContext.SportSponsorDetails
                                         .AnyAsync(s => s.Email == sportSponsorDetailViewModel.Email);
                bool phoneExists = await _servicePlusContext.SportSponsorDetails
                                          .AnyAsync(s => s.PhoneNumber == sportSponsorDetailViewModel.PhoneNumber);

                if (emailExists)
                {
                    return BadRequest(new { IsSucced = false, Message = "Email already exists." });
                }

                if (phoneExists)
                {
                    return BadRequest(new { IsSucced = false, Message = "Phone number already exists." });
                }
                // Save SportSponsorDetail first to generate an Id
                await _servicePlusContext.SportSponsorDetails.AddAsync(sportSponsorDetail);

                await _servicePlusContext.SponsorPlayers.AddRangeAsync(sportSponsorDetail.SponsorPlayers);
                await _servicePlusContext.SaveChangesAsync();

            }
            else
            {
                // Assign the correct SportSponsorDetailId to the players
                sportSponsorDetail.SponsorPlayers.ToList().ForEach(player => player.SportSponsorDetailId = sportSponsorDetail.Id);

                await _servicePlusContext.SponsorPlayers.AddRangeAsync(sportSponsorDetail.SponsorPlayers);
                await _servicePlusContext.SaveChangesAsync();

            }


            return Ok(new { IsSucced = true, Message = "Kindly Download your Slip" });
        }


        [Route("GetSponsorByPhoneNumber")]
        [HttpGet]
        public async Task<IActionResult> GetSponsorByPhoneNumber(string phoneNumber)
        {
            var sponsorDetails = await _servicePlusContext.SportSponsorDetails
                .SingleOrDefaultAsync(x => x.PhoneNumber == phoneNumber);

            if (sponsorDetails == null)
            {
                return NotFound("Phone Number Doesn't Exist");
            }

            return Ok(sponsorDetails);
        }

        [Route("GetSponsorPlayersByRefNo")]
        [HttpPost]
        public async Task<IActionResult> GetSponsorPlayersByRefNo([FromBody] List<string> applRefNos)
        {
            if (applRefNos == null || applRefNos.Count == 0)
            {
                return BadRequest(new { message = "No application reference numbers provided" });
            }

            // Convert to HashSet for faster lookup
            var applRefNoSet = new HashSet<string>(applRefNos);

            var query = await _servicePlusContext.InitiatedDatas
                .Where(initiatedData =>
                    initiatedData.ServiceName.Contains("Punjab Sports Events Portal") &&
                    applRefNoSet.Contains(initiatedData.ApplRefNo) &&
                    initiatedData.InitiatedRecordInsertionFlag == 1)
                .Select(initiatedData => new
                {
                    initiatedData.InitiatedDataId,
                    initiatedData.ServiceId,
                    initiatedData.ServiceName,
                    initiatedData.ApplId,
                    initiatedData.ApplRefNo,
                    initiatedData.SubmissionDate,
                    AttributeDetails = initiatedData.AttributeDetail
                        .Where(attr => new HashSet<string>
                        {
                    "169958", "169959", "169983", "171761", "169987",
                    "169984", "169988", "169985"
                        }.Contains(attr.ApplicationFormFieldID))
                        .ToList()
                })
                .ToListAsync();

            if (!query.Any())
            {
                return NotFound(new { message = "No records found" });
            }

            var result = query.Select(q => new
            {
                initiatedDataId = q.InitiatedDataId,
                applRefNo = q.ApplRefNo,
                applicantMobileNo = GetValue(q.AttributeDetails, "169958"),
                applicantEmail = GetValue(q.AttributeDetails, "169959"),
                accountNumber = GetValue(q.AttributeDetails, "169983"),
                accountHolder = GetValue(q.AttributeDetails, "171761")?.Split('~').Last(),
                ifscCode = GetValue(q.AttributeDetails, "169987"),
                nameOnPassbook = GetValue(q.AttributeDetails, "169984"),
                bankAddress = GetValue(q.AttributeDetails, "169988"),
                bankName = GetValue(q.AttributeDetails, "169985"),
            }).ToList();

            return Ok(result);
        }

        #endregion

        #region GetPlayerCertificateDetail

        [Route("GetPlayerCertificateDetail")]
        [HttpPost]
        public async Task<IActionResult> GetPlayerCertificateDetail(string district, string gameName, string AgeGroup)
        {

            return Ok(await GeneratePlayerCertificate(district, gameName, AgeGroup) + " Record Updated Successfully");

        }
        [Route("UpdateCertificatePlayers")]
        [HttpPost]
        public async Task<IActionResult> UpdateCertificatePlayers()
        {

            var googleSheetsService = new GoogleSheetsService(_servicePlusContext); // Pass the context here
            var playerDetails = await googleSheetsService.GetFilteredPlayerCertificateDetails();

            if (playerDetails)
            {

                return Ok("Record Updated Successfully");
            }
            else
            {
                return BadRequest(new { message = "No record found" });
            }
        }

        private async Task<string> GeneratePlayerCertificate(string districtName, string gameName, string ageGroup)
        {
            // District-wise serial number prefixes to create Folder Name
            var districtPrefixes = new Dictionary<string, string>
    {
        { "PATIALA", "PAT" },
        { "AMRITSAR", "AMR" },
        { "BATHINDA", "BAT" },
        { "LUDHIANA", "LUD" }
        // Add more districts as needed
    };

            // Get the prefix for the given district, default to "GEN000" if not found
            string randomDistrictSr = districtPrefixes.ContainsKey(districtName.ToUpper())
                ? districtPrefixes[districtName.ToUpper()]
                : "GEN000";
             
            // Fetch issued certificates first (executed on DB)
            var existingCertificates = await _servicePlusContext.PlayerIssuedCertificate
                .Where(c => c.GameHeldDistrict == districtName
                            && c.ApplicantGame == gameName
                            && c.ApplicantAgeGroup == ageGroup
                            && c.CertificateSerialNo != null)
                .Select(c => new
                {
                    c.ApplicantFullName,
                    c.ApplicantFatherName,
                    c.ApplicantDOB,
                    c.ApplicantGame,
                    c.ApplicantEvent,
                    c.ApplicantAgeGroup
                })
                .ToListAsync(); // Move data to memory

            // Fetch all players (executed on DB)
            var allPlayers = await _servicePlusContext.PlayerCertificateDetails
                .Where(d => d.GameHeldDistrict == districtName
                            && d.ApplicantGame == gameName
                            && d.ApplicantAgeGroup == ageGroup)
                .ToListAsync(); // Move data to memory

            // Perform filtering in memory (LINQ to Objects)
            var playerCertificateDetails = allPlayers
                .Where(d => !existingCertificates.Any(c =>
                    c.ApplicantFullName == d.ApplicantFullName &&
                    c.ApplicantFatherName == d.ApplicantFatherName &&
                    c.ApplicantDOB == d.ApplicantDOB &&
                    c.ApplicantGame == d.ApplicantGame &&
                    c.ApplicantEvent == d.ApplicantEvent &&
                    c.ApplicantAgeGroup == d.ApplicantAgeGroup))
                .ToList(); // Filtering done in memory


            if (!playerCertificateDetails.Any())
            {
                return "No new certificates to generate.";
            }
            //  var getSigns = playerCertificateDetails.FirstOrDefault();

            // Signature For Convenor
            // Define the base directory where images are stored
            string baseDirectory = @"http://10.147.24.36:8082/SSD/SportsSignature";

            // Dictionary to store (district, game) as key and image path as value
            Dictionary<(string, string), string> gameSignatures = new Dictionary<(string, string), string>
{
                    //Amritsar
                    { ("Amritsar", "Gatka"), $@"{baseDirectory}\Amritsar\Gatka Convenor Sign\dummy.png" },
                    { ("Amritsar", "Rugby"), $@"{baseDirectory}\Amritsar\Rugby Convenor Sign\dummy.png" },

                    //Barnala
                    { ("Barnala", "Netball"), $@"{baseDirectory}\Barnala\Netball Convenor Sign\NET-remove.png" },
                    { ("Patiala", "Table Tennis"), $@"{baseDirectory}\Barnala\Table Tennis Convenor Sign\TT-remove.png" },

                    //Bathinda
                    { ("Bathinda", "Hockey"), $@"{baseDirectory}\Bathinda\Hocky Convenor Sign\HOCKEY-remove.png" },
                    { ("Bathinda", "Powerlifting"), $@"{baseDirectory}\Bathinda\Powerlifting Convenor Sign\POWERLIFTING-remove.png" },

                    //Faridkot
                    { ("Faridkot", "Basketball"), $@"{baseDirectory}\Faridkot\Basketball Convenor Sign\dummy.png" },
                    { ("Faridkot", "Taekwondo"), $@"{baseDirectory}\Faridkot\Taekwondo Convenor Sign\dummy.png" },

                    //Fatehgarh Sahib
                    { ("Fatehgarh Sahib", "Fencing"), $@"{baseDirectory}\Fatehgarh Sahib\Fencing Convenor Sign\FENCING-remove.png" },
                    { ("Fatehgarh Sahib", "Softball"), $@"{baseDirectory}\Fatehgarh Sahib\Softball Convenor Sign\SOFT-removebg.png" },

                    //Hoshiarpur
                    { ("Hoshiarpur", "Football"), $@"{baseDirectory}\Hoshiarpur\Football Convenor Sign\dummy.png" },

                    //Jalandhar
                    { ("Jalandhar", "Chess"), $@"{baseDirectory}\Jalandhar\Chess Convenor Sign\Chess_Convener-remove.png" },
                    { ("Jalandhar", "Volleyball Smashing"), $@"{baseDirectory}\Jalandhar\Volleyball Smashing Convenor Sign\Volleyball_Smashing_Convener_sign-remove.png" },

                    //Ludhiana
                    { ("Ludhiana", "Athletics"), $@"{baseDirectory}\Ludhiana\Athletics Convenor Sign\ATHLETICS-removebg-preview.png" },
                    { ("Ludhiana", "Baseball"), $@"{baseDirectory}\Ludhiana\Baseball Convenor Sign\BASEBALL-removebg-preview.png" },
                    { ("Ludhiana", "Cycling"), $@"{baseDirectory}\Ludhiana\Cycling Convenor Sign\CYCLING-removebg-preview.png" },
                    { ("Ludhiana", "Kick Boxing"), $@"{baseDirectory}\Ludhiana\Kick Boxing Convenor Sign\KICKBOXING-removebg-preview.png" },
                    { ("Ludhiana", "Lawn Tennis"), $@"{baseDirectory}\Ludhiana\Lawn Tennis Convenor Sign\LAWN_TENNIS-removebg-preview.png" },

                    //Malerkotla
                    { ("Malerkotla", "Volleyball Shooting"), $@"{baseDirectory}\Malerkotla\Volleyball Shooting Convenor Sign\dummy.png" },

                    //Mansa
                    { ("Mansa", "Judo"), $@"{baseDirectory}\Mansa\Judo Convenor Sign\JUDO-removebg-preview.png" },
                    { ("Mansa", "Wrestling"), $@"{baseDirectory}\Mansa\Wrestling Convenor Sign\dummy.png" },

                    //Patiala
                    { ("PATIALA", "Archary"), $@"{baseDirectory}\Patiala\Archary Convenor Sign\ARCHERY-removebg-preview.png" },
                    { ("PATIALA", "Gymnastics"), $@"{baseDirectory}\Patiala\Gymnastics Convenor Sign\GYMNASTICS-removebg-preview.png" },
                    { ("PATIALA", "KABADDI CIRCLE"), $@"{baseDirectory}\Patiala\Kabbadi circle style Convenor Sign\KABADDI_CS-removebg-preview.png" },
                    { ("PATIALA", "Kho-Kho"), $@"{baseDirectory}\Patiala\Kho-Kho Convenor Sign\KHO_KHO-removebg-preview.png" },

                    //Rupnagar
                    { ("Rupnagar", "Handball"), $@"{baseDirectory}\Rupnagar\Handball Convenor Sign\dummy.png" },
                    { ("Rupnagar", "Kayking"), $@"{baseDirectory}\Rupnagar\Kayking Convenor Sign\dummy.png" },
                    { ("Rupnagar", "Rowing"), $@"{baseDirectory}\Rupnagar\Rowing Convenor Sign\dummy.png" },
                    
                    //Sangrur
                    { ("Sangrur", "Kabaddi"), $@"{baseDirectory}\Sangrur\Kabaddi National Style Convenor Sign\dummy.png" },
                    { ("Sangrur", "Roller Skating"), $@"{baseDirectory}\Sangrur\Roller Skating Convenor Sign\RS-removebg-preview.png" },
                    { ("Sangrur", "Roller Skating Speed Skating"), $@"{baseDirectory}\Sangrur\Roller Skating Speed Skating Convenor Sign\dummy.png" },
                    { ("Sangrur", "Weightlifting"), $@"{baseDirectory}\Sangrur\Weightlifting Convenor SIgn\WL-removebg-preview.png" },
                    { ("Sangrur", "Wushu"), $@"{baseDirectory}\Sangrur\Wushu Convenor Sign\WUSHU-removebg-preview.png" },

                    //SAS Nagar
                    { ("SAS Nagar", "Equestrian"), $@"{baseDirectory}\SAS Nagar\Equestrian Convenor Sign\dummy.png" },
                    { ("SAS Nagar", "Shooting"), $@"{baseDirectory}\SAS Nagar\Shooting Convenor Sign\dummy.png" },
                    { ("SAS Nagar", "Swimming"), $@"{baseDirectory}\SAS Nagar\Swimming Convenor Sign\dummy.png" },

                    //SAS Nagar
                    { ("SBS Nagar", "Boxing"), $@"{baseDirectory}\SBS Nagar\Boxing Convenor Sign\dummy.png" },

                };

            // Normalize input (Trim spaces and capitalize first letter)
            districtName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(districtName);
            gameName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(gameName);

            // Try to get the image path from the dictionary
            if (!gameSignatures.TryGetValue((districtName, gameName), out string ConveyorImagePath))
            {
                ConveyorImagePath = "./images/default-sign.png"; // Fallback image if not found
            }




            Dictionary<string, string> dsoSignatures = new Dictionary<string, string>
{
                        { "Amritsar", $@"{baseDirectory}\Amritsar\DSO Sign\Dso_Amritsar_official_signature-remove.png" },
                        { "Barnala", $@"{baseDirectory}\Barnala\DSO Sign\dso_barnala_signs-remove.png" },
                        { "Bathinda", $@"{baseDirectory}\Bathinda\DSO Sign\dummy.png" },
                        { "Faridkot", $@"{baseDirectory}\Faridkot\DSO Sign\Dso_Faridkot_Official_Signature-remove.png" },
                        { "Fatehgarh Sahib", $@"{baseDirectory}\Fatehgarh Sahib\DSO Sign\dummy.png" },
                        { "Hoshiarpur", $@"{baseDirectory}\Hoshiarpur\DSO Sign\dummy.png" },
                        { "Jalandhar", $@"{baseDirectory}\Jalandhar\DSO Sign\dso_jalandhar_signs-remove.png" },
                        { "Ludhiana", $@"{baseDirectory}\Ludhiana\DSO Sign\Dso_Ludhiana_official_signature-remove.png" },
                        { "Malerkotla", $@"{baseDirectory}\Malerkotla\DSO Sign\dso_malerkotla_official_signature.png-removebg-preview.png" },
                        { "Mansa", $@"{baseDirectory}\Mansa\DSO Sign\dso_mansa_sign-removebg-preview.png" },
                        { "PATIALA", $@"{baseDirectory}\Patiala\DSO Sign\Dso_Patiala_Official_Signature-removebg-preview.png" },
                        { "Rupnagar", $@"{baseDirectory}\Rupnagar\DSO Sign\dummy.png" },
                        { "Sangrur", $@"{baseDirectory}\Sangrur\DSO Sign\dummy.png" },
                        { "SAS Nagar", $@"{baseDirectory}\SAS Nagar\DSO Sign\MOHALI-removebg-preview.png" },
                        { "SBS Nagar", $@"{baseDirectory}\SBS Nagar\DSO Sign\Dso_SBS_Nagar_official_signature-remove.png" }
                    };

            districtName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(districtName);

            // Try to get the image path from the dictionary
            if (!dsoSignatures.TryGetValue((districtName), out string dsoImagePath))
            {
                dsoImagePath = "./images/default-sign.png"; // Fallback image if not found
            }

            // Get last serial number and generate a new one
            var lastIssuedCertificate = await _servicePlusContext.PlayerIssuedCertificate
                                         .Where(c => c.GameHeldDistrict == districtName) // Filter by district
                                         .OrderByDescending(c => c.CertificateSerialNo)
                                         .Select(d => d.CertificateSerialNo)
                                         .FirstOrDefaultAsync();


            int newSerialNumber = lastIssuedCertificate != null && int.TryParse(lastIssuedCertificate, out int lastSerial)
                ? lastSerial + 1
                : 1;


            var newCertificates = new List<PlayerIssuedCertificate>();

            foreach (var player in playerCertificateDetails)
            {
                string startDate = "01-01-2024";
                string endDate = "31-12-2024";
                // Ensure a unique 6-digit serial number
                //string certificateNo = newSerialNumber.ToString("D6");
                int currentYear = DateTime.Now.Year;
                string formattedGameName = gameName.Replace(" ", ""); // Remove spaces
                string certificateNo = $"{randomDistrictSr}-2024-{newSerialNumber:D6}";
                // Define folder hierarchy
                string baseFolder = "GeneratedCertificates"; // First folder
                string districtFolder = $"{districtName}"; // Second folder
                string gameFolder = gameName; // Third folder
                string ageGroupFolder = ageGroup; // Fourth folder

                // Combine paths to create full directory structure
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), baseFolder, districtFolder, gameFolder, ageGroupFolder);

                // Check if directory exists, if not, create it
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Define certificate filename
                string fileName = $"{certificateNo}.pdf";
                string filePath = Path.Combine(folderPath, fileName);
                var googleSheetsService = new GoogleSheetsService(_servicePlusContext); // Pass the context here
                await googleSheetsService.UpdateCertificateDetails(
                    player.SrNo,
                    $"{baseFolder}\\{districtFolder}\\{gameFolder}\\{ageGroupFolder}\\{certificateNo}",
                    certificateNo
                );

                // Puppeteer PDF Generation Logic
                await new BrowserFetcher().DownloadAsync();
                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] {
                "--font-render-hinting=none",
                "--force-color-profile=srgb"
            }
                });

                await using var page = await browser.NewPageAsync();
                await page.SetUserAgentAsync("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36");
                await page.EmulateMediaTypeAsync(MediaType.Screen);

                // Further processing...



                string htmlContent = $@"<html>
        <head>
            <style>
                body {{ margin: 0; padding: 0; font-family: Arial, sans-serif; }}
                #certificate-container {{
                    position: fixed;
                    width: 100%;
                    height: 100%;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    text-align: center;
                }}
                #background-img {{
                    position: fixed;
                    padding-left:6px;
                    padding-right:4px;
                    padding-top:6px;
                    padding-bottom:6px;

                    width: 99%;
                    height: 98%;
                }}
                .text-bold {{ font-weight: bold; }}
            </style>
        </head>
        <body>
            <img id='background-img' src='http://10.147.24.36:8082/SSD/SportsCertificateBgNew.png' />
            <div id='certificate-container'>
                <div style='position: absolute; top: 16%; left: 10%; width: 80%; height:100%; padding: 30px; border-radius: 10px; box-sizing: border-box; text-align: center;'>
                    <div style='margin: 8px 0; font-size: 16px; font-weight: bold; position: absolute; top: -12%; right: 3%;'>
                        ਸਰਟੀਫਿਕੇਟ ਨੰ. : <u>{certificateNo}</u>
                    </div>
                    <div class='text-bold' style=' font-size: 25px; font-weight: bold; padding-top: 2px;'>ਖੇਡਾਂ ਅਤੇ ਯੁਵਾ ਮਾਮਲੇ ਵਿਭਾਗ</div>
<img style='width: 42%;height: 3%;' src='http://10.147.24.36:8082/SSD/arrow.png'>
                    <div style='margin: 3px 0;  '>
                        <span style='font-size:35px; color: #3d387c;'><strong>ਖੇਡਾਂ ਵਤਨ ਪੰਜਾਬ ਦੀਆਂ 2024</strong></span>
                    </div>
    <div style='margin: 5px 0; font-size: 22px; margin-top: 3px; font-weight: bold;'>ਮੈਰਿਟ ਸਰਟੀਫਿਕੇਟ</div>
              <div style='
            display: inline-block; 
            background-color: #d32f2f; 
            color: white; 
            padding: 6px 18px; 
            border-radius: 20px 0 20px 0; 
            font-size: 16px; 
            font-weight: bold; 
            font-family: 'Gurmukhi', Arial, sans-serif;'>ਰਾਜ ਪੱਧਰੀ ਟੂਰਨਾਮੈਂਟ</div>
              
             <div style=' font-size: 20px; margin-top: 3px; font-weight: bold;'>{await TranslateToPunjabi(player.GameHeldDistrict)}</div>
                    <div style='margin: 10px 0; font-size: 18px; font-weight: bold;'>
                        ਮਿਤੀ ਤੋਂ <strong>{startDate}</strong> ਮਿਤੀ ਤੱਕ <strong>{endDate}</strong>
                    </div> 
                   <div style='text-align: justify; margin-top: 5px; font-size: 16px;line-height:2.5;'>
                            ਤਸਦੀਕ ਕੀਤਾ ਜਾਂਦਾ ਹੈ ਕਿ 
                            <strong>
                                <span style='display: inline-block; width: 83%; text-align: center;  border-bottom: 0.5px dashed #000;min-height: 16px; line-height: 16px; padding-bottom: 2px;'>
                                    {await TranslateToPunjabi(player.ApplicantFullName)}
                                </span>
                            </strong><br>
                            ਪੁੱਤਰ/ਪੁਤਰੀ ਸ਼੍ਰੀ 
                            <strong><span style='display: inline-block; width: 39%; text-align: center; border-bottom:0.2px dashed #000; min-height: 16px; line-height: 16px; padding-bottom: 2px;'>{await TranslateToPunjabi(player.ApplicantFatherName)}</span></strong>
                            ਜਿਨ੍ਹਾਂ ਦੀ ਜਨਮ ਮਿਤੀ 
                            <strong><span style='display: inline-block; width: 38%; text-align: center; border-bottom: 0.3px dashed #000;min-height: 16px; line-height: 16px; padding-bottom: 2px;'>{player.ApplicantDOB}</span></strong><br>
                            ਨੇ ਪੰਜਾਬ ਰਾਜ ਖੇਡਾ - 2024 ਵਿੱਚ ਜ਼ਿਲ੍ਹਾ 
                            <strong><span style='display: inline-block; width: 76%; text-align: center; border-bottom: 0.4px dashed #000;min-height: 16px; line-height: 16px; padding-bottom: 2px;'>{await TranslateToPunjabi(player.GameRepresentingDistrict)}</span></strong> <br>
                            ਵਲੋਂ ਖੇਡ 
                            <strong><span style='display: inline-block; width: 44%; text-align: center; border-bottom: 0.6px dashed #000;min-height: 16px; line-height: 16px; padding-bottom: 2px;'>{await TranslateToPunjabi(player.ApplicantGame)}</span></strong>  
                            ਵਿਵੇਟ/ਵਰਗ 
                            <strong><span style='display: inline-block; width: 41%; text-align: center; border-bottom: 0.7px dashed #000;min-height: 16px; line-height: 16px; padding-bottom: 2px;'>{await TranslateToPunjabi(player.ApplicantEvent)}</span></strong> <br>
                            ਈਵੈਂਟ ਸਮਾਂ/ਦੂਰੀ/ਉਚਾਈ/ਭਾਰ 
                            <strong><span style='display: inline-block; width: 35%; text-align: center; border-bottom: 0.8px dashed #000;min-height: 16px; line-height: 16px; padding-bottom: 2px;'>{await TranslateToPunjabi(player.Score)}</span></strong>  
                            ਵਿਚ ਭਾਗ ਲਿਆ ਅਤੇ 
                            <strong><span style='display: inline-block; width: 22%; text-align: center; border-bottom: 0.9px dashed #000;min-height: 16px; line-height: 16px; padding-bottom: 2px;'>{player.Position}</span></strong>  
                            ਪੁਜੀਸ਼ਨ ਪ੍ਰਾਪਤ ਕੀਤੀ <br>
                            ਉਮਰ ਵਰਗ 
                            <strong><span style='display: inline-block; width: 45%; text-align: center; border-bottom: 1.5px dashed #000;min-height: 16px; line-height: 16px; padding-bottom: 2px;'>{await TranslateToPunjabi(player.ApplicantAgeGroup)}</span></strong>
                        <strong><span style='display:  inline-block; width: 45%; text-align: center; border-bottom: 0px dashed #000;'> </span></strong>
                     </div>                

                    <div style='display: flex; justify-content: space-between; align-items: center; margin: 60px 0 0; text-align: center; flex-direction: column; position: relative;'>

            <!-- Image Section -->
            <div style='display: flex; justify-content: space-between; width: 100%; position: relative;'>
                <div style='width: 33.33%; position: relative;'>
                    <img src='{ConveyorImagePath}' style='width: 32%; display: block; margin: 0 auto; position: absolute; top: -38px; left: 50%; transform: translateX(-50%); z-index: 1;' />
                </div>
                <div style='width: 33.33%; position: relative;'>
                    <img src='{dsoImagePath}' style='width: 32%; display: block; margin: 0 auto; position: absolute; top: -43px; left: 50%; transform: translateX(-50%); z-index: 1;' />
                </div>
                <div style='width: 35%; position: relative;'>
                    <img src='{dsoImagePath}' style='width: 32%; display: block; margin: 0 auto; position: absolute; top: -43px; left: 50%; transform: translateX(-50%); z-index: 1;' />
                </div>
            </div>
        
            <!-- Text Section -->
            <div style='display: flex; justify-content: space-between; width: 100%; position: relative;'>
                <div style='width: 33.33%; position: relative;'>
                    <span style='font-size: 18px; display: block;'>ਕਨਵੀਨਰ</span>
                </div>
                <div style='width: 33.33%; position: relative;'>
                    <span style='font-size: 18px; display: block;'>ਜ਼ਿਲ੍ਹਾ ਖੇਡ ਅਫ਼ਸਰ</span>
                </div>
                <div style='width: 35%; text-align:center; position: relative;'>
                    <span style='font-size: 18px; display: block;'>ਡਾਇਰੈਕਟਰ ਸਪੋਰਟਸ <br />ਪੰਜਾਬ</span>
                </div>
            </div>
        
        </div>
                </div>
            </div>
        </body>
        </html>";

                await page.SetContentAsync(htmlContent);

                await page.PdfAsync(filePath, new PdfOptions
                {
                    PrintBackground = true,
                    Format = PaperFormat.Legal,
                    Landscape = true,
                    Width = "90%",
                });

                // Add the new record to the list
                newCertificates.Add(new PlayerIssuedCertificate
                {
                    GameHeldDistrict = districtName,
                    ApplicantGame = gameName,
                    ApplicantAgeGroup = ageGroup,
                    ApplicantFullName = player.ApplicantFullName,
                    ApplicantFatherName = player.ApplicantFatherName,
                    ApplicantDOB = player.ApplicantDOB,
                    ApplicantEvent = player.ApplicantEvent,
                    CertificateSerialNo = certificateNo,
                    CertificatePath = filePath
                });
                newSerialNumber++; // Increment serial number for the next certificate
            }
            // **Save all records at once**
            if (newCertificates.Any())
            {
                await _servicePlusContext.PlayerIssuedCertificate.AddRangeAsync(newCertificates);
                await _servicePlusContext.SaveChangesAsync();
            }
            return newCertificates.Count.ToString();
        }

        private async Task<string> TranslateToPunjabi(string text)
        {
            using HttpClient client = new HttpClient();
            string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=pa&dt=t&q={text}";

            var response = await client.GetStringAsync(url);
            var jsonData = System.Text.Json.JsonSerializer.Deserialize<object[]>(response);
            var translatedText = ((JsonElement)jsonData[0]).EnumerateArray().First().EnumerateArray().First().GetString();


            return translatedText;
            // return text;
        }

         
        #endregion

        #region Under Development
        //    #region Dynamic Report using Service Name

        //    [HttpGet]
        //    [Route("DynamicReportServiceWise")]
        //    public async Task<IActionResult> DynamicReportServiceWise(
        //[FromQuery] List<string> selectedColumns,
        //[FromQuery] string serviceName,
        //[FromQuery] string? fromDate,
        //[FromQuery] string? toDate,
        //[FromQuery] int? draw,
        //[FromQuery] int? start,
        //[FromQuery] int? length)
        //    {
        //        var query = _servicePlusContext.InitiatedDatas.AsQueryable();

        //        if (selectedColumns != null && selectedColumns.Any())
        //        {
        //            // Create a dynamic projection expression for selected columns
        //            var parameter = Expression.Parameter(typeof(InitiatedData));
        //            var bindings = new List<MemberBinding>();

        //            foreach (var columnName in selectedColumns)
        //            {
        //                var propertyInfo = typeof(InitiatedData).GetProperty(columnName);
        //                var memberAccess = Expression.MakeMemberAccess(parameter, propertyInfo);
        //                bindings.Add(Expression.Bind(propertyInfo, memberAccess));
        //            }

        //            var memberInit = Expression.MemberInit(Expression.New(typeof(InitiatedData)), bindings);
        //            var lambda = Expression.Lambda<Func<InitiatedData, InitiatedData>>(memberInit, parameter);

        //            query = query.Where(data => data.ServiceName == serviceName);

        //            // Filter the results by date range if provided
        //            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
        //            {
        //                if (DateTime.TryParse(fromDate, out var from) && DateTime.TryParse(toDate, out var to))
        //                {
        //                    from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        //                    to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        //                    query = query.Where(data => data.SubmissionDate >= from && data.SubmissionDate <= to);
        //                }
        //            }

        //            // Apply the dynamic projection to the query
        //            query = query.Select(lambda);
        //        }

        //        // Apply pagination
        //        if (start.HasValue && length.HasValue)
        //        {
        //            query = query.Skip(start.Value).Take(length.Value);
        //        }

        //        var result = await query.ToListAsync();
        //        return Ok(result);
        //    }



        //    #endregion


        [HttpGet]
        [Route("DynamicReportServiceWise")]
        public async Task<IActionResult> DynamicReportServiceWise(
    [FromQuery] List<string> selectedColumns,
    [FromQuery] string serviceName,
    [FromQuery] string? fromDate,
    [FromQuery] string? toDate,
    [FromQuery] int? draw,
    [FromQuery] int? start,
    [FromQuery] int? length)
        {
            var query = _servicePlusContext.InitiatedDatas.AsQueryable();

            if (selectedColumns != null && selectedColumns.Any())
            {
                var parameter = Expression.Parameter(typeof(InitiatedData));
                var propertyInfos = selectedColumns.Select(columnName => typeof(InitiatedData).GetProperty(columnName)).ToList();

                var bindings = propertyInfos.Select(propertyInfo =>
                {
                    var memberAccess = Expression.MakeMemberAccess(parameter, propertyInfo);
                    return Expression.Bind(propertyInfo, memberAccess);
                }).ToList();

                var memberInit = Expression.MemberInit(Expression.New(typeof(InitiatedData)), bindings);
                var lambda = Expression.Lambda<Func<InitiatedData, InitiatedData>>(memberInit, parameter);

                query = query.Where(data => data.ServiceName == serviceName);

                // Filter the results by date range if provided
                if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                {
                    if (DateTime.TryParse(fromDate, out var from) && DateTime.TryParse(toDate, out var to))
                    {
                        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
                        to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

                        query = query.Where(data => data.SubmissionDate >= from && data.SubmissionDate <= to);
                    }
                }

                // Apply the dynamic projection to the query
                query = query.Select(lambda);
            }

            // Apply pagination
            if (start.HasValue && length.HasValue)
            {
                query = query.Skip(start.Value).Take(length.Value);
            }

            var result = await query.ToListAsync();

            return Ok(result);
        }
        #endregion
    }



}

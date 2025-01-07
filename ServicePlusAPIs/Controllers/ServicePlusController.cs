using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicePlusAPIs.AuthenticateModels;
using ServicePlusAPIs.Context;
using ServicePlusAPIs.HelperModels;
using ServicePlusAPIs.HelperViewModel;
using ServicePlusAPIs.Models;
using ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel;
using ServicePlusAPIs.Models.EnclouserDetails;
using ServicePlusAPIs.Models.ExecutionModel;
using ServicePlusAPIs.Models.InitiatedModel;
using ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Execution_OfficialFormDetails;
using ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Initiated_AttributeDetails;
using ServicePlusAPIs.ReportsModel;
using ServicePlusAPIs.ReportsViewModel;
using ServicePlusAPIs.ViewModels;
using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

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
            string json = JsonSerializer.Serialize(serviceViewModel, new JsonSerializerOptions
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
            string json = JsonSerializer.Serialize(serviceViewModel, new JsonSerializerOptions
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
                            ApiNames apiName = new ApiNames()
                            {
                                ApiName = method.Name
                            };

                            // Check if the apiName already exists in the database
                            var existingApiName = _servicePlusContext.ApiNames.FirstOrDefault(an => an.ApiName == apiName.ApiName);
                            if (existingApiName == null)
                            {
                                apiNames.Add(apiName);
                            }
                        }
                    }
                }
            }
            if (apiNames.Count > 0)
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

    }



}

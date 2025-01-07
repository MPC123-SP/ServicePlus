

using AutoMapper;
using ServicePlusAPIs.AuthenticateModels;
using ServicePlusAPIs.AuthenticateViewModels;
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
using ServicePlusAPIs.UserModels;
using ServicePlusAPIs.UserViewModels;
using ServicePlusAPIs.ViewModels;
using ServicePlusAPIs.ViewModels.CommonModel.ExecutionCommonModel;
using ServicePlusAPIs.ViewModels.EnclouserDetails;
using ServicePlusAPIs.ViewModels.ExecutionModel;
using ServicePlusAPIs.ViewModels.InitiatedModel;
using ServicePlusAPIs.ViewModels.ServiceWiseModels.PSEB_Execution_OfficialFormDetails;
using System.Globalization;

namespace ServicePlusAPIs.Helper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            #region APINameViewModel , APIName
            CreateMap<ApiNameViewModel, ApiNames>()
                .ForMember(dest => dest.ApiName, opt => opt.MapFrom(src => src.ApiName))
                .ForMember(dest=>dest.ApiDescription,opt=>opt.MapFrom(src=>src.ApiDescription))
                .ReverseMap();

            #endregion

            #region UserLoginViewModel, UserLogin
            CreateMap<UserLoginViewModel, UserLogin>()
             .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
             .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password)) 
          .ReverseMap();
            #endregion

            #region RegisterUserViewModel, RegisterUser
            CreateMap<RegisterUserViewModel, RegisterUser>()
                        
             .ReverseMap();
            #endregion

            #region RolesViewModel, Roles
            CreateMap<RolesViewModel, Roles>()
             .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role)) 
          .ReverseMap();
            #endregion
             

            #region ServiceViewModel, ServiceModel
            //Map from serviceViewModel to ServiceModel
            CreateMap<ServiceViewModel, ServiceModel>()
                .ForMember(dest => dest.InitiatedData, opt => opt.MapFrom(src => src.initiated_data))
                .ForMember(dest => dest.ExecutionData, opt => opt.MapFrom(src => src.execution_data))
             .ReverseMap();
            #endregion

            #region InitiatedDataViewModel, InitiatedData
            //Initiated Model Mapping Start from here

            _ = CreateMap<InitiatedDataViewModel, InitiatedData>()
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.department_id))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.department_name))
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.service_id))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.service_name))
                .ForMember(dest => dest.ApplId, opt => opt.MapFrom(src => Convert.ToInt32(src.appl_id)))
                .ForMember(dest => dest.ApplRefNo, opt => opt.MapFrom(src => src.appl_ref_no))
                .ForMember(dest => dest.NoOfAttachment, opt => opt.MapFrom(src => src.no_of_attachment))
                .ForMember(dest => dest.SubmissionMode, opt => opt.MapFrom(src => src.submission_mode))
                .ForMember(dest => dest.SubmissionDate, opt => opt.MapFrom(src => ConvertStringToUtcDateTime(src.submission_date)))
                .ForMember(dest => dest.AppliedBy, opt => opt.MapFrom(src => src.applied_by))
                .ForMember(dest => dest.SubmissionLocation, opt => opt.MapFrom(src =>  src.submission_location))
                .ForMember(dest => dest.SubmissionLocationId, opt => opt.MapFrom(src => src.submission_location_id))
                .ForMember(dest => dest.SubmissionLocationTypeId, opt => opt.MapFrom(src => src.submission_location_type_id))
                .ForMember(dest => dest.PaymentMode, opt => opt.MapFrom(src => src.payment_mode))
                .ForMember(dest => dest.ReferenceNo, opt => opt.MapFrom(src => src.reference_no))
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => ConvertStringToUtcDateTime(src.payment_date)))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.amount))
                .ForMember(dest => dest.RegistrationId, opt => opt.MapFrom(src => src.registration_id))
                .ForMember(dest => dest.BaseServiceId, opt => opt.MapFrom(src => src.base_service_id))
                .ForMember(dest => dest.VersionNo, opt => opt.MapFrom(src => src.version_no))
                .ForMember(dest => dest.SubVersion, opt => opt.MapFrom(src => src.sub_version))                
                .AfterMap((src, dest) =>
               {
                   if (src.enclosure_details != null)
                   {
                       dest.EnclosureDetails = new List<EnclosureDetail>();
                       foreach (var item in src.enclosure_details)
                       {
                           foreach (var item1 in item)
                               dest.EnclosureDetails.Add(new EnclosureDetail
                               {
                                   EnclousersId = item1.Key,
                                   EnclousersValue = item1.Value
                               });
                       }
                   }
               })
               .AfterMap((src, dest) =>
               {
                   if (src.attribute_details != null)
                   {
                       dest.AttributeDetail = new List<AttributeDetail>();
                       foreach (var item in src.attribute_details)
                       {
                           dest.AttributeDetail.Add(new AttributeDetail
                           {
                               ApplicationFormFieldID = item.Key,
                               ApplicationFormFieldValue = item.Value
                           });
                       }
                   }
               }).ReverseMap();
            ;

            #endregion
            //Initiated Model Mapping End  here

            #region ExecutionDataViewModel, ExecutionData
            //ExecutionData Model Mapping Start from  here
            CreateMap<ExecutionDataViewModel, ExecutionData>()
                //Only Property
                .ForMember(dest => dest.ApplicantTaskDetails, opt => opt.MapFrom(src => src.applicant_task_details))
                //Below both list type Model
                .ForMember(dest => dest.TaskDetails, opt => opt.MapFrom(src => src.task_details))
               //.ForMember(dest=>dest.OfficialFormDetail,opt=>opt.MapFrom(src=>src.official_form_details))
               .AfterMap((src, dest) =>
               {
                   if (src.official_form_details != null)
                   {
                       dest.OfficialFormDetail = new List<OfficialFormDetail>();
                       foreach (var item in src.official_form_details)
                       {
                           dest.OfficialFormDetail.Add(new OfficialFormDetail
                           {
                               OfficalFormID = item.Key,
                               OfficalFormValue = item.Value
                           });
                       }
                   }
               }).ReverseMap();
            ;

            #endregion

            #region TaskDetailViewModel, TaskDetail 
            CreateMap<TaskDetailViewModel, TaskDetail>()
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.amount))
                .ForMember(dest => dest.ApplId, opt => opt.MapFrom(src => Convert.ToInt32(src.appl_id)))
                .ForMember(dest => dest.Remarks, opt => opt.MapFrom(src => src.remarks))
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => Convert.ToInt32(src.task_id)))
                .ForMember(dest => dest.ActionNo, opt => opt.MapFrom(src => src.action_no))
                .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.task_name))
                .ForMember(dest => dest.TaskType, opt => opt.MapFrom(src => src.task_type))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.user_name))
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.service_id))         
                .ForMember(dest => dest.UserDetail, opt => opt.MapFrom(src => src.user_detail))              
                .ForMember(dest => dest.ActionTaken, opt => opt.MapFrom(src => src.action_taken))
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => ConvertStringToUtcDateTime(src.payment_date)))
                .ForMember(dest => dest.PaymentMode, opt => opt.MapFrom(src => src.payment_mode))
                .ForMember(dest => dest.PullUserId, opt => opt.MapFrom(src => src.pull_user_id))
                .ForMember(dest => dest.ExecutedTime, opt => opt.MapFrom(src => ConvertStringToUtcDateTime(src.executed_time)))
                .ForMember(dest => dest.ReceivedTime, opt => opt.MapFrom(src => ConvertStringToUtcDateTime(src.received_time)))
                .ForMember(dest => dest.PaymentRefNo, opt => opt.MapFrom(src => src.payment_ref_no))
                .ForMember(dest => dest.CurrentProcessId, opt => opt.MapFrom(src => src.current_process_id))
                .ForMember(dest => dest.CallbackCurrProcId, opt => opt.MapFrom(src => src.callback_curr_proc_id))
                .ReverseMap();
            #endregion

            #region UserDetailViewModel, UserDetail
            CreateMap<UserDetailViewModel, UserDetail>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.user_name)) 
                .ForMember(dest => dest.Designation, opt => opt.MapFrom(src => src.designation))
                .ForMember(dest => dest.LocationId, opt => opt.MapFrom(src => src.location_id))
                .ForMember(dest => dest.PullUserId, opt => opt.MapFrom(src => src.pull_user_id))
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.location_name))
                .ForMember(dest => dest.DepartmentLevel, opt => opt.MapFrom(src => src.department_level))
                .ForMember(dest => dest.LocationTypeId, opt => opt.MapFrom(src => src.location_type_id))
                .ForMember(dest => dest.CurrentProcessId, opt => opt.MapFrom(src => src.current_process_id))
              .ForMember(dest=>dest.TaskDetail,opt=>opt.Ignore())
                .ReverseMap();

            #endregion
            //end Map from  ServiceViewModel to ServiceModel
            


            //Map from ServiceModel to ServiceViewModelZombie  

            #region ServiceModel, ServiceViewModelZombie
            CreateMap<ServiceModel, ServiceViewModelZombie>()
            .ForMember(dest => dest.initiated_data, opt => opt.MapFrom(src => src.InitiatedData))
            .ForMember(dest => dest.execution_data, opt => opt.MapFrom(src => src.ExecutionData))
            
            .ReverseMap();
            #endregion

            #region InitiatedData,InitiatedDataViewModelZombie
            //Initiated Model Mapping Start from here

            CreateMap<InitiatedData, InitiatedDataViewModelZombie>()
               .ForMember(dest => dest.department_id, opt => opt.MapFrom(src => src.DepartmentId))
               .ForMember(dest => dest.department_name, opt => opt.MapFrom(src => src.DepartmentName))
               .ForMember(dest => dest.service_id, opt => opt.MapFrom(src => src.ServiceId))
               .ForMember(dest => dest.service_name, opt => opt.MapFrom(src => src.ServiceName))
               .ForMember(dest => dest.appl_id, opt => opt.MapFrom(src => src.ApplId))
               .ForMember(dest => dest.appl_ref_no, opt => opt.MapFrom(src => src.ApplRefNo))
               .ForMember(dest => dest.no_of_attachment, opt => opt.MapFrom(src => src.NoOfAttachment))
               .ForMember(dest => dest.submission_mode, opt => opt.MapFrom(src => src.SubmissionMode))
               .ForMember(dest => dest.submission_date, opt => opt.MapFrom(src => src.SubmissionDate))
               .ForMember(dest => dest.applied_by, opt => opt.MapFrom(src => src.AppliedBy))
               .ForMember(dest => dest.submission_location, opt => opt.MapFrom(src => src.SubmissionLocation))
               .ForMember(dest => dest.submission_location_id, opt => opt.MapFrom(src => src.SubmissionLocationId))
               .ForMember(dest => dest.submission_location_type_id, opt => opt.MapFrom(src => src.SubmissionLocationTypeId))
               .ForMember(dest => dest.payment_mode, opt => opt.MapFrom(src => src.PaymentMode))
               .ForMember(dest => dest.reference_no, opt => opt.MapFrom(src => src.ReferenceNo))
               .ForMember(dest => dest.payment_date, opt => opt.MapFrom(src => src.PaymentDate))
               .ForMember(dest => dest.amount, opt => opt.MapFrom(src => src.Amount))
               .ForMember(dest => dest.registration_id, opt => opt.MapFrom(src => src.RegistrationId))
               .ForMember(dest => dest.base_service_id, opt => opt.MapFrom(src => src.BaseServiceId))
               .ForMember(dest => dest.version_no, opt => opt.MapFrom(src => src.VersionNo))
               .ForMember(dest => dest.sub_version, opt => opt.MapFrom(src => src.SubVersion))
               .ForMember(dest=>dest.attribute_details,opt=>opt.MapFrom(src=>src.AttributeDetail))
               .ForMember(dest=>dest.enclosure_details,opt=>opt.MapFrom(src=>src.EnclosureDetails))

           .ReverseMap();

            #endregion

            #region AttributeDetails, AttributeDetailViewModelZombie 
            //ExecutionData Model Mapping Start from  here
                 CreateMap<AttributeDetail, AttributeDetailViewModelZombie>()
                //Only Property
                .ForMember(dest => dest.ApplicationFormFieldID, opt => opt.MapFrom(src => src.ApplicationFormFieldID))

                .ForMember(dest => dest.ApplicationFormFieldValue, opt => opt.MapFrom(src => src.ApplicationFormFieldValue))
                .ReverseMap();

            #endregion

            #region EnclosureDetail, EnclosureDetailViewModelZombie 
            //ExecutionData Model Mapping Start from  here
            CreateMap<EnclosureDetail, EnclosureDetailViewModelZombie>()
                //Only Property
                .ForMember(dest => dest.EnclousersId, opt => opt.MapFrom(src => src.EnclousersId))

                .ForMember(dest => dest.EnclousersValue, opt => opt.MapFrom(src => src.EnclousersValue))
                .ReverseMap()
                 ;

            #endregion

            #region ExecutionData, ExecutionDataViewModelZombie 
            //ExecutionData Model Mapping Start from  here
            CreateMap<ExecutionData,ExecutionDataViewModelZombie>()
                //Only Property
                .ForMember(dest => dest.applicant_task_details, opt => opt.MapFrom(src => src.ApplicantTaskDetails))
              //  //Below both list type Model
               .ForMember(dest => dest.task_details, opt => opt.MapFrom(src => src.TaskDetails))
              .ForMember(dest => dest.official_form_details, opt => opt.MapFrom(src => src.OfficialFormDetail))
              .ReverseMap()
            ;

            #endregion

            #region OfficialFormDetails, OfficialformDetailsViewModelZombie 
            //ExecutionData Model Mapping Start from  here
            CreateMap<OfficialFormDetail, OfficialFormDetailViewModelZombie>()
                //Only Property
                .ForMember(dest => dest.OfficalFormID, opt => opt.MapFrom(src => src.OfficalFormID))
               
                .ForMember(dest => dest.OfficalFormValue, opt => opt.MapFrom(src => src.OfficalFormValue))
              .ReverseMap();

            #endregion

            #region TaskDetail, TaskDetailViewModel
            CreateMap<TaskDetail, TaskDetailViewModelZombie>()
                .ForMember(dest => dest.amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.appl_id, opt => opt.MapFrom(src => src.ApplId))
                .ForMember(dest => dest.remarks, opt => opt.MapFrom(src => src.Remarks))
                .ForMember(dest => dest.task_id, opt => opt.MapFrom(src => src.TaskId))
                .ForMember(dest => dest.action_no, opt => opt.MapFrom(src => src.ActionNo))
                .ForMember(dest => dest.task_name, opt => opt.MapFrom(src => src.TaskName))
                .ForMember(dest => dest.task_type, opt => opt.MapFrom(src => src.TaskType))
                .ForMember(dest => dest.user_name, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.service_id, opt => opt.MapFrom(src => src.ServiceId))
                .ForMember(dest => dest.user_detail, opt => opt.MapFrom(src => src.UserDetail))
                .ForMember(dest => dest.action_taken, opt => opt.MapFrom(src => src.ActionTaken))
                .ForMember(dest => dest.payment_date, opt => opt.MapFrom(src => src.PaymentDate))
                .ForMember(dest => dest.payment_mode, opt => opt.MapFrom(src => src.PaymentMode))
                .ForMember(dest => dest.pull_user_id, opt => opt.MapFrom(src => src.PullUserId))
                .ForMember(dest => dest.executed_time, opt => opt.MapFrom(src => src.ExecutedTime))
                .ForMember(dest => dest.received_time, opt => opt.MapFrom(src => src.ReceivedTime))
                .ForMember(dest => dest.payment_ref_no, opt => opt.MapFrom(src => src.PaymentRefNo))
                .ForMember(dest => dest.current_process_id, opt => opt.MapFrom(src => src.CurrentProcessId))
                .ForMember(dest => dest.callback_curr_proc_id, opt => opt.MapFrom(src => src.CallbackCurrProcId))
                .ReverseMap();
            #endregion

            #region UserDetail,UserDetailViewModelZombie
            CreateMap<UserDetail,UserDetailViewModelZombie>()
                .ForMember(dest => dest.user_name, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.designation, opt => opt.MapFrom(src => src.Designation))
                .ForMember(dest => dest.location_id, opt => opt.MapFrom(src => src.LocationId))
                .ForMember(dest => dest.pull_user_id, opt => opt.MapFrom(src => src.PullUserId))
                .ForMember(dest => dest.location_name, opt => opt.MapFrom(src => src.LocationName))
                .ForMember(dest => dest.department_level, opt => opt.MapFrom(src => src.DepartmentLevel))
                .ForMember(dest => dest.location_type_id, opt => opt.MapFrom(src => src.LocationTypeId))
                .ForMember(dest => dest.current_process_id, opt => opt.MapFrom(src => src.CurrentProcessId))
               
                .ReverseMap();

            #endregion

            //end Map from ServiceModel to ServiceViewModel  

            #region CustomLGDDistrictViewModel, CustomLGDTehsilSubTehsilViewModel
            CreateMap<CustomLGDDistrictViewModel, CustomLGDDistrict>();

            CreateMap<CustomLGDTehsilSubTehsilViewModel, CustomLGDTehsilSubTehsil>();
            #endregion

            #region PendencyReportViewModel, PendencyReport
            CreateMap<PendencyReportViewModel, PendencyReport>();
            #endregion   

            #region JSONReceivedViewModel,JSONReceived
            CreateMap<JSONReceivedViewModel,JSONReceived>().ReverseMap();
            #endregion

                      
        }

        //  method for converting string to UTC DateTime
        private DateTime? ConvertStringToUtcDateTime(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
            {
                return null;
            }

            DateTime dateTime = DateTime.ParseExact(dateString, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            var dateTime1= ConvertToUtc(dateTime);

            return dateTime1;
        }




        private DateTime? ConvertToUtc(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);
            }
            return null;
        }


    }
}

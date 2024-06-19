using AutoMapper;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Entities;
using HRM_BACKEND_VSA.Entities.Applicant;
using HRM_BACKEND_VSA.Entities.Staff;
using static HRM_BACKEND_VSA.Contracts.StaffContracts;
using static HRM_BACKEND_VSA.Contracts.UserContracts;
using static HRM_BACKEND_VSA.Domains.Applicant.Applicant_BioData.AddBioData;
using static HRM_BACKEND_VSA.Domains.HR_Management.App_Setup.Grade.AddGrade;
using static HRM_BACKEND_VSA.Features.SMS_Setup.CreateSMSTemplate;
using static HRM_BACKEND_VSA.Features.SMS_Setup.EditSMSTemplate;

namespace HRM_BACKEND_VSA.Extensions
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {

            CreateMap<EditSMSTemplateRequest, NewSMSTemplateRequest>().ReverseMap();
            CreateMap<ApplicantLoginResponse, Applicant>().ReverseMap();
            CreateMap<ApplicantBioData, AddBioDataRequest>().ReverseMap();
            CreateMap<AddGradeRequest, Grade>().ReverseMap();
            CreateMap<Staff, ApplicantBioData>().ReverseMap();
            CreateMap<Staff, StaffLoginResponse>().ReverseMap();
            CreateMap<User, UserLoginResponse>().ReverseMap();
            CreateMap<StaffAppointment, StaffAppointmentHistory>().ReverseMap();
            CreateMap<StaffAccomodationDetail, StaffAccomodationUpdateHistory>().ReverseMap();
            CreateMap<Staff, StaffBioUpdateHistory>().ReverseMap();
        }
    }
}

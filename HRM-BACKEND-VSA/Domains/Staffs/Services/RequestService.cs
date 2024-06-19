using AutoMapper;
using HRM_BACKEND_VSA.Contracts;
using HRM_BACKEND_VSA.Database;
using HRM_BACKEND_VSA.Domains.HR_Management.StaffRequestHandlers;
using HRM_BACKEND_VSA.Domains.Staffs.StaffRequestHandlers;
using HRM_BACKEND_VSA.Providers;


namespace HRM_BACKEND_VSA.Domains.Staffs.Services
{
    public class RequestService
    {
        private readonly HRMDBContext _dbContext;
        private readonly HRMStaffDBContext _staffDbContext;
        private readonly Dictionary<string, IStaffRequest> _mapper;
        private readonly IMapper _mapperFactory;
        private readonly Authprovider _authProvider;

        public RequestService(HRMDBContext dbContext, HRMStaffDBContext staffDbContext, Authprovider authProvider, IMapper mapperFactory)
        {
            _dbContext = dbContext;
            _staffDbContext = staffDbContext;
            _authProvider = authProvider;
            _mapperFactory = mapperFactory;

            _mapper = new Dictionary<string, IStaffRequest>
            {
                { RegisterationRequestTypes.newRegisteration, new NewStaffRegisterationHandler(_dbContext,staffDbContext,_authProvider) },
                { RegisterationRequestTypes.bioData, new StaffBioDataRequest(_dbContext, _authProvider,_mapperFactory) },
                { RegisterationRequestTypes.childrenDetails, new ChildrenDetailRequest(_dbContext,_authProvider) },
                { RegisterationRequestTypes.familyDetails, new FamilyDetailRequest(_dbContext,_authProvider) },
                { RegisterationRequestTypes.accomodation, new AccomodationRequest(_dbContext,_mapperFactory,_authProvider )},
                { RegisterationRequestTypes.bankUpdate, new BankRequest(_dbContext,_authProvider) },
                { RegisterationRequestTypes.professionalLicense, new ProfessionalLincenseRequest(_dbContext, _authProvider) }
             };
        }


        public IStaffRequest getRequestService(string serviceName)
        {
            if (serviceName is null) return null;
            IStaffRequest service;
            _mapper.TryGetValue(serviceName, out service);

            if (service is null) return null;

            return service;

        }


    }
}



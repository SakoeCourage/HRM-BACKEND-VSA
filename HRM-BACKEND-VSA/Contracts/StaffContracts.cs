namespace HRM_BACKEND_VSA.Contracts
{
    public class StaffContracts
    {

        public class StaffUnitResponsePartial
        {
            public Guid Id { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime updatedAt { get; set; } = DateTime.UtcNow;
            public string unitName { get; set; } = String.Empty;
        }

        public class StaffLoginResponse
        {
            public Guid Id { get; set; }
            public string staffIdentificationNumber { get; set; }
            public string firstName { get; set; } = string.Empty;
            public string lastName { get; set; } = string.Empty;
            public string? otherNames { get; set; } = string.Empty;
            public string gender { get; set; } = string.Empty;
            public string email { get; set; } = string.Empty;
            public string? passportPicture { get; set; } = string.Empty;
            public SetupContract.DirectorateListResponseDto directorate { get; set; } = new();
            public SetupContract.DepartmentListResponseDto department { get; set; } = new();
            public StaffUnitResponsePartial unit { get; set; } = new();
            public string accessToken { get; set; }
        }
        
        public static class StaffRequestStatusTypes
        {
            public static readonly string pending = "PENDING";
            public static readonly string appointed = "APPOINTED";
            public static readonly string approved = "APPROVED";
            public static readonly string rejected = "REJECTED";
            public static readonly string posted = "POSTED";
        }

        public static class StaffStatusTypes
        {
            public static readonly string active = "ACTIVE";
            public static readonly string inActive = "INACTIVE";


        }
        
           public class StaffSpecialityResponseDto
        {
            public Guid Id { get; set; }
            public Guid categoryId { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime updatedAt { get; set; } = DateTime.UtcNow;
            public string specialityName { get; set; } = String.Empty;
        }

        public class UnitResponseDto
        {
            public Guid Id { get; set; }
            public Guid departmentId { get; set; }
            public Guid? unitHeadId { get; set; }
            public Guid? directorateId { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime updatedAt { get; set; } = DateTime.UtcNow;
            public string unitName { get; set; }
        }

        public class StaffBankResponseDto
        {
            public Guid Id { get; set; }
            public Guid staffId { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
            public Guid bankId { get; set; }
            public string accountType { get; set; }
            public string branch { get; set; }
            public string accountNumber { get; set; }
            public Boolean? isApproved { get; set; } = false;
            public Boolean? isAlterable { get; set; } = false;
        }

        public class StaffFamilyResponseDto
        {
            public Guid Id { get; set; }
            public Guid staffId { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
            public string fathersName { get; set; } = String.Empty;
            public string mothersName { get; set; } = String.Empty;
            public string spouseName { get; set; } = String.Empty;
            public string spousePhoneNumber { get; set; } = String.Empty;
            public string nextOfKIN { get; set; } = String.Empty;
            public string nextOfKINPhoneNumber { get; set; } = String.Empty;
            public string emergencyPerson { get; set; } = String.Empty;
            public string emergencyPersonPhoneNumber { get; set; } = String.Empty;
            public Boolean? isApproved { get; set; } = false;
            public Boolean? isAlterable { get; set; } = false;
        }

        public class StaffProfessionalLicenseDto
        {
            public Guid Id { get; set; }
            public Guid professionalBodyId { get; set; }
            public Guid staffId { get; set; }
            public string pin { get; set; }
            public DateOnly issuedDate { get; set; }
            public DateOnly expiryDate { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
            public Boolean? isApproved { get; set; } = false;
            public Boolean? isAlterable { get; set; } = false;
        }

        public class staffChildrenResponseDto
        {
            public Guid Id { get; set; }
            public Guid staffId { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
            public string childName { get; set; } = string.Empty;
            public DateOnly dateOfBirth { get; set; }
            public string gender { get; set; } = string.Empty;
            public Boolean? isApproved { get; set; } = false;
            public Boolean? isAlterable { get; set; } = false;
        }

        public class staffAccomodationResponseDto
        {
            public Guid Id { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
            public Guid staffId { get; set; }
            public string source { get; set; } = String.Empty;
            public string gpsAddress { get; set; } = String.Empty;
            public string accomodationType { get; set; } = String.Empty;
            public DateOnly? allocationDate { get; set; }
            public string? flatNumber { get; set; } = String.Empty;
            public Boolean? isApproved { get; set; } = false;
            public Boolean? isAlterable { get; set; } = false;
        }


        public class StaffAppointmentResponseDto
        {
            public Guid Id { get; set; }
            public Guid gradeId { get; set; }
            public Guid staffId { get; set; }
            public Guid? staffSpecialityId { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
            public string appointmentType { get; set; } = String.Empty;
            public string staffType { get; set; } = String.Empty;
            public DateOnly? endDate { get; set; }
            public string paymentSource { get; set; } = String.Empty;
            public DateOnly notionalDate { get; set; }
            public DateOnly substantiveDate { get; set; }
            public Boolean? isApproved { get; set; } = false;
            public Boolean? isAlterable { get; set; } = false;
        }

        public class staffPostingResponseDto
        {
            public Guid Id { get; set; }
            public Guid staffId { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
            public Guid? directorateId { get; set; }
            public Guid? departmentId { get; set; }
            public Guid? unitId { get; set; }
            public DateOnly postingDate { get; set; }
        }
        
        public class StaffProfileResponse
        {
            public Guid Id { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? lastSeen { get; set; }
            public string title { get; set; } = string.Empty;
            public string GPSAddress { get; set; } = string.Empty;
            public string nationality { get; set; } = string.Empty;
            public string staffIdentificationNumber { get; set; }
            public string firstName { get; set; } = string.Empty;
            public string lastName { get; set; } = string.Empty;
            public string? otherNames { get; set; } = string.Empty;
            public Guid? specialityId { get; set; }
            public DateOnly? dateOfBirth { get; set; }
            public string phone { get; set; } = string.Empty;
            public string gender { get; set; } = string.Empty;
            public string SNNITNumber { get; set; } = string.Empty;
            public string email { get; set; } = string.Empty;
            public string disability { get; set; } = string.Empty;
            public string? passportPicture { get; set; } = string.Empty;
            public string ECOWASCardNumber { get; set; } = string.Empty;
            public string status { get; set; } = StaffStatusTypes.active;
            public staffPostingResponseDto? staffPosting { get; set; }
            public UnitResponseDto unit { get; set; }
            public StaffSpecialityResponseDto speciality { get; set; }
            public StaffBankResponseDto bankDetail { get; set; }
            public StaffFamilyResponseDto familyDetail { get; set; }
            public StaffProfessionalLicenseDto professionalLincense { get; set; }
            public ICollection<staffChildrenResponseDto> staffChildren { get; set; }
            public staffAccomodationResponseDto staffAccomodation { get; set; }
            public StaffAppointmentResponseDto currentAppointment { get; set; }
        }
    }
    }




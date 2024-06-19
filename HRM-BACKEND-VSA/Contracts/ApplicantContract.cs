using System.Runtime.Serialization;

namespace HRM_BACKEND_VSA.Contracts
{
    public enum TitleEnum
    {
        [EnumMember(Value = "Mr")]
        Mr,
        [EnumMember(Value = "Mrs")]
        Mrs,
        [EnumMember(Value = "Dr")]
        Dr,
        [EnumMember(Value = "Miss")]
        Miss,
        [EnumMember(Value = "Prof")]
        Prof,
        [EnumMember(Value = "Sir")]
        Sir

    }

    public enum GenderEnum
    {
        [EnumMember(Value = "Male")]
        Male,
        [EnumMember(Value = "Female")]
        Female
    }
    public class EducationalBackgroudRequestData
    {
        public DateOnly yearCompleted { get; set; }
        public string institutionName { get; set; } = String.Empty;
        public string certificate { get; set; } = String.Empty;

    }

    public class ApplicantLoginResponse
    {
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string firsName { get; set; } = String.Empty;
        public string lastName { get; set; } = String.Empty;
        public string? email { get; set; }
        public string contact { get; set; } = String.Empty;
        public bool? hasSubmittedApplication { get; set; } = false;
        public string? applicationStatus { get; set; }
        public string accessToken { get; set; }
    }

}



namespace HRM_BACKEND_VSA.Contracts;

public class SetupContract
{
    public class DepartmentListResponseDto
    {
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string departmentName { get; set; } = String.Empty;
    }
    
    public class DirectorateListResponseDto
    {
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string directorateName { get; set; } = String.Empty;
    }   
    
    public class UnitListResponseDTO
    {
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string unitName { get; set; } = String.Empty;
        public  DepartmentListResponseDto? department { get; set; } = new DepartmentListResponseDto();
        public DirectorateListResponseDto? directorate { get; set; } = new DirectorateListResponseDto();
    }
    
    public class CategoryListResponseDto
    {
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string categoryName { get; set; } = String.Empty;
    }
    
    public class GradeListResponseDto
    {
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string gradeName { get; set; } = string.Empty;
        public string level { get; set; } = string.Empty;
        public string scale { get; set; } = string.Empty;
        public Double marketPremium { get; set; }
        public int minimumStep { get; set; } 
        public int maximumStep { get; set; }
    }
    
    public class BankListResponseDto
    {
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string bankName { get; set; } = string.Empty;
      
    }
    
    public class SpecialityListResponseDto
    {
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string specialityName { get; set; } = string.Empty;
        public CategoryListResponseDto category { get; set; } = new CategoryListResponseDto();
    }

    public class GradeResponseDto
    {
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public Guid categoryId { get; set; } = Guid.Empty;
        public string gradeName { get; set; } = string.Empty;
        public string level { get; set; } = string.Empty;
        public string scale { get; set; } = string.Empty;
        public Double marketPremium { get; set; } 
        public int minimumStep { get; set; }
        public int maximumStep { get; set; }
        public ICollection<GradeStepResponseDto> steps { get; set; } = new List<GradeStepResponseDto>();
    }

    public class GradeStepResponseDto
    {
        public Guid Id { get; set; }
        public Guid gradeId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public int stepIndex { get; set; }
        public Double salary { get; set; }
        public Double marketPreBaseSalary { get; set; }
        
    }

    public class CategoryResponseDto
    {
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string categoryName { get; set; } = string.Empty;
        public string gradeName { get; set; } = string.Empty;
        public string level { get; set; } = string.Empty;
        public string scale { get; set; } = string.Empty;
        public ICollection<GradeStepResponseDto> steps { get; set; } 
    }
    
}
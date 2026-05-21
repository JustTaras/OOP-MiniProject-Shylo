namespace VetClinic.Infrastructure.Persistence.Dtos;

/// <summary>
/// Data Transfer Object for JSON serialization of Owner
/// </summary>
public class OwnerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Data Transfer Object for JSON serialization of Pet
/// </summary>
public class PetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int OwnerIdRef { get; set; }
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public int AgeInMonths { get; set; }
}

/// <summary>
/// Data Transfer Object for JSON serialization of Veterinarian
/// </summary>
public class VeterinarianDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
}

/// <summary>
/// Data Transfer Object for JSON serialization of Appointment
/// </summary>
public class AppointmentDto
{
    public int Id { get; set; }
    public int PetIdRef { get; set; }
    public int VeterinarianIdRef { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data Transfer Object for JSON serialization of MedicalRecord
/// </summary>
public class MedicalRecordDto
{
    public int Id { get; set; }
    public int AppointmentIdRef { get; set; }
    public int PetIdRef { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public DateTime VisitDate { get; set; }
    public DateTime RecordedAt { get; set; }
}

/// <summary>
/// Container for all clinic data in persistent storage
/// </summary>
public class ClinicDataSnapshot
{
    public List<OwnerDto> Owners { get; set; } = [];
    public List<PetDto> Pets { get; set; } = [];
    public List<VeterinarianDto> Veterinarians { get; set; } = [];
    public List<AppointmentDto> Appointments { get; set; } = [];
    public List<MedicalRecordDto> MedicalRecords { get; set; } = [];
}

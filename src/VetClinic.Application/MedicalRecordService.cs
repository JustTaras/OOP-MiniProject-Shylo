namespace VetClinic.Application;

using VetClinic.Domain;

/// <summary>
/// Application service for managing medical records (Lab 35)
/// Implements business logic for medical record creation and retrieval
/// </summary>
public class MedicalRecordService
{
    private readonly IAppointmentRepository _repository;

    public MedicalRecordService(IAppointmentRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Business Rule 1: Cannot create medical record for non-completed appointment
    /// Business Rule 2: Diagnosis must be provided
    /// Business Rule 3: Treatment must be detailed
    /// </summary>
    public Result<MedicalRecord> CreateMedicalRecord(
        Appointment appointment,
        Pet pet,
        string diagnosis,
        string treatment)
    {
        try
        {
            // BR1: Verify appointment is completed
            if (appointment.Status != AppointmentStatus.Completed)
            {
                return Result<MedicalRecord>.Fail(
                    $"Cannot create medical record for appointment in {appointment.Status} status. " +
                    "Only completed appointments can have medical records.");
            }

            // BR2/3: Create record (constructor will validate diagnosis and treatment)
            int recordId = _repository.GetNextMedicalRecordId();
            var record = new MedicalRecord(
                recordId,
                appointment.Id,
                pet,
                diagnosis,
                treatment,
                DateTime.Now);  // Medical record visit date is when it's created, not when appointment was scheduled

            _repository.AddMedicalRecord(record);

            return Result<MedicalRecord>.Ok(record,
                $"Medical record created for {pet.Name} on {appointment.AppointmentDateTime:yyyy-MM-dd}");
        }
        catch (ArgumentNullException ex)
        {
            return Result<MedicalRecord>.Fail($"Missing required data: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            return Result<MedicalRecord>.Fail($"Invalid medical record data: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<MedicalRecord>.Fail($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get medical history for a pet (ordered by visit date, newest first)
    /// Business Rule 5: Only show completed appointments with medical records
    /// </summary>
    public IReadOnlyList<MedicalRecord> GetPetMedicalHistory(Pet pet)
    {
        if (pet == null)
            throw new ArgumentNullException(nameof(pet));

        return _repository.GetMedicalRecordsByPet(pet);
    }

    /// <summary>
    /// Get all medical records
    /// </summary>
    public IReadOnlyList<MedicalRecord> GetAllMedicalRecords()
    {
        return _repository.GetAllMedicalRecords();
    }

    /// <summary>
    /// Get medical records for a date range
    /// </summary>
    public IReadOnlyList<MedicalRecord> GetRecordsByDateRange(DateTime startDate, DateTime endDate)
    {
        return _repository.GetAllMedicalRecords()
            .Where(r => r.VisitDate >= startDate && r.VisitDate <= endDate)
            .OrderByDescending(r => r.VisitDate)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Get all diagnoses for a specific pet
    /// Used for medical history analysis
    /// </summary>
    public IReadOnlyList<string> GetDiagnosesForPet(Pet pet)
    {
        var history = GetPetMedicalHistory(pet);
        return history
            .Select(r => r.Diagnosis)
            .Distinct()
            .OrderBy(d => d)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Check if pet has medical history
    /// </summary>
    public bool HasMedicalHistory(Pet pet)
    {
        return GetPetMedicalHistory(pet).Count > 0;
    }
}

namespace VetClinic.Domain;

/// <summary>
/// Repository interface for managing appointments
/// Follows Repository pattern for data access abstraction
/// </summary>
public interface IAppointmentRepository
{
    /// <summary>
    /// Add a new appointment to the repository
    /// </summary>
    void Add(Appointment appointment);

    /// <summary>
    /// Get appointment by ID
    /// </summary>
    Appointment? GetById(int id);

    /// <summary>
    /// Get all appointments
    /// </summary>
    IReadOnlyList<Appointment> GetAll();

    /// <summary>
    /// Get all appointments for a specific pet
    /// </summary>
    IReadOnlyList<Appointment> GetByPet(Pet pet);

    /// <summary>
    /// Get all appointments with a specific veterinarian
    /// </summary>
    IReadOnlyList<Appointment> GetByVeterinarian(Veterinarian veterinarian);

    /// <summary>
    /// Get appointments within a specific date range
    /// </summary>
    IReadOnlyList<Appointment> GetByDateRange(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Update an existing appointment
    /// </summary>
    void Update(Appointment appointment);

    /// <summary>
    /// Remove an appointment from the repository
    /// </summary>
    bool Remove(int appointmentId);

    /// <summary>
    /// Check if veterinarian is available at specific date/time
    /// </summary>
    bool IsVeterinarianAvailable(Veterinarian veterinarian, DateTime appointmentDateTime);

    /// <summary>
    /// Add a medical record to the repository (Lab 35+)
    /// </summary>
    void AddMedicalRecord(MedicalRecord record);

    /// <summary>
    /// Get medical record by ID (Lab 35+)
    /// </summary>
    MedicalRecord? GetMedicalRecordById(int id);

    /// <summary>
    /// Get all medical records for a specific pet (Lab 35+)
    /// </summary>
    IReadOnlyList<MedicalRecord> GetMedicalRecordsByPet(Pet pet);

    /// <summary>
    /// Get all medical records (Lab 35+)
    /// </summary>
    IReadOnlyList<MedicalRecord> GetAllMedicalRecords();

    /// <summary>
    /// Get next appointment ID for new appointments (Lab 35+)
    /// </summary>
    int GetNextAppointmentId();

    /// <summary>
    /// Get next medical record ID for new records (Lab 35+)
    /// </summary>
    int GetNextMedicalRecordId();
}

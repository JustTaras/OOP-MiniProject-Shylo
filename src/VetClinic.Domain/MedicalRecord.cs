namespace VetClinic.Domain;

/// <summary>
/// Represents a medical record for a pet visit
/// </summary>
public class MedicalRecord
{
    public int Id { get; }
    public int AppointmentId { get; }
    public Pet Pet { get; }
    public string Diagnosis { get; }
    public string Treatment { get; }
    public DateTime VisitDate { get; }
    public DateTime RecordedAt { get; }

    /// <summary>
    /// Creates a new MedicalRecord with validation
    /// Lab 35: Simplified constructor for persistence compatibility
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when data is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown when pet is null</exception>
    public MedicalRecord(int id, int appointmentId, Pet pet, string diagnosis, 
                        string treatment, DateTime visitDate)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be positive", nameof(id));

        if (appointmentId <= 0)
            throw new ArgumentException("Appointment ID must be positive", nameof(appointmentId));
        
        if (pet == null)
            throw new ArgumentNullException(nameof(pet), "Medical record must have a pet");
        
        if (string.IsNullOrWhiteSpace(diagnosis))
            throw new ArgumentException("Diagnosis cannot be empty", nameof(diagnosis));
        
        if (diagnosis.Length > 500)
            throw new ArgumentException("Diagnosis is too long (max 500 characters)", nameof(diagnosis));
        
        if (string.IsNullOrWhiteSpace(treatment))
            throw new ArgumentException("Treatment cannot be empty", nameof(treatment));
        
        if (treatment.Length > 1000)
            throw new ArgumentException("Treatment description is too long (max 1000 characters)", nameof(treatment));

        if (visitDate > DateTime.Now)
            throw new ArgumentException("Visit date cannot be in the future", nameof(visitDate));

        Id = id;
        AppointmentId = appointmentId;
        Pet = pet;
        Diagnosis = diagnosis.Trim();
        Treatment = treatment.Trim();
        VisitDate = visitDate;
        RecordedAt = DateTime.Now;
    }

    public override string ToString()
    {
        return $"Medical Record ID: {Id}, Pet: {Pet.Name}, Date: {VisitDate:yyyy-MM-dd}, " +
               $"Diagnosis: {Diagnosis}";
    }
}

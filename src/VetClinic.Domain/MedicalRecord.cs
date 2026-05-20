namespace VetClinic.Domain;

/// <summary>
/// Represents a medical record for a pet visit
/// </summary>
public class MedicalRecord
{
    public int Id { get; }
    public Pet Pet { get; }
    public Appointment Appointment { get; }
    public string Diagnosis { get; }
    public string Treatment { get; }
    public string Notes { get; }
    public DateTime RecordDate { get; }

    /// <summary>
    /// Creates a new MedicalRecord with validation
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when data is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown when pet or appointment is null</exception>
    public MedicalRecord(int id, Pet pet, Appointment appointment, string diagnosis, 
                        string treatment, string notes)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be positive", nameof(id));
        
        if (pet == null)
            throw new ArgumentNullException(nameof(pet), "Medical record must have a pet");
        
        if (appointment == null)
            throw new ArgumentNullException(nameof(appointment), "Medical record must be linked to an appointment");
        
        if (appointment.Status != AppointmentStatus.Completed)
            throw new ArgumentException("Medical record can only be created for completed appointments", 
                                      nameof(appointment));
        
        if (string.IsNullOrWhiteSpace(diagnosis))
            throw new ArgumentException("Diagnosis cannot be empty", nameof(diagnosis));
        
        if (diagnosis.Length > 500)
            throw new ArgumentException("Diagnosis is too long (max 500 characters)", nameof(diagnosis));
        
        if (string.IsNullOrWhiteSpace(treatment))
            throw new ArgumentException("Treatment cannot be empty", nameof(treatment));
        
        if (treatment.Length > 1000)
            throw new ArgumentException("Treatment description is too long (max 1000 characters)", nameof(treatment));
        
        // Notes are optional, but if provided, validate length
        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > 1000)
            throw new ArgumentException("Notes are too long (max 1000 characters)", nameof(notes));

        Id = id;
        Pet = pet;
        Appointment = appointment;
        Diagnosis = diagnosis.Trim();
        Treatment = treatment.Trim();
        Notes = notes?.Trim() ?? string.Empty;
        RecordDate = DateTime.Now;
    }

    public override string ToString()
    {
        return $"Medical Record ID: {Id}, Pet: {Pet.Name}, Date: {RecordDate:yyyy-MM-dd}, " +
               $"Diagnosis: {Diagnosis}";
    }
}

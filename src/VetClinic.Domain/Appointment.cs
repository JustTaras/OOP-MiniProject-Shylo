namespace VetClinic.Domain;

/// <summary>
/// Represents an appointment at the veterinary clinic
/// </summary>
public class Appointment
{
    public int Id { get; }
    public Pet Pet { get; }
    public Veterinarian Veterinarian { get; }
    public DateTime AppointmentDateTime { get; }
    public string Reason { get; }
    public AppointmentStatus Status { get; private set; }
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Creates a new Appointment with validation
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when data is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown when pet or vet is null</exception>
    public Appointment(int id, Pet pet, Veterinarian veterinarian, DateTime appointmentDateTime, string reason)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be positive", nameof(id));
        
        if (pet == null)
            throw new ArgumentNullException(nameof(pet), "Appointment must have a pet");
        
        if (veterinarian == null)
            throw new ArgumentNullException(nameof(veterinarian), "Appointment must have a veterinarian");
        
        if (appointmentDateTime <= DateTime.Now)
            throw new ArgumentException("Appointment time must be in the future", nameof(appointmentDateTime));
        
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason for appointment cannot be empty", nameof(reason));
        
        if (reason.Length > 500)
            throw new ArgumentException("Reason is too long (max 500 characters)", nameof(reason));

        Id = id;
        Pet = pet;
        Veterinarian = veterinarian;
        AppointmentDateTime = appointmentDateTime;
        Reason = reason.Trim();
        Status = AppointmentStatus.Scheduled;
        CreatedAt = DateTime.Now;
    }

    /// <summary>
    /// Mark appointment as completed
    /// </summary>
    public void Complete()
    {
        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot complete a cancelled appointment");
        
        Status = AppointmentStatus.Completed;
    }

    /// <summary>
    /// Cancel the appointment
    /// </summary>
    public void Cancel()
    {
        if (Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed appointment");
        
        Status = AppointmentStatus.Cancelled;
    }

    public override string ToString()
    {
        return $"Appointment ID: {Id}, Pet: {Pet.Name}, Vet: {Veterinarian.Name}, " +
               $"Time: {AppointmentDateTime:yyyy-MM-dd HH:mm}, Status: {Status}";
    }
}

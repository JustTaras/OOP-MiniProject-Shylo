namespace VetClinic.Application;

using VetClinic.Domain;

/// <summary>
/// Result pattern for handling operation outcomes
/// </summary>
public sealed record Result(bool Success, string Message)
{
    public static Result Ok(string message = "Operation successful") => new(true, message);
    public static Result Fail(string message) => new(false, message);
}

/// <summary>
/// Generic Result pattern
/// </summary>
public sealed record Result<T>(bool Success, T? Data, string Message)
{
    public static Result<T> Ok(T data, string message = "Operation successful") => 
        new(true, data, message);
    
    public static Result<T> Fail(string message) => 
        new(false, default, message);
}

/// <summary>
/// Application service for managing appointments
/// Implements business logic and orchestrates domain operations
/// </summary>
public class AppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository 
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
    }

    /// <summary>
    /// Schedule a new appointment with validation
    /// </summary>
    public Result<Appointment> ScheduleAppointment(Pet pet, Veterinarian veterinarian, 
                                                   DateTime appointmentDateTime, string reason)
    {
        try
        {
            // Validate veterinarian availability
            if (!_appointmentRepository.IsVeterinarianAvailable(veterinarian, appointmentDateTime))
            {
                return Result<Appointment>.Fail(
                    $"Veterinarian {veterinarian.Name} is not available at {appointmentDateTime:yyyy-MM-dd HH:mm}");
            }

            // Create appointment (domain layer will validate)
            int newId = GenerateAppointmentId();
            var appointment = new Appointment(newId, pet, veterinarian, appointmentDateTime, reason);

            // Persist
            _appointmentRepository.Add(appointment);

            return Result<Appointment>.Ok(appointment, 
                $"Appointment scheduled for {pet.Name} with {veterinarian.Name} on {appointmentDateTime:yyyy-MM-dd HH:mm}");
        }
        catch (ArgumentNullException ex)
        {
            return Result<Appointment>.Fail($"Missing required data: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            return Result<Appointment>.Fail($"Invalid appointment data: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<Appointment>.Fail($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all appointments
    /// </summary>
    public Result<IReadOnlyList<Appointment>> GetAllAppointments()
    {
        try
        {
            var appointments = _appointmentRepository.GetAll();
            return Result<IReadOnlyList<Appointment>>.Ok(appointments, 
                $"Retrieved {appointments.Count} appointments");
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Appointment>>.Fail($"Error retrieving appointments: {ex.Message}");
        }
    }

    /// <summary>
    /// Get appointments for a specific pet
    /// </summary>
    public Result<IReadOnlyList<Appointment>> GetAppointmentsByPet(Pet pet)
    {
        try
        {
            if (pet == null)
                return Result<IReadOnlyList<Appointment>>.Fail("Pet cannot be null");

            var appointments = _appointmentRepository.GetByPet(pet);
            return Result<IReadOnlyList<Appointment>>.Ok(appointments, 
                $"Retrieved {appointments.Count} appointments for {pet.Name}");
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Appointment>>.Fail($"Error retrieving pet appointments: {ex.Message}");
        }
    }

    /// <summary>
    /// Get appointments for a specific veterinarian
    /// </summary>
    public Result<IReadOnlyList<Appointment>> GetAppointmentsByVeterinarian(Veterinarian veterinarian)
    {
        try
        {
            if (veterinarian == null)
                return Result<IReadOnlyList<Appointment>>.Fail("Veterinarian cannot be null");

            var appointments = _appointmentRepository.GetByVeterinarian(veterinarian);
            return Result<IReadOnlyList<Appointment>>.Ok(appointments, 
                $"Retrieved {appointments.Count} appointments for Dr. {veterinarian.Name}");
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Appointment>>.Fail($"Error retrieving veterinarian appointments: {ex.Message}");
        }
    }

    /// <summary>
    /// Complete an appointment
    /// </summary>
    public Result CompleteAppointment(Appointment appointment)
    {
        try
        {
            if (appointment == null)
                return Result.Fail("Appointment cannot be null");

            if (appointment.Status == AppointmentStatus.Completed)
                return Result.Fail("Appointment is already completed");

            if (appointment.Status == AppointmentStatus.Cancelled)
                return Result.Fail("Cannot complete a cancelled appointment");

            appointment.Complete();
            _appointmentRepository.Update(appointment);

            return Result.Ok($"Appointment {appointment.Id} marked as completed");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error completing appointment: {ex.Message}");
        }
    }

    /// <summary>
    /// Cancel an appointment
    /// </summary>
    public Result CancelAppointment(Appointment appointment)
    {
        try
        {
            if (appointment == null)
                return Result.Fail("Appointment cannot be null");

            if (appointment.Status == AppointmentStatus.Completed)
                return Result.Fail("Cannot cancel a completed appointment");

            if (appointment.Status == AppointmentStatus.Cancelled)
                return Result.Fail("Appointment is already cancelled");

            appointment.Cancel();
            _appointmentRepository.Update(appointment);

            return Result.Ok($"Appointment {appointment.Id} cancelled");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error cancelling appointment: {ex.Message}");
        }
    }

    /// <summary>
    /// Simple ID generator (in-memory, will be replaced with database in future iterations)
    /// </summary>
    private int GenerateAppointmentId()
    {
        var all = _appointmentRepository.GetAll();
        return all.Count == 0 ? 1 : all.Max(a => a.Id) + 1;
    }
}

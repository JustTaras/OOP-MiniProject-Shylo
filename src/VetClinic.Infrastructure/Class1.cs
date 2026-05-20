namespace VetClinic.Infrastructure;

using VetClinic.Domain;
using VetClinic.Application;

/// <summary>
/// In-memory implementation of IAppointmentRepository
/// For Iteration 1 testing only; will be replaced with database in future iterations
/// </summary>
public class InMemoryAppointmentRepository : IAppointmentRepository
{
    private readonly List<Appointment> _appointments = new();

    public void Add(Appointment appointment)
    {
        if (appointment == null)
            throw new ArgumentNullException(nameof(appointment));
        
        if (_appointments.Any(a => a.Id == appointment.Id))
            throw new InvalidOperationException($"Appointment with ID {appointment.Id} already exists");

        _appointments.Add(appointment);
    }

    public Appointment? GetById(int id)
    {
        return _appointments.FirstOrDefault(a => a.Id == id);
    }

    public IReadOnlyList<Appointment> GetAll()
    {
        return _appointments.AsReadOnly();
    }

    public IReadOnlyList<Appointment> GetByPet(Pet pet)
    {
        if (pet == null)
            throw new ArgumentNullException(nameof(pet));

        return _appointments
            .Where(a => a.Pet.Id == pet.Id)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<Appointment> GetByVeterinarian(Veterinarian veterinarian)
    {
        if (veterinarian == null)
            throw new ArgumentNullException(nameof(veterinarian));

        return _appointments
            .Where(a => a.Veterinarian.Id == veterinarian.Id)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<Appointment> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        return _appointments
            .Where(a => a.AppointmentDateTime >= startDate && a.AppointmentDateTime <= endDate)
            .ToList()
            .AsReadOnly();
    }

    public void Update(Appointment appointment)
    {
        if (appointment == null)
            throw new ArgumentNullException(nameof(appointment));

        var existing = _appointments.FirstOrDefault(a => a.Id == appointment.Id);
        if (existing == null)
            throw new InvalidOperationException($"Appointment with ID {appointment.Id} not found");

        // Replace the appointment in the list
        int index = _appointments.IndexOf(existing);
        _appointments[index] = appointment;
    }

    public bool Remove(int appointmentId)
    {
        var appointment = _appointments.FirstOrDefault(a => a.Id == appointmentId);
        if (appointment == null)
            return false;

        return _appointments.Remove(appointment);
    }

    public bool IsVeterinarianAvailable(Veterinarian veterinarian, DateTime appointmentDateTime)
    {
        if (veterinarian == null)
            throw new ArgumentNullException(nameof(veterinarian));

        // Check for conflicts: veterinarian should not have another appointment at the same time
        // Using 1-hour window (appointments cannot overlap)
        var conflicts = _appointments.Where(a =>
            a.Veterinarian.Id == veterinarian.Id &&
            a.Status != AppointmentStatus.Cancelled &&
            a.AppointmentDateTime == appointmentDateTime
        ).ToList();

        return conflicts.Count == 0;
    }
}

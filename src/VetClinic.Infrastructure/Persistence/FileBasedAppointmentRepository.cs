namespace VetClinic.Infrastructure.Persistence;

using VetClinic.Application;
using VetClinic.Application.Persistence;
using VetClinic.Domain;

/// <summary>
/// File-based appointment repository with async persistence
/// Combines in-memory storage with JSON file persistence
/// </summary>
public class FileBasedAppointmentRepository : IAppointmentRepository
{
    private readonly List<Appointment> _appointments = [];
    private readonly List<MedicalRecord> _medicalRecords = [];
    private readonly JsonDataStore _dataStore;
    private bool _isInitialized = false;

    public FileBasedAppointmentRepository(JsonDataStore dataStore)
    {
        _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
    }

    /// <summary>
    /// Initialize repository by loading data from persistent storage
    /// Must be called before using the repository
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            return;

        try
        {
            var (appointments, medicalRecords) = await _dataStore.LoadAsync(cancellationToken);
            _appointments.Clear();
            _appointments.AddRange(appointments);

            _medicalRecords.Clear();
            _medicalRecords.AddRange(medicalRecords);

            _isInitialized = true;
        }
        catch (InvalidOperationException)
        {
            // File doesn't exist or is corrupted - start with empty repository
            _isInitialized = true;
        }
    }

    /// <summary>
    /// Persist current state to file
    /// </summary>
    public async Task PersistAsync(
        IReadOnlyCollection<Owner> owners,
        IReadOnlyCollection<Pet> pets,
        IReadOnlyCollection<Veterinarian> veterinarians,
        CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Repository not initialized. Call InitializeAsync first.");

        await _dataStore.SaveClinicDataAsync(
            _appointments.AsReadOnly(),
            _medicalRecords.AsReadOnly(),
            owners,
            pets,
            veterinarians,
            cancellationToken);
    }

    public void Add(Appointment appointment)
    {
        if (appointment == null)
            throw new ArgumentNullException(nameof(appointment));

        if (_appointments.Any(a => a.Id == appointment.Id))
            throw new InvalidOperationException(
                $"Appointment with ID {appointment.Id} already exists");

        _appointments.Add(appointment);
    }

    public void AddMedicalRecord(MedicalRecord record)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));

        if (_medicalRecords.Any(r => r.Id == record.Id))
            throw new InvalidOperationException(
                $"Medical record with ID {record.Id} already exists");

        _medicalRecords.Add(record);
    }

    public Appointment? GetById(int id)
    {
        return _appointments.FirstOrDefault(a => a.Id == id);
    }

    public MedicalRecord? GetMedicalRecordById(int id)
    {
        return _medicalRecords.FirstOrDefault(r => r.Id == id);
    }

    public IReadOnlyList<Appointment> GetAll()
    {
        return _appointments.AsReadOnly();
    }

    public IReadOnlyList<MedicalRecord> GetAllMedicalRecords()
    {
        return _medicalRecords.AsReadOnly();
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

    public IReadOnlyList<MedicalRecord> GetMedicalRecordsByPet(Pet pet)
    {
        if (pet == null)
            throw new ArgumentNullException(nameof(pet));

        return _medicalRecords
            .Where(r => r.Pet.Id == pet.Id)
            .OrderByDescending(r => r.VisitDate)
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
            .Where(a => a.AppointmentDateTime >= startDate &&
                        a.AppointmentDateTime <= endDate)
            .ToList()
            .AsReadOnly();
    }

    public void Update(Appointment appointment)
    {
        if (appointment == null)
            throw new ArgumentNullException(nameof(appointment));

        var existing = _appointments.FirstOrDefault(a => a.Id == appointment.Id);
        if (existing == null)
            throw new InvalidOperationException(
                $"Appointment with ID {appointment.Id} not found");

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

        var conflicts = _appointments.Where(a =>
            a.Veterinarian.Id == veterinarian.Id &&
            a.Status != AppointmentStatus.Cancelled &&
            a.AppointmentDateTime == appointmentDateTime
        ).ToList();

        return conflicts.Count == 0;
    }

    /// <summary>
    /// Get appointment ID to use for new appointments
    /// Simple implementation: find max ID + 1
    /// </summary>
    public int GetNextAppointmentId()
    {
        if (_appointments.Count == 0)
            return 1;
        return _appointments.Max(a => a.Id) + 1;
    }

    /// <summary>
    /// Get medical record ID to use for new records
    /// </summary>
    public int GetNextMedicalRecordId()
    {
        if (_medicalRecords.Count == 0)
            return 1;
        return _medicalRecords.Max(r => r.Id) + 1;
    }
}

namespace VetClinic.Infrastructure.Persistence;

using System.Text.Json;
using VetClinic.Application.Persistence;
using VetClinic.Domain;
using VetClinic.Infrastructure.Persistence.Dtos;

/// <summary>
/// JSON-based persistence implementation for clinic data.
/// Handles asynchronous file I/O with proper error handling and data validation.
/// Note: Does not implement IDataStore<T> directly; use through FileBasedAppointmentRepository
/// </summary>
public class JsonDataStore
{
    private readonly string _dataDirectory;
    private readonly string _dataFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Managed references to all domain entities for reconstruction
    /// </summary>
    private readonly Dictionary<int, Owner> _owners = [];
    private readonly Dictionary<int, Pet> _pets = [];
    private readonly Dictionary<int, Veterinarian> _veterinarians = [];
    private readonly List<Appointment> _appointments = [];
    private readonly List<MedicalRecord> _medicalRecords = [];

    public JsonDataStore(string? dataDirectory = null)
    {
        _dataDirectory = dataDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VetClinic");
        
        _dataFilePath = Path.Combine(_dataDirectory, "clinic_data.json");
    }

    /// <summary>
    /// Load all clinic data from persistent storage
    /// </summary>
    public async Task<(IReadOnlyCollection<Appointment> appointments, IReadOnlyCollection<MedicalRecord> medicalRecords)> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                return ([], []);
            }

            string json = await File.ReadAllTextAsync(_dataFilePath, cancellationToken);
            
            if (string.IsNullOrWhiteSpace(json))
            {
                return ([], []);
            }

            var snapshot = JsonSerializer.Deserialize<ClinicDataSnapshot>(json, JsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize clinic data");

            // Reconstruct domain entities
            ReconstructDomainEntities(snapshot);

            return (_appointments.AsReadOnly(), _medicalRecords.AsReadOnly());
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Corrupted clinic data file: {ex.Message}", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException(
                $"Error reading clinic data: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Save clinic data snapshot to persistent storage
    /// </summary>
    public async Task SaveClinicDataAsync(
        IReadOnlyCollection<Appointment> appointments,
        IReadOnlyCollection<MedicalRecord> medicalRecords,
        IReadOnlyCollection<Owner> owners,
        IReadOnlyCollection<Pet> pets,
        IReadOnlyCollection<Veterinarian> veterinarians,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(_dataDirectory);

            // Serialize to DTOs
            var snapshot = new ClinicDataSnapshot
            {
                Owners = owners.Select(o => new OwnerDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    PhoneNumber = o.PhoneNumber,
                    Email = o.Email
                }).ToList(),
                
                Pets = pets.Select(p => new PetDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    OwnerIdRef = p.Owner.Id,
                    Species = p.Species.ToString(),
                    Breed = p.Breed,
                    AgeInMonths = p.AgeInMonths
                }).ToList(),
                
                Veterinarians = veterinarians.Select(v => new VeterinarianDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    Specialization = v.Specialization,
                    LicenseNumber = v.LicenseNumber
                }).ToList(),
                
                Appointments = appointments.Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    PetIdRef = a.Pet.Id,
                    VeterinarianIdRef = a.Veterinarian.Id,
                    AppointmentDateTime = a.AppointmentDateTime,
                    Reason = a.Reason,
                    Status = a.Status.ToString(),
                    CreatedAt = a.CreatedAt
                }).ToList(),
                
                MedicalRecords = medicalRecords.Select(mr => new MedicalRecordDto
                {
                    Id = mr.Id,
                    AppointmentIdRef = mr.AppointmentId,
                    PetIdRef = mr.Pet.Id,
                    Diagnosis = mr.Diagnosis,
                    Treatment = mr.Treatment,
                    VisitDate = mr.VisitDate,
                    RecordedAt = mr.RecordedAt
                }).ToList()
            };

            string json = JsonSerializer.Serialize(snapshot, JsonOptions);
            
            // Write to temporary file first, then move (atomic operation)
            string tempFile = _dataFilePath + ".tmp";
            await File.WriteAllTextAsync(tempFile, json, cancellationToken);
            File.Move(tempFile, _dataFilePath, overwrite: true);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to serialize clinic data: {ex.Message}", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException(
                $"Error writing clinic data: {ex.Message}", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException(
                $"Access denied when saving clinic data: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Reconstruct domain entities from DTOs
    /// </summary>
    private void ReconstructDomainEntities(ClinicDataSnapshot snapshot)
    {
        _owners.Clear();
        _pets.Clear();
        _veterinarians.Clear();
        _appointments.Clear();
        _medicalRecords.Clear();

        // Reconstruct Owners
        foreach (var ownerDto in snapshot.Owners)
        {
            var owner = new Owner(ownerDto.Id, ownerDto.Name, ownerDto.PhoneNumber, ownerDto.Email);
            _owners.Add(owner.Id, owner);
        }

        // Reconstruct Veterinarians
        foreach (var vetDto in snapshot.Veterinarians)
        {
            var vet = new Veterinarian(vetDto.Id, vetDto.Name, vetDto.Specialization, vetDto.LicenseNumber);
            _veterinarians.Add(vet.Id, vet);
        }

        // Reconstruct Pets (with owner references)
        foreach (var petDto in snapshot.Pets)
        {
            if (_owners.TryGetValue(petDto.OwnerIdRef, out var owner))
            {
                if (Enum.TryParse<Species>(petDto.Species, out var species))
                {
                    var pet = new Pet(petDto.Id, petDto.Name, species, petDto.Breed, petDto.AgeInMonths, owner);
                    _pets.Add(pet.Id, pet);
                }
            }
        }

        // Reconstruct Appointments (with pet and vet references)
        foreach (var appointmentDto in snapshot.Appointments)
        {
            if (_pets.TryGetValue(appointmentDto.PetIdRef, out var pet) &&
                _veterinarians.TryGetValue(appointmentDto.VeterinarianIdRef, out var vet))
            {
                if (Enum.TryParse<AppointmentStatus>(appointmentDto.Status, out var status))
                {
                    // Create appointment with past date validation disabled for loading
                    var appointment = new Appointment(
                        appointmentDto.Id, pet, vet,
                        appointmentDto.AppointmentDateTime,
                        appointmentDto.Reason);

                    // Restore status if not Scheduled
                    if (status == AppointmentStatus.Completed)
                        appointment.Complete();
                    else if (status == AppointmentStatus.Cancelled)
                        appointment.Cancel();

                    _appointments.Add(appointment);
                }
            }
        }

        // Reconstruct MedicalRecords (with appointment and pet references)
        foreach (var recordDto in snapshot.MedicalRecords)
        {
            if (_pets.TryGetValue(recordDto.PetIdRef, out var pet))
            {
                var record = new MedicalRecord(
                    recordDto.Id,
                    recordDto.AppointmentIdRef,
                    pet,
                    recordDto.Diagnosis,
                    recordDto.Treatment,
                    recordDto.VisitDate);

                _medicalRecords.Add(record);
            }
        }
    }

    /// <summary>
    /// Get loaded appointments (used internally by repository)
    /// </summary>
    public IReadOnlyList<Appointment> GetLoadedAppointments() => _appointments.AsReadOnly();

    /// <summary>
    /// Get loaded medical records (used internally by repository)
    /// </summary>
    public IReadOnlyList<MedicalRecord> GetLoadedMedicalRecords() => _medicalRecords.AsReadOnly();

    /// <summary>
    /// Get loaded entities (used for reference resolution)
    /// </summary>
    public IReadOnlyDictionary<int, Owner> GetOwners() => _owners;
    public IReadOnlyDictionary<int, Pet> GetPets() => _pets;
    public IReadOnlyDictionary<int, Veterinarian> GetVeterinarians() => _veterinarians;

    /// <summary>
    /// Clear all loaded data (useful for unit tests)
    /// </summary>
    public void ClearAll()
    {
        _owners.Clear();
        _pets.Clear();
        _veterinarians.Clear();
        _appointments.Clear();
        _medicalRecords.Clear();
    }
}

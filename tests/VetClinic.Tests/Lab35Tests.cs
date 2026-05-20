namespace VetClinic.Tests;

using Xunit;
using VetClinic.Domain;
using VetClinic.Application;
using VetClinic.Infrastructure;

/// <summary>
/// Lab 35 Tests - Iteration 2: Business Logic, Data Persistence, Queries, and Extensibility
/// Tests for domain invariants, business rules, persistence layer, LINQ analytics, and pattern implementation
/// </summary>

/// <summary>
/// Tests for MedicalRecord domain invariants
/// Ensures data validation for diagnosis/treatment, date constraints
/// </summary>
public class MedicalRecordInvariantsTests
{
    private Owner CreateValidOwner() => new(1, "John Smith", "+1-555-0101", "john@email.com");
    private Pet CreateValidPet() => new(1, "Fluffy", Species.Cat, "Persian", 36, CreateValidOwner());

    [Fact]
    public void MedicalRecord_EmptyDiagnosis_ThrowsArgumentException()
    {
        // Arrange
        var pet = CreateValidPet();
        var visitDate = DateTime.Now.AddDays(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(1, 100, pet, "", "Treatment needed", visitDate)
        );
    }

    [Fact]
    public void MedicalRecord_DiagnosisTooLong_ThrowsArgumentException()
    {
        // Arrange
        var pet = CreateValidPet();
        var visitDate = DateTime.Now.AddDays(-1);
        var longDiagnosis = new string('a', 501);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(1, 100, pet, longDiagnosis, "Treatment needed", visitDate)
        );
    }

    [Fact]
    public void MedicalRecord_EmptyTreatment_ThrowsArgumentException()
    {
        // Arrange
        var pet = CreateValidPet();
        var visitDate = DateTime.Now.AddDays(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(1, 100, pet, "Flu", "", visitDate)
        );
    }

    [Fact]
    public void MedicalRecord_TreatmentTooLong_ThrowsArgumentException()
    {
        // Arrange
        var pet = CreateValidPet();
        var visitDate = DateTime.Now.AddDays(-1);
        var longTreatment = new string('a', 1001);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(1, 100, pet, "Flu", longTreatment, visitDate)
        );
    }

    [Fact]
    public void MedicalRecord_FutureVisitDate_ThrowsArgumentException()
    {
        // Arrange
        var pet = CreateValidPet();
        var futureDate = DateTime.Now.AddDays(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(1, 100, pet, "Flu", "Rest and fluids", futureDate)
        );
    }

    [Fact]
    public void MedicalRecord_ValidParameters_CreatedSuccessfully()
    {
        // Arrange
        var pet = CreateValidPet();
        var visitDate = DateTime.Now.AddDays(-1);

        // Act
        var record = new MedicalRecord(1, 100, pet, "Flu", "Rest and fluids", visitDate);

        // Assert
        Assert.NotNull(record);
        Assert.Equal(1, record.Id);
        Assert.Equal(100, record.AppointmentId);
        Assert.Equal("Flu", record.Diagnosis);
        Assert.Equal("Rest and fluids", record.Treatment);
    }
}

/// <summary>
/// Tests for MedicalRecord business logic via service layer
/// Ensures appointments must be completed before creating medical records
/// </summary>
public class MedicalRecordServiceTests
{
    private Owner CreateValidOwner() => new(1, "John Smith", "+1-555-0101", "john@email.com");
    private Pet CreateValidPet() => new(1, "Fluffy", Species.Cat, "Persian", 36, CreateValidOwner());
    private Veterinarian CreateValidVet() => new(1, "Dr. Sarah Wilson", "Small Animals", "VET-001");

    [Fact]
    public void CreateMedicalRecord_NonCompletedAppointment_ReturnsFailed()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repository);
        var pet = CreateValidPet();
        var vet = CreateValidVet();
        
        var appointment = new Appointment(1, pet, vet, DateTime.Now.AddDays(1), "Checkup");
        repository.AddAppointment(appointment);

        // Act
        var result = service.CreateMedicalRecord(1, appointment.Id, pet, "Flu", "Rest and fluids");

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void CreateMedicalRecord_CompletedAppointment_CreatesSuccessfully()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repository);
        var pet = CreateValidPet();
        var vet = CreateValidVet();
        
        var appointment = new Appointment(1, pet, vet, DateTime.Now.AddDays(-1), "Checkup");
        appointment.Complete();
        repository.AddAppointment(appointment);

        // Act
        var result = service.CreateMedicalRecord(1, appointment.Id, pet, "Normal checkup", "No treatment needed");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Normal checkup", result.Value.Diagnosis);
    }

    [Fact]
    public void GetPetMedicalHistory_WithMultipleRecords_ReturnsSortedByDate()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repository);
        var pet = CreateValidPet();
        
        var record1 = new MedicalRecord(1, 1, pet, "Flu", "Rest", DateTime.Now.AddDays(-5));
        var record2 = new MedicalRecord(2, 2, pet, "Allergy", "Medication", DateTime.Now.AddDays(-2));
        var record3 = new MedicalRecord(3, 3, pet, "Injury", "Bandage", DateTime.Now.AddDays(-10));
        
        repository.AddMedicalRecord(record1);
        repository.AddMedicalRecord(record2);
        repository.AddMedicalRecord(record3);

        // Act
        var history = service.GetPetMedicalHistory(pet);

        // Assert
        Assert.Equal(3, history.Count);
        // Newest first (record2 is most recent)
        Assert.Equal(record2.Id, history[0].Id);
        Assert.Equal(record1.Id, history[1].Id);
        Assert.Equal(record3.Id, history[2].Id);
    }

    [Fact]
    public void GetRecordsByDateRange_FiltersCorrectly()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repository);
        var pet = CreateValidPet();
        
        var record1 = new MedicalRecord(1, 1, pet, "Flu", "Rest", DateTime.Now.AddDays(-5));
        var record2 = new MedicalRecord(2, 2, pet, "Allergy", "Medication", DateTime.Now.AddDays(-2));
        var record3 = new MedicalRecord(3, 3, pet, "Injury", "Bandage", DateTime.Now.AddDays(-10));
        
        repository.AddMedicalRecord(record1);
        repository.AddMedicalRecord(record2);
        repository.AddMedicalRecord(record3);

        // Act
        var filtered = service.GetRecordsByDateRange(
            DateTime.Now.AddDays(-6),
            DateTime.Now.AddDays(-3)
        );

        // Assert
        Assert.Single(filtered);
        Assert.Equal(record1.Id, filtered[0].Id);
    }
}

/// <summary>
/// Tests for DiagnosisSeverityScorer strategy pattern implementation
/// Ensures multiple scoring strategies work correctly and can be swapped at runtime
/// </summary>
public class DiagnosisSeverityScorerTests
{
    [Fact]
    public void KeywordBasedScorer_HighSeverityDiagnosis_ReturnsHighScore()
    {
        // Arrange
        IDiagnosisSeverityScorer scorer = new KeywordBasedSeverityScorer();

        // Act
        var score = scorer.CalculateSeverityScore("fracture");
        var level = scorer.GetSeverityLevel(score);

        // Assert
        Assert.True(score >= 75, $"Fracture should score >= 75, got {score}");
        Assert.Equal("High", level);
    }

    [Fact]
    public void KeywordBasedScorer_MediumSeverityDiagnosis_ReturnsMediumScore()
    {
        // Arrange
        IDiagnosisSeverityScorer scorer = new KeywordBasedSeverityScorer();

        // Act
        var score = scorer.CalculateSeverityScore("infection");
        var level = scorer.GetSeverityLevel(score);

        // Assert
        Assert.True(score >= 60 && score < 80, $"Infection should score 60-80, got {score}");
        Assert.Equal("Medium", level);
    }

    [Fact]
    public void KeywordBasedScorer_LowSeverityDiagnosis_ReturnsLowScore()
    {
        // Arrange
        IDiagnosisSeverityScorer scorer = new KeywordBasedSeverityScorer();

        // Act
        var score = scorer.CalculateSeverityScore("rash");
        var level = scorer.GetSeverityLevel(score);

        // Assert
        Assert.True(score < 50, $"Rash should score < 50, got {score}");
        Assert.Equal("Low", level);
    }

    [Fact]
    public void LengthBasedScorer_ShortDiagnosis_ReturnsLowScore()
    {
        // Arrange
        IDiagnosisSeverityScorer scorer = new LengthBasedSeverityScorer();

        // Act
        var score = scorer.CalculateSeverityScore("Flu");

        // Assert
        Assert.True(score <= 30, $"Short diagnosis should score <= 30, got {score}");
    }

    [Fact]
    public void LengthBasedScorer_LongDiagnosis_ReturnsHigherScore()
    {
        // Arrange
        IDiagnosisSeverityScorer scorer = new LengthBasedSeverityScorer();
        var shortDiagnosis = "Flu";
        var longDiagnosis = "Chronic respiratory infection with suspected secondary bacterial colonization requiring extensive treatment";

        // Act
        var shortScore = scorer.CalculateSeverityScore(shortDiagnosis);
        var longScore = scorer.CalculateSeverityScore(longDiagnosis);

        // Assert
        Assert.True(longScore > shortScore, "Longer diagnosis should have higher score");
    }

    [Fact]
    public void DiagnosisAnalysisService_StrategySwapping_UsesCorrectScorer()
    {
        // Arrange
        var service = new DiagnosisAnalysisService();
        var diagnosis = "fracture";

        // Act - Analyze with keyword scorer
        service.SetSeverityScorer(new KeywordBasedSeverityScorer());
        var result1 = service.AnalyzeDiagnosis(diagnosis);

        // Act - Analyze with length scorer
        service.SetSeverityScorer(new LengthBasedSeverityScorer());
        var result2 = service.AnalyzeDiagnosis(diagnosis);

        // Assert
        Assert.NotEqual(result1.ScoringStrategy, result2.ScoringStrategy);
        Assert.Equal("KeywordBasedSeverityScorer", result1.ScoringStrategy);
        Assert.Equal("LengthBasedSeverityScorer", result2.ScoringStrategy);
    }
}

/// <summary>
/// Tests for AnalyticsService LINQ queries
/// Ensures complex queries produce correct aggregated results
/// </summary>
public class AnalyticsServiceLINQTests
{
    private Owner CreateOwner(int id, string name) => new(id, name, $"+1-555-010{id}", $"{name.ToLower()}@email.com");
    private Pet CreatePet(int id, string name, Owner owner) => new(id, name, Species.Cat, "Persian", 36, owner);
    private Veterinarian CreateVet(int id, string name) => new(id, $"Dr. {name}", "General", $"VET-00{id}");

    [Fact]
    public void GetVeterinarianStatistics_AggregatesCorrectly()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repository);
        var owner = CreateOwner(1, "John");
        var pet = CreatePet(1, "Fluffy", owner);
        var vet = CreateVet(1, "Smith");
        
        repository.AddAppointment(new Appointment(1, pet, vet, DateTime.Now.AddDays(-5), "Checkup"));
        repository.AddAppointment(new Appointment(2, pet, vet, DateTime.Now.AddDays(-3), "Vaccination"));
        repository.AddAppointment(new Appointment(3, pet, vet, DateTime.Now.AddDays(5), "Follow-up"));

        // Act
        var stats = analytics.GetVeterinarianStatistics(vet);

        // Assert
        Assert.Equal(3, stats.TotalAppointments);
        Assert.Equal(1, stats.UpcomingAppointments);
        Assert.Equal(0, stats.CancelledAppointments);
    }

    [Fact]
    public void SearchAppointments_FiltersByOwnerName()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repository);
        var owner1 = CreateOwner(1, "John");
        var owner2 = CreateOwner(2, "Mary");
        var pet1 = CreatePet(1, "Fluffy", owner1);
        var pet2 = CreatePet(2, "Rex", owner2);
        var vet = CreateVet(1, "Smith");
        
        repository.AddAppointment(new Appointment(1, pet1, vet, DateTime.Now.AddDays(1), "Checkup"));
        repository.AddAppointment(new Appointment(2, pet2, vet, DateTime.Now.AddDays(2), "Surgery"));

        // Act
        var results = analytics.SearchAppointments(ownerName: "John", petName: null, status: null, fromDate: null, toDate: null);

        // Assert
        Assert.Single(results);
        Assert.Equal("Fluffy", results[0].Pet.Name);
    }

    [Fact]
    public void SearchAppointments_FiltersByPetName()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repository);
        var owner1 = CreateOwner(1, "John");
        var owner2 = CreateOwner(2, "Mary");
        var pet1 = CreatePet(1, "Fluffy", owner1);
        var pet2 = CreatePet(2, "Rex", owner2);
        var vet = CreateVet(1, "Smith");
        
        repository.AddAppointment(new Appointment(1, pet1, vet, DateTime.Now.AddDays(1), "Checkup"));
        repository.AddAppointment(new Appointment(2, pet2, vet, DateTime.Now.AddDays(2), "Surgery"));

        // Act
        var results = analytics.SearchAppointments(ownerName: null, petName: "Rex", status: null, fromDate: null, toDate: null);

        // Assert
        Assert.Single(results);
        Assert.Equal("Mary", results[0].Pet.Owner.Name);
    }

    [Fact]
    public void GetPetMedicalProfile_AggregatesDiagnoses()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repository);
        var owner = CreateOwner(1, "John");
        var pet = CreatePet(1, "Fluffy", owner);
        
        repository.AddMedicalRecord(new MedicalRecord(1, 1, pet, "Flu", "Rest", DateTime.Now.AddDays(-5)));
        repository.AddMedicalRecord(new MedicalRecord(2, 2, pet, "Allergy", "Medication", DateTime.Now.AddDays(-3)));
        repository.AddMedicalRecord(new MedicalRecord(3, 3, pet, "Flu", "Rest", DateTime.Now.AddDays(-1)));

        // Act
        var profile = analytics.GetPetMedicalProfile(pet);

        // Assert
        Assert.Equal(2, profile.UniqueDiagnoses.Count);
        Assert.Contains("Flu", profile.UniqueDiagnoses);
        Assert.Contains("Allergy", profile.UniqueDiagnoses);
    }

    [Fact]
    public void GetClinicStatistics_CalculatesRatesCorrectly()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repository);
        var owner = CreateOwner(1, "John");
        var pet = CreatePet(1, "Fluffy", owner);
        var vet = CreateVet(1, "Smith");
        
        var appt1 = new Appointment(1, pet, vet, DateTime.Now.AddDays(-1), "Checkup");
        appt1.Complete();
        var appt2 = new Appointment(2, pet, vet, DateTime.Now.AddDays(-1), "Checkup");
        appt2.Cancel();
        
        repository.AddAppointment(appt1);
        repository.AddAppointment(appt2);

        // Act
        var stats = analytics.GetClinicStatistics();

        // Assert
        Assert.Equal(2, stats.TotalAppointments);
        Assert.Equal(1, stats.CompletedAppointments);
        Assert.Equal(1, stats.CancelledAppointments);
        Assert.Equal(50.0, stats.CompletionRate);
        Assert.Equal(50.0, stats.CancellationRate);
    }

    [Fact]
    public void GetVeterinarianUtilization_CalculatesRateCorrectly()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repository);
        var owner = CreateOwner(1, "John");
        var pet = CreatePet(1, "Fluffy", owner);
        var vet1 = CreateVet(1, "Smith");
        var vet2 = CreateVet(2, "Chen");
        
        repository.AddAppointment(new Appointment(1, pet, vet1, DateTime.Now.AddDays(1), "Checkup"));
        repository.AddAppointment(new Appointment(2, pet, vet1, DateTime.Now.AddDays(2), "Checkup"));
        repository.AddAppointment(new Appointment(3, pet, vet2, DateTime.Now.AddDays(3), "Checkup"));

        // Act
        var utilization = analytics.GetVeterinarianUtilization();

        // Assert
        Assert.Equal(2, utilization.TotalVeterinarians);
        Assert.Equal(3, utilization.TotalAppointments);
        var expectedRate = Math.Round((3.0 / 2) * 100, 2);
        Assert.Equal(expectedRate, utilization.UtilizationRate);
    }
}

/// <summary>
/// Tests for persistence layer (file-based repository)
/// Ensures data loading, saving, and error handling work correctly
/// </summary>
public class PersistenceLayerTests
{
    [Fact]
    public async Task FileBasedRepository_Initialize_WithEmptyData_StartsClean()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();

        // Act - Create appointments
        var owner = new Owner(1, "John", "+1234567890", "john@example.com");
        var pet = new Pet(1, "Fluffy", Species.Cat, "Persian", 36, owner);
        var vet = new Veterinarian(1, "Dr. Smith", "General", "VET-001");
        
        repository.AddAppointment(new Appointment(1, pet, vet, DateTime.Now.AddDays(1), "Checkup"));

        // Assert
        var all = repository.GetAllAppointments();
        Assert.Single(all);
    }

    [Fact]
    public void InMemoryRepository_AddAndRetrieveAppointments_Works()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var owner = new Owner(1, "John", "+1234567890", "john@example.com");
        var pet = new Pet(1, "Fluffy", Species.Cat, "Persian", 36, owner);
        var vet = new Veterinarian(1, "Dr. Smith", "General", "VET-001");
        
        var appointment = new Appointment(1, pet, vet, DateTime.Now.AddDays(1), "Checkup");

        // Act
        repository.AddAppointment(appointment);
        var retrieved = repository.GetAppointmentById(1);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(appointment.Id, retrieved.Id);
    }

    [Fact]
    public void InMemoryRepository_AddAndRetrieveMedicalRecords_Works()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var owner = new Owner(1, "John", "+1234567890", "john@example.com");
        var pet = new Pet(1, "Fluffy", Species.Cat, "Persian", 36, owner);
        
        var record = new MedicalRecord(1, 1, pet, "Flu", "Rest and fluids", DateTime.Now.AddDays(-1));

        // Act
        repository.AddMedicalRecord(record);
        var retrieved = repository.GetMedicalRecordById(1);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(record.Id, retrieved.Id);
        Assert.Equal("Flu", retrieved.Diagnosis);
    }

    [Fact]
    public void Repository_GetMedicalRecordsByPet_FilterCorrectly()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var owner = new Owner(1, "John", "+1234567890", "john@example.com");
        var pet1 = new Pet(1, "Fluffy", Species.Cat, "Persian", 36, owner);
        var pet2 = new Pet(2, "Rex", Species.Dog, "German Shepherd", 60, owner);
        
        repository.AddMedicalRecord(new MedicalRecord(1, 1, pet1, "Flu", "Rest", DateTime.Now.AddDays(-5)));
        repository.AddMedicalRecord(new MedicalRecord(2, 2, pet2, "Allergy", "Medication", DateTime.Now.AddDays(-3)));
        repository.AddMedicalRecord(new MedicalRecord(3, 3, pet1, "Injury", "Bandage", DateTime.Now.AddDays(-1)));

        // Act
        var pet1Records = repository.GetMedicalRecordsByPet(pet1);

        // Assert
        Assert.Equal(2, pet1Records.Count);
        Assert.All(pet1Records, r => Assert.Equal(pet1.Id, r.Pet.Id));
    }
}

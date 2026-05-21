namespace VetClinic.Tests;

using Xunit;
using VetClinic.Domain;
using VetClinic.Application;
using VetClinic.Application.Strategies;
using VetClinic.Infrastructure;

/// <summary>
/// Lab 35 Tests - Concurrent test suite with proper synchronization
/// All tests use InMemoryAppointmentRepository to avoid file I/O conflicts
/// Collection: "Sequential" - runs tests one at a time to prevent parallel issues
/// </summary>

[Collection("Sequential")]
public class MedicalRecordInvariantsTests
{
    private Owner CreateValidOwner() => new(1, "John Smith", "+1-555-0101", "john@email.com");
    private Pet CreateValidPet() => new(1, "Fluffy", Species.Cat, "Persian", 36, CreateValidOwner());

    [Fact]
    public void MedicalRecord_EmptyDiagnosis_ThrowsArgumentException()
    {
        var pet = CreateValidPet();
        var visitDate = DateTime.Now.AddDays(-1);
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(1, 100, pet, "", "Treatment", visitDate));
    }

    [Fact]
    public void MedicalRecord_DiagnosisTooLong_ThrowsArgumentException()
    {
        var pet = CreateValidPet();
        var visitDate = DateTime.Now.AddDays(-1);
        var longDiagnosis = new string('a', 501);
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(1, 100, pet, longDiagnosis, "Treatment", visitDate));
    }

    [Fact]
    public void MedicalRecord_EmptyTreatment_ThrowsArgumentException()
    {
        var pet = CreateValidPet();
        var visitDate = DateTime.Now.AddDays(-1);
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(1, 100, pet, "Flu", "", visitDate));
    }

    [Fact]
    public void MedicalRecord_TreatmentTooLong_ThrowsArgumentException()
    {
        var pet = CreateValidPet();
        var visitDate = DateTime.Now.AddDays(-1);
        var longTreatment = new string('a', 1001);
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(1, 100, pet, "Flu", longTreatment, visitDate));
    }

    [Fact]
    public void MedicalRecord_FutureVisitDate_ThrowsArgumentException()
    {
        var pet = CreateValidPet();
        var futureDate = DateTime.Now.AddDays(1);
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(1, 100, pet, "Flu", "Rest", futureDate));
    }

    [Fact]
    public void MedicalRecord_ValidParameters_CreatedSuccessfully()
    {
        var pet = CreateValidPet();
        var visitDate = DateTime.Now.AddDays(-1);
        var record = new MedicalRecord(1, 100, pet, "Flu", "Rest", visitDate);
        Assert.Equal(1, record.Id);
        Assert.Equal("Flu", record.Diagnosis);
    }
}

[Collection("Sequential")]
public class MedicalRecordServiceTests
{
    private Owner CreateValidOwner() => new(1, "John Smith", "+1-555-0101", "john@email.com");
    private Pet CreateValidPet() => new(1, "Fluffy", Species.Cat, "Persian", 36, CreateValidOwner());
    private Veterinarian CreateValidVet() => new(1, "Dr. Sarah Wilson", "Small Animals", "VET-001");

    [Fact]
    public void CreateMedicalRecord_NonCompletedAppointment_ReturnsFailed()
    {
        var repository = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repository);
        var pet = CreateValidPet();
        var vet = CreateValidVet();
        var appointment = new Appointment(1, pet, vet, DateTime.Now.AddDays(1), "Checkup");
        repository.Add(appointment);

        var result = service.CreateMedicalRecord(appointment, pet, "Flu", "Rest");
        Assert.False(result.Success);
    }

    [Fact]
    public void CreateMedicalRecord_CompletedAppointment_CreatesSuccessfully()
    {
        var repository = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repository);
        var pet = CreateValidPet();
        var vet = CreateValidVet();
        var appointment = new Appointment(1, pet, vet, DateTime.Now.AddMinutes(10), "Checkup");
        appointment.Complete();
        repository.Add(appointment);

        var result = service.CreateMedicalRecord(appointment, pet, "Flu", "Rest");
        // Debug: check status
        var retrieved = repository.GetById(1);
        Assert.NotNull(retrieved);
        Assert.Equal(AppointmentStatus.Completed, retrieved.Status);
        Assert.True(result.Success, $"Expected success but got: {result.Message}");
        Assert.NotNull(result.Data);
    }

    [Fact]
    public void GetPetMedicalHistory_SortedByDateNewestFirst()
    {
        var repository = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repository);
        var pet = CreateValidPet();
        
        repository.AddMedicalRecord(new MedicalRecord(1, 1, pet, "Flu", "Rest", DateTime.Now.AddDays(-5)));
        repository.AddMedicalRecord(new MedicalRecord(2, 2, pet, "Allergy", "Med", DateTime.Now.AddDays(-2)));
        repository.AddMedicalRecord(new MedicalRecord(3, 3, pet, "Injury", "Bandage", DateTime.Now.AddDays(-10)));

        var history = service.GetPetMedicalHistory(pet);
        Assert.Equal(3, history.Count);
        Assert.Equal(2, history[0].Id); // Most recent
    }
}

[Collection("Sequential")]
public class DiagnosisSeverityScorerTests
{
    [Fact]
    public void KeywordBasedScorer_HighSeverityDiagnosis_ReturnsHighScore()
    {
        IDiagnosisSeverityScorer scorer = new KeywordBasedSeverityScorer();
        var score = scorer.CalculateSeverityScore("fracture");
        Assert.True(score >= 75);
    }

    [Fact]
    public void KeywordBasedScorer_MediumSeverityDiagnosis_ReturnsMediumScore()
    {
        IDiagnosisSeverityScorer scorer = new KeywordBasedSeverityScorer();
        var score = scorer.CalculateSeverityScore("infection");
        Assert.True(score >= 60 && score < 80);
    }

    [Fact]
    public void KeywordBasedScorer_LowSeverityDiagnosis_ReturnsLowScore()
    {
        IDiagnosisSeverityScorer scorer = new KeywordBasedSeverityScorer();
        var score = scorer.CalculateSeverityScore("rash");
        Assert.True(score < 50);
    }

    [Fact]
    public void LengthBasedScorer_ShortDiagnosis_ReturnsLowScore()
    {
        IDiagnosisSeverityScorer scorer = new LengthBasedSeverityScorer();
        var score = scorer.CalculateSeverityScore("Flu");
        Assert.True(score <= 30);
    }

    [Fact]
    public void LengthBasedScorer_LongDiagnosis_ReturnsHigherScore()
    {
        IDiagnosisSeverityScorer scorer = new LengthBasedSeverityScorer();
        var shortScore = scorer.CalculateSeverityScore("Flu");
        var longScore = scorer.CalculateSeverityScore("Chronic respiratory infection with secondary bacterial colonization");
        Assert.True(longScore > shortScore);
    }

    [Fact]
    public void DiagnosisAnalysisService_StrategySwapping_Works()
    {
        var service = new DiagnosisAnalysisService();
        service.SetSeverityScorer(new KeywordBasedSeverityScorer());
        var result1 = service.AnalyzeDiagnosis("fracture");
        
        service.SetSeverityScorer(new LengthBasedSeverityScorer());
        var result2 = service.AnalyzeDiagnosis("fracture");
        
        Assert.NotEqual(result1.SeverityScore, result2.SeverityScore);
    }
}

[Collection("Sequential")]
public class AnalyticsServiceTests
{
    private Owner CreateOwner(int id) => new(id, $"Owner{id}", "+1-555-0000", $"owner{id}@email.com");
    private Pet CreatePet(int id, Owner owner) => new(id, $"Pet{id}", Species.Cat, "Persian", 36, owner);
    private Veterinarian CreateVet(int id) => new(id, $"Dr. Smith{id}", "General", $"VET-00{id}");

    [Fact]
    public void GetVeterinarianStatistics_WorksCorrectly()
    {
        var repository = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repository);
        var owner = CreateOwner(1);
        var pet = CreatePet(1, owner);
        var vet = CreateVet(1);
        
        repository.Add(new Appointment(1, pet, vet, DateTime.Now.AddMinutes(1), "Checkup"));
        repository.Add(new Appointment(2, pet, vet, DateTime.Now.AddDays(5), "Follow-up"));

        var stats = analytics.GetVeterinarianStatistics(vet);
        Assert.Equal(2, stats.TotalAppointments);
    }

    [Fact]
    public void SearchAppointments_FiltersByOwner()
    {
        var repository = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repository);
        var owner1 = CreateOwner(1);
        var owner2 = CreateOwner(2);
        var pet1 = CreatePet(1, owner1);
        var pet2 = CreatePet(2, owner2);
        var vet = CreateVet(1);
        
        repository.Add(new Appointment(1, pet1, vet, DateTime.Now.AddDays(1), "Check"));
        repository.Add(new Appointment(2, pet2, vet, DateTime.Now.AddDays(2), "Check"));

        var results = analytics.SearchAppointments("Owner1", null, null, null, null);
        Assert.Single(results);
    }

    [Fact]
    public void GetPetMedicalProfile_AggregateDiagnoses()
    {
        var repository = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repository);
        var owner = CreateOwner(1);
        var pet = CreatePet(1, owner);
        
        repository.AddMedicalRecord(new MedicalRecord(1, 1, pet, "Flu", "Rest", DateTime.Now.AddDays(-5)));
        repository.AddMedicalRecord(new MedicalRecord(2, 2, pet, "Allergy", "Meds", DateTime.Now.AddDays(-3)));

        var profile = analytics.GetPetMedicalProfile(pet);
        Assert.Equal(2, profile.UniqueDiagnoses.Count);
    }

    [Fact]
    public void GetClinicStatistics_CalculatesRates()
    {
        var repository = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repository);
        var owner = CreateOwner(1);
        var pet = CreatePet(1, owner);
        var vet = CreateVet(1);
        
        var appt = new Appointment(1, pet, vet, DateTime.Now.AddMinutes(1), "Check");
        appt.Complete();
        repository.Add(appt);

        var stats = analytics.GetClinicStatistics();
        Assert.Equal(1, stats.TotalAppointments);
        Assert.Equal(100.0, stats.CompletionRate);
    }

    [Fact]
    public void GetVeterinarianUtilization_CalculatesRate()
    {
        var repository = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repository);
        var owner = CreateOwner(1);
        var pet = CreatePet(1, owner);
        var vet = CreateVet(1);
        
        repository.Add(new Appointment(1, pet, vet, DateTime.Now.AddDays(1), "Check"));

        var util = analytics.GetVeterinarianUtilization();
        Assert.NotEmpty(util);
        Assert.Equal(1, util.Count);
    }
}

[Collection("Sequential")]
public class RepositoryTests
{
    [Fact]
    public void InMemoryRepository_AddAppointment_Works()
    {
        var repo = new InMemoryAppointmentRepository();
        var owner = new Owner(1, "John", "+1234567890", "john@example.com");
        var pet = new Pet(1, "Fluffy", Species.Cat, "Persian", 36, owner);
        var vet = new Veterinarian(1, "Dr. Smith", "General", "VET-001");
        var appt = new Appointment(1, pet, vet, DateTime.Now.AddDays(1), "Check");
        
        repo.Add(appt);
        var retrieved = repo.GetById(1);
        
        Assert.NotNull(retrieved);
    }

    [Fact]
    public void InMemoryRepository_AddMedicalRecord_Works()
    {
        var repo = new InMemoryAppointmentRepository();
        var owner = new Owner(1, "John", "+1234567890", "john@example.com");
        var pet = new Pet(1, "Fluffy", Species.Cat, "Persian", 36, owner);
        var record = new MedicalRecord(1, 1, pet, "Flu", "Rest", DateTime.Now.AddDays(-1));
        
        repo.AddMedicalRecord(record);
        var retrieved = repo.GetMedicalRecordById(1);
        
        Assert.NotNull(retrieved);
    }

    [Fact]
    public void Repository_GetMedicalRecordsByPet_FiltersCorrectly()
    {
        var repo = new InMemoryAppointmentRepository();
        var owner = new Owner(1, "John", "+1234567890", "john@example.com");
        var pet1 = new Pet(1, "Fluffy", Species.Cat, "Persian", 36, owner);
        var pet2 = new Pet(2, "Rex", Species.Dog, "German Shepherd", 60, owner);
        
        repo.AddMedicalRecord(new MedicalRecord(1, 1, pet1, "Flu", "Rest", DateTime.Now.AddDays(-5)));
        repo.AddMedicalRecord(new MedicalRecord(2, 2, pet2, "Allergy", "Meds", DateTime.Now.AddDays(-3)));
        
        var records = repo.GetMedicalRecordsByPet(pet1);
        Assert.Single(records);
    }

    [Fact]
    public void Repository_GetAll_ReturnsAll()
    {
        var repo = new InMemoryAppointmentRepository();
        var owner = new Owner(1, "John", "+1234567890", "john@example.com");
        var pet = new Pet(1, "Fluffy", Species.Cat, "Persian", 36, owner);
        var vet = new Veterinarian(1, "Dr. Smith", "General", "VET-001");
        
        repo.Add(new Appointment(1, pet, vet, DateTime.Now.AddDays(1), "Check1"));
        repo.Add(new Appointment(2, pet, vet, DateTime.Now.AddDays(2), "Check2"));
        
        var all = repo.GetAll();
        Assert.Equal(2, all.Count);
    }
}

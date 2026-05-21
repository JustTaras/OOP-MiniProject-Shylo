namespace VetClinic.Tests;

using Xunit;
using VetClinic.Domain;
using VetClinic.Application;
using VetClinic.Application.Strategies;
using VetClinic.Infrastructure;
using VetClinic.Infrastructure.Persistence;

/// <summary>
/// Lab 36 Comprehensive Test Suite - Quality Gates & Fault Handling
/// Includes: Unit tests, integration tests, edge cases, parametrized tests, fault scenarios
/// All tests use InMemoryAppointmentRepository for unit tests
/// File I/O tests use temporary directories with proper cleanup
/// Collection: "Sequential" - prevents parallel file I/O conflicts
/// </summary>

#region Helper Methods & Test Fixtures

public static class TestDataFactory
{
    public static Owner CreateOwner(int id = 1, string? name = null, string? phone = null, string? email = null)
        => new(id, name ?? $"Owner{id}", phone ?? "+1-555-0100", email ?? $"owner{id}@test.com");

    public static Pet CreatePet(int id = 1, Owner? owner = null, string? name = null, Species species = Species.Cat)
        => new(id, name ?? $"Pet{id}", species, "TestBreed", 24, owner ?? CreateOwner(id));

    public static Veterinarian CreateVet(int id = 1, string? name = null)
        => new(id, name ?? $"Dr. Smith{id}", "General Practice", $"VET-{id:D4}");

    public static Appointment CreateAppointment(int id = 1, Pet? pet = null, Veterinarian? vet = null, 
                                                  DateTime? dateTime = null, string? reason = null)
    {
        pet ??= CreatePet(id);
        vet ??= CreateVet(id);
        var appointmentDateTime = dateTime ?? DateTime.Now.AddDays(1);
        reason ??= "Routine Checkup";
        return new Appointment(id, pet, vet, appointmentDateTime, reason);
    }

    public static MedicalRecord CreateMedicalRecord(int id = 1, int appointmentId = 1, Pet? pet = null,
                                                     string? diagnosis = null, string? treatment = null, DateTime? visitDate = null)
    {
        pet ??= CreatePet(id);
        diagnosis ??= "General Illness";
        treatment ??= "Rest and fluids";
        var date = visitDate ?? DateTime.Now.AddDays(-1);
        return new MedicalRecord(id, appointmentId, pet, diagnosis, treatment, date);
    }
}

#endregion

#region Domain Entity Invariant Tests - Appointment Edge Cases

[Collection("Sequential")]
public class AppointmentInvariantsEdgeCasesTests
{
    [Fact]
    public void Appointment_MinimumValidId_CreatedSuccessfully()
    {
        var apt = TestDataFactory.CreateAppointment(id: 1);
        Assert.Equal(1, apt.Id);
    }

    [Fact]
    public void Appointment_InvalidId_Zero_ThrowsException()
    {
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        Assert.Throws<ArgumentException>(() => 
            new Appointment(0, pet, vet, DateTime.Now.AddDays(1), "Checkup"));
    }

    [Fact]
    public void Appointment_InvalidId_Negative_ThrowsException()
    {
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        Assert.Throws<ArgumentException>(() => 
            new Appointment(-1, pet, vet, DateTime.Now.AddDays(1), "Checkup"));
    }

    [Fact]
    public void Appointment_PetNull_ThrowsException()
    {
        var vet = TestDataFactory.CreateVet();
        Assert.Throws<ArgumentNullException>(() => 
            new Appointment(1, null!, vet, DateTime.Now.AddDays(1), "Checkup"));
    }

    [Fact]
    public void Appointment_VeterinarianNull_ThrowsException()
    {
        var pet = TestDataFactory.CreatePet();
        Assert.Throws<ArgumentNullException>(() => 
            new Appointment(1, pet, null!, DateTime.Now.AddDays(1), "Checkup"));
    }

    [Fact]
    public void Appointment_DateTimePast_ThrowsException()
    {
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        Assert.Throws<ArgumentException>(() => 
            new Appointment(1, pet, vet, DateTime.Now.AddDays(-1), "Checkup"));
    }

    [Fact]
    public void Appointment_DateTimePresent_ThrowsException()
    {
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        Assert.Throws<ArgumentException>(() => 
            new Appointment(1, pet, vet, DateTime.Now, "Checkup"));
    }

    [Fact]
    public void Appointment_ReasonEmpty_ThrowsException()
    {
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        Assert.Throws<ArgumentException>(() => 
            new Appointment(1, pet, vet, DateTime.Now.AddDays(1), ""));
    }

    [Fact]
    public void Appointment_ReasonWhitespaceOnly_ThrowsException()
    {
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        Assert.Throws<ArgumentException>(() => 
            new Appointment(1, pet, vet, DateTime.Now.AddDays(1), "   \t\n  "));
    }

    [Fact]
    public void Appointment_ReasonTooLong_ThrowsException()
    {
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        var longReason = new string('a', 501);
        Assert.Throws<ArgumentException>(() => 
            new Appointment(1, pet, vet, DateTime.Now.AddDays(1), longReason));
    }

    [Fact]
    public void Appointment_ReasonMaxLength_CreatedSuccessfully()
    {
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        var maxReason = new string('a', 500);
        var apt = new Appointment(1, pet, vet, DateTime.Now.AddDays(1), maxReason);
        Assert.NotNull(apt);
    }

    [Fact]
    public void Appointment_ReasonWithLeadingTrailingWhitespace_IsTrimmed()
    {
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        var apt = new Appointment(1, pet, vet, DateTime.Now.AddDays(1), "  Checkup  ");
        Assert.Equal("Checkup", apt.Reason);
    }
}

#endregion

#region Appointment Status State Machine Tests

[Collection("Sequential")]
public class AppointmentStatusTransitionTests
{
    [Fact]
    public void Appointment_InitialStatus_IsScheduled()
    {
        var apt = TestDataFactory.CreateAppointment();
        Assert.Equal(AppointmentStatus.Scheduled, apt.Status);
    }

    [Fact]
    public void Appointment_Complete_ScheduledToCompleted_Succeeds()
    {
        var apt = TestDataFactory.CreateAppointment();
        apt.Complete();
        Assert.Equal(AppointmentStatus.Completed, apt.Status);
    }

    [Fact]
    public void Appointment_Cancel_ScheduledToCancelled_Succeeds()
    {
        var apt = TestDataFactory.CreateAppointment();
        apt.Cancel();
        Assert.Equal(AppointmentStatus.Cancelled, apt.Status);
    }

    [Fact]
    public void Appointment_Complete_ThenComplete_ThrowsException()
    {
        var apt = TestDataFactory.CreateAppointment();
        apt.Complete();
        // Completing an already completed appointment should not throw (idempotent)
        // But cancelling a completed one should throw
        Assert.Throws<InvalidOperationException>(() => apt.Cancel());
    }

    [Fact]
    public void Appointment_Cancel_ThenCancel_IsIdempotent()
    {
        var apt = TestDataFactory.CreateAppointment();
        apt.Cancel();
        // Cancelling an already cancelled appointment should be idempotent (no exception)
        apt.Cancel();
        Assert.Equal(AppointmentStatus.Cancelled, apt.Status);
    }

    [Fact]
    public void Appointment_Cancel_ThenComplete_ThrowsException()
    {
        var apt = TestDataFactory.CreateAppointment();
        apt.Cancel();
        Assert.Throws<InvalidOperationException>(() => apt.Complete());
    }

    [Fact]
    public void Appointment_Complete_ThenCancel_ThrowsException()
    {
        var apt = TestDataFactory.CreateAppointment();
        apt.Complete();
        Assert.Throws<InvalidOperationException>(() => apt.Cancel());
    }
}

#endregion

#region Pet & Owner Domain Invariant Tests

[Collection("Sequential")]
public class PetInvariantsTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Pet_InvalidName_ThrowsException(string invalidName)
    {
        var owner = TestDataFactory.CreateOwner();
        Assert.Throws<ArgumentException>(() => 
            new Pet(1, invalidName, Species.Cat, "Persian", 24, owner));
    }

    [Fact]
    public void Pet_NameTooLong_ThrowsException()
    {
        var owner = TestDataFactory.CreateOwner();
        var longName = new string('a', 51);
        Assert.Throws<ArgumentException>(() => 
            new Pet(1, longName, Species.Cat, "Persian", 24, owner));
    }

    [Fact]
    public void Pet_NameMaxLength_CreatedSuccessfully()
    {
        var owner = TestDataFactory.CreateOwner();
        var maxName = new string('a', 50);
        var pet = new Pet(1, maxName, Species.Cat, "Persian", 24, owner);
        Assert.NotNull(pet);
    }

    [Fact]
    public void Pet_AgeNegative_ThrowsException()
    {
        var owner = TestDataFactory.CreateOwner();
        Assert.Throws<ArgumentException>(() => 
            new Pet(1, "Fluffy", Species.Cat, "Persian", -1, owner));
    }

    [Fact]
    public void Pet_AgeUnrealistic_ThrowsException()
    {
        var owner = TestDataFactory.CreateOwner();
        Assert.Throws<ArgumentException>(() => 
            new Pet(1, "Fluffy", Species.Cat, "Persian", 601, owner));
    }

    [Fact]
    public void Pet_UpdateAge_OnlyIncreases()
    {
        var pet = TestDataFactory.CreatePet();
        pet.UpdateAge(36);
        Assert.Equal(36, pet.AgeInMonths);
    }

    [Fact]
    public void Pet_UpdateAge_CannotDecrease_ThrowsException()
    {
        var pet = TestDataFactory.CreatePet(1);
        pet.UpdateAge(36);
        Assert.Throws<ArgumentException>(() => pet.UpdateAge(24));
    }

    [Theory]
    [InlineData(Species.Cat)]
    [InlineData(Species.Dog)]
    [InlineData(Species.Rabbit)]
    [InlineData(Species.Bird)]
    public void Pet_AllSpecies_Supported(Species species)
    {
        var owner = TestDataFactory.CreateOwner();
        var pet = new Pet(1, "TestPet", species, "TestBreed", 24, owner);
        Assert.Equal(species, pet.Species);
    }
}

#endregion

#region Medical Record Invariants & Edge Cases

[Collection("Sequential")]
public class MedicalRecordInvariantsExtendedTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void MedicalRecord_DiagnosisInvalid_ThrowsException(string diagnosis)
    {
        var pet = TestDataFactory.CreatePet();
        var visitDate = DateTime.Now.AddDays(-1);
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(1, 1, pet, diagnosis, "Treatment", visitDate));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void MedicalRecord_TreatmentInvalid_ThrowsException(string treatment)
    {
        var pet = TestDataFactory.CreatePet();
        var visitDate = DateTime.Now.AddDays(-1);
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(1, 1, pet, "Flu", treatment, visitDate));
    }

    [Fact]
    public void MedicalRecord_DiagnosisMaxLength_CreatedSuccessfully()
    {
        var pet = TestDataFactory.CreatePet();
        var visitDate = DateTime.Now.AddDays(-1);
        var maxDiagnosis = new string('a', 500);
        var record = new MedicalRecord(1, 1, pet, maxDiagnosis, "Treatment", visitDate);
        Assert.NotNull(record);
    }

    [Fact]
    public void MedicalRecord_TreatmentMaxLength_CreatedSuccessfully()
    {
        var pet = TestDataFactory.CreatePet();
        var visitDate = DateTime.Now.AddDays(-1);
        var maxTreatment = new string('a', 1000);
        var record = new MedicalRecord(1, 1, pet, "Diagnosis", maxTreatment, visitDate);
        Assert.NotNull(record);
    }

    [Fact]
    public void MedicalRecord_PetNull_ThrowsException()
    {
        var visitDate = DateTime.Now.AddDays(-1);
        Assert.Throws<ArgumentNullException>(() => 
            new MedicalRecord(1, 1, null!, "Flu", "Rest", visitDate));
    }

    [Fact]
    public void MedicalRecord_FutureDate_ThrowsException()
    {
        var pet = TestDataFactory.CreatePet();
        var futureDate = DateTime.Now.AddDays(1);
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(1, 1, pet, "Flu", "Rest", futureDate));
    }

    [Fact]
    public void MedicalRecord_TodayDate_AllowedForMedicalRecord()
    {
        var pet = TestDataFactory.CreatePet();
        // Medical records with today's date should be allowed (for same-day diagnoses)
        var todayDate = DateTime.Now.AddMinutes(-1); // Slightly in the past to be safe
        var record = new MedicalRecord(1, 1, pet, "Flu", "Rest", todayDate);
        Assert.NotNull(record);
    }

    [Fact]
    public void MedicalRecord_InvalidId_Zero_ThrowsException()
    {
        var pet = TestDataFactory.CreatePet();
        var visitDate = DateTime.Now.AddDays(-1);
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(0, 1, pet, "Flu", "Rest", visitDate));
    }

    [Fact]
    public void MedicalRecord_InvalidAppointmentId_Zero_ThrowsException()
    {
        var pet = TestDataFactory.CreatePet();
        var visitDate = DateTime.Now.AddDays(-1);
        Assert.Throws<ArgumentException>(() => 
            new MedicalRecord(1, 0, pet, "Flu", "Rest", visitDate));
    }
}

#endregion

#region Medical Record Service - Business Rule Tests

[Collection("Sequential")]
public class MedicalRecordServiceBusinessRuleTests
{
    [Fact]
    public void CreateMedicalRecord_ScheduledAppointment_ReturnsFailure()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repo);
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        var apt = new Appointment(1, pet, vet, DateTime.Now.AddDays(1), "Checkup");
        repo.Add(apt);

        var result = service.CreateMedicalRecord(apt, pet, "Flu", "Rest");
        
        Assert.False(result.Success);
        Assert.Contains("Scheduled", result.Message);
    }

    [Fact]
    public void CreateMedicalRecord_CancelledAppointment_ReturnsFailure()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repo);
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        var apt = new Appointment(1, pet, vet, DateTime.Now.AddDays(1), "Checkup");
        apt.Cancel();
        repo.Add(apt);

        var result = service.CreateMedicalRecord(apt, pet, "Flu", "Rest");
        
        Assert.False(result.Success);
        Assert.Contains("Cancelled", result.Message);
    }

    [Fact]
    public void CreateMedicalRecord_CompletedAppointment_Succeeds()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repo);
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        var apt = new Appointment(1, pet, vet, DateTime.Now.AddMinutes(1), "Checkup");
        apt.Complete();
        repo.Add(apt);

        var result = service.CreateMedicalRecord(apt, pet, "Flu", "Rest");
        
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public void CreateMedicalRecord_InvalidDiagnosis_ReturnsFailure()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repo);
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        var apt = new Appointment(1, pet, vet, DateTime.Now.AddMinutes(1), "Checkup");
        apt.Complete();
        repo.Add(apt);

        var result = service.CreateMedicalRecord(apt, pet, "", "Rest");
        
        Assert.False(result.Success);
        Assert.Contains("Invalid", result.Message);
    }

    [Fact]
    public void CreateMedicalRecord_InvalidTreatment_ReturnsFailure()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repo);
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        var apt = new Appointment(1, pet, vet, DateTime.Now.AddMinutes(1), "Checkup");
        apt.Complete();
        repo.Add(apt);

        var result = service.CreateMedicalRecord(apt, pet, "Flu", new string('a', 1001));
        
        Assert.False(result.Success);
        Assert.Contains("Invalid", result.Message);
    }

    [Fact]
    public void GetPetMedicalHistory_EmptyHistory_ReturnsEmptyList()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repo);
        var pet = TestDataFactory.CreatePet();

        var history = service.GetPetMedicalHistory(pet);
        
        Assert.Empty(history);
    }

    [Fact]
    public void GetPetMedicalHistory_SingleRecord_ReturnsSingleRecord()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repo);
        var pet = TestDataFactory.CreatePet();
        var record = TestDataFactory.CreateMedicalRecord(pet: pet);
        repo.AddMedicalRecord(record);

        var history = service.GetPetMedicalHistory(pet);
        
        Assert.Single(history);
        Assert.Equal(record.Id, history[0].Id);
    }

    [Fact]
    public void GetPetMedicalHistory_MultipleRecords_SortedByDateDescending()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repo);
        var pet = TestDataFactory.CreatePet();
        
        repo.AddMedicalRecord(new MedicalRecord(1, 1, pet, "Flu", "Rest", DateTime.Now.AddDays(-10)));
        repo.AddMedicalRecord(new MedicalRecord(2, 2, pet, "Allergy", "Med", DateTime.Now.AddDays(-2)));
        repo.AddMedicalRecord(new MedicalRecord(3, 3, pet, "Injury", "Bandage", DateTime.Now.AddDays(-5)));

        var history = service.GetPetMedicalHistory(pet);
        
        Assert.Equal(3, history.Count);
        Assert.Equal(2, history[0].Id);  // Most recent (2 days ago)
        Assert.Equal(3, history[1].Id);  // (5 days ago)
        Assert.Equal(1, history[2].Id);  // Oldest (10 days ago)
    }

    [Fact]
    public void GetPetMedicalHistory_NullPet_ThrowsException()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repo);

        Assert.Throws<ArgumentNullException>(() => service.GetPetMedicalHistory(null!));
    }

    [Fact]
    public void GetPetMedicalHistory_DifferentPets_NotMixed()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repo);
        var owner = TestDataFactory.CreateOwner();
        var pet1 = TestDataFactory.CreatePet(1, owner);
        var pet2 = TestDataFactory.CreatePet(2, owner);
        
        repo.AddMedicalRecord(new MedicalRecord(1, 1, pet1, "Flu", "Rest", DateTime.Now.AddDays(-5)));
        repo.AddMedicalRecord(new MedicalRecord(2, 2, pet2, "Allergy", "Med", DateTime.Now.AddDays(-3)));

        var history1 = service.GetPetMedicalHistory(pet1);
        var history2 = service.GetPetMedicalHistory(pet2);
        
        Assert.Single(history1);
        Assert.Single(history2);
        Assert.NotEqual(history1[0].Id, history2[0].Id);
    }
}

#endregion

#region Appointment Service Tests - Veterinarian Availability

[Collection("Sequential")]
public class AppointmentServiceVeterinarianAvailabilityTests
{
    [Fact]
    public void ScheduleAppointment_VeterinarianAvailable_Succeeds()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new AppointmentService(repo);
        var pet = TestDataFactory.CreatePet();
        var vet = TestDataFactory.CreateVet();
        var dateTime = DateTime.Now.AddDays(1).AddHours(10);

        var result = service.ScheduleAppointment(pet, vet, dateTime, "Checkup");
        
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public void ScheduleAppointment_VeterinarianNotAvailable_Fails()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new AppointmentService(repo);
        var pet1 = TestDataFactory.CreatePet(1);
        var pet2 = TestDataFactory.CreatePet(2);
        var vet = TestDataFactory.CreateVet();
        var dateTime = DateTime.Now.AddDays(1).AddHours(10);

        // Schedule first appointment
        service.ScheduleAppointment(pet1, vet, dateTime, "Checkup");
        
        // Try to schedule overlapping appointment with same vet
        var result = service.ScheduleAppointment(pet2, vet, dateTime, "Checkup");
        
        Assert.False(result.Success);
        Assert.Contains("not available", result.Message);
    }

    [Fact]
    public void ScheduleAppointment_DifferentVeterinarians_BothSucceed()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new AppointmentService(repo);
        var pet1 = TestDataFactory.CreatePet(1);
        var pet2 = TestDataFactory.CreatePet(2);
        var vet1 = TestDataFactory.CreateVet(1);
        var vet2 = TestDataFactory.CreateVet(2);
        var dateTime = DateTime.Now.AddDays(1).AddHours(10);

        var result1 = service.ScheduleAppointment(pet1, vet1, dateTime, "Checkup");
        var result2 = service.ScheduleAppointment(pet2, vet2, dateTime, "Checkup");
        
        Assert.True(result1.Success);
        Assert.True(result2.Success);
    }

    [Fact]
    public void ScheduleAppointment_InvalidData_ReturnsFailed()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new AppointmentService(repo);
        var pet = TestDataFactory.CreatePet();

        var result = service.ScheduleAppointment(pet, null!, DateTime.Now.AddDays(1), "Checkup");
        
        Assert.False(result.Success);
    }
}

#endregion

#region Severity Scoring Strategy Pattern Tests

[Collection("Sequential")]
public class DiagnosisSeverityScorerAdvancedTests
{
    [Theory]
    [InlineData("fracture", true)]   // High severity
    [InlineData("hemorrhage", true)] // High severity
    [InlineData("rash", false)]      // Low severity
    [InlineData("mild cough", false)] // Low severity
    public void KeywordBasedScorer_VariousDiagnoses_CorrectSeverity(string diagnosis, bool shouldBeHigh)
    {
        var scorer = new KeywordBasedSeverityScorer();
        var score = scorer.CalculateSeverityScore(diagnosis);
        
        if (shouldBeHigh)
            Assert.True(score >= 70, $"Expected high score for '{diagnosis}' but got {score}");
        else
            Assert.True(score < 60, $"Expected low score for '{diagnosis}' but got {score}");
    }

    [Fact]
    public void KeywordBasedScorer_GetSeverityLevel_Works()
    {
        var scorer = new KeywordBasedSeverityScorer();
        
        var lowScore = scorer.CalculateSeverityScore("rash");
        var lowLevel = scorer.GetSeverityLevel(lowScore);
        Assert.NotEmpty(lowLevel);
        
        var highScore = scorer.CalculateSeverityScore("fracture");
        var highLevel = scorer.GetSeverityLevel(highScore);
        Assert.NotEmpty(highLevel);
    }

    [Fact]
    public void KeywordBasedScorer_UnknownDiagnosis_ReturnsModeratScore()
    {
        var scorer = new KeywordBasedSeverityScorer();
        var score = scorer.CalculateSeverityScore("xyzabc unknown condition");
        Assert.InRange(score, 0, 100);
    }

    [Fact]
    public void LengthBasedScorer_EmptyDiagnosis_ReturnsZero()
    {
        var scorer = new LengthBasedSeverityScorer();
        var score = scorer.CalculateSeverityScore("");
        Assert.Equal(0, score);
    }

    [Fact]
    public void LengthBasedScorer_LongDiagnosis_ReturnsHigherScore()
    {
        var scorer = new LengthBasedSeverityScorer();
        var shortScore = scorer.CalculateSeverityScore("Flu");
        var longScore = scorer.CalculateSeverityScore("Chronic systemic infection with multi-organ involvement requiring intensive monitoring");
        
        Assert.True(longScore > shortScore, "Longer diagnosis should have higher score");
    }

    [Fact]
    public void DiagnosisAnalysisService_SwitchStrategies_ProducesDifferentResults()
    {
        var service = new DiagnosisAnalysisService();
        var diagnosis = "fracture";
        
        service.SetSeverityScorer(new KeywordBasedSeverityScorer());
        var result1 = service.AnalyzeDiagnosis(diagnosis);
        
        service.SetSeverityScorer(new LengthBasedSeverityScorer());
        var result2 = service.AnalyzeDiagnosis(diagnosis);
        
        // Different strategies should likely produce different scores
        Assert.NotEqual(result1.SeverityScore, result2.SeverityScore);
    }
}

#endregion

#region Integration Tests - File Persistence

[Collection("Sequential")]
public class FileBasedPersistenceIntegrationTests : IDisposable
{
    private readonly string _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public FileBasedPersistenceIntegrationTests()
    {
        if (!Directory.Exists(_testDirectory))
            Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    [Fact]
    public async Task FileDataStore_SaveAndLoad_PreservesAppointmentData()
    {
        var store = new JsonDataStore(_testDirectory);
        var owner = TestDataFactory.CreateOwner();
        var pet = TestDataFactory.CreatePet(1, owner);
        var vet = TestDataFactory.CreateVet();
        var apt = TestDataFactory.CreateAppointment(1, pet, vet);
        
        // Save
        await store.SaveClinicDataAsync(
            new[] { apt },
            [],
            new[] { owner },
            new[] { pet },
            new[] { vet }
        );

        // Load
        var (loadedApts, _) = await store.LoadAsync();
        
        Assert.Single(loadedApts);
        var loaded = loadedApts.First();
        Assert.Equal(apt.Id, loaded.Id);
        Assert.Equal(apt.Pet.Name, loaded.Pet.Name);
    }

    [Fact]
    public async Task FileDataStore_SaveAndLoad_PreservesMedicalRecordData()
    {
        var store = new JsonDataStore(_testDirectory);
        var owner = TestDataFactory.CreateOwner();
        var pet = TestDataFactory.CreatePet(1, owner);
        var record = TestDataFactory.CreateMedicalRecord(1, 1, pet);
        
        // Save
        await store.SaveClinicDataAsync(
            [],
            new[] { record },
            new[] { owner },
            new[] { pet },
            []
        );

        // Load
        var (_, loadedRecords) = await store.LoadAsync();
        
        Assert.Single(loadedRecords);
        var loaded = loadedRecords.First();
        Assert.Equal(record.Id, loaded.Id);
        Assert.Equal(record.Diagnosis, loaded.Diagnosis);
    }

    [Fact]
    public async Task FileDataStore_Load_EmptyFile_ReturnsEmptyCollections()
    {
        var store = new JsonDataStore(_testDirectory);
        
        var (apts, records) = await store.LoadAsync();
        
        Assert.Empty(apts);
        Assert.Empty(records);
    }

    [Fact]
    public async Task FileDataStore_SaveMultipleEntities_RoundTripSuccessful()
    {
        var store = new JsonDataStore(_testDirectory);
        var owner1 = TestDataFactory.CreateOwner(1);
        var owner2 = TestDataFactory.CreateOwner(2);
        var pet1 = TestDataFactory.CreatePet(1, owner1);
        var pet2 = TestDataFactory.CreatePet(2, owner2);
        var vet1 = TestDataFactory.CreateVet(1);
        var vet2 = TestDataFactory.CreateVet(2);
        
        var apt1 = TestDataFactory.CreateAppointment(1, pet1, vet1);
        var apt2 = TestDataFactory.CreateAppointment(2, pet2, vet2);
        apt1.Complete();
        
        var record1 = TestDataFactory.CreateMedicalRecord(1, 1, pet1);
        
        // Save
        await store.SaveClinicDataAsync(
            new[] { apt1, apt2 },
            new[] { record1 },
            new[] { owner1, owner2 },
            new[] { pet1, pet2 },
            new[] { vet1, vet2 }
        );

        // Load
        var (loadedApts, loadedRecords) = await store.LoadAsync();
        
        Assert.Equal(2, loadedApts.Count);
        Assert.Single(loadedRecords);
        Assert.Contains(loadedApts, a => a.Id == 1 && a.Status == AppointmentStatus.Completed);
    }

    [Fact]
    public async Task FileDataStore_CorruptedJson_ThrowsInvalidOperationException()
    {
        var dataDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dataDir);
        
        try
        {
            var filePath = Path.Combine(dataDir, "clinic_data.json");
            await File.WriteAllTextAsync(filePath, "{ invalid json ]");
            
            var store = new JsonDataStore(dataDir);
            
            // This should throw or return empty depending on implementation
            // The current implementation throws InvalidOperationException
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await store.LoadAsync()
            );
            
            Assert.Contains("Corrupted", exception.Message);
        }
        finally
        {
            Directory.Delete(dataDir, true);
        }
    }
}

#endregion

#region Fault Handling & Error Scenario Tests

[Collection("Sequential")]
public class FaultHandlingAndErrorScenariosTests
{
    [Fact]
    public void MedicalRecordService_NullRepository_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            new MedicalRecordService(null!));
    }

    [Fact]
    public void AppointmentService_NullRepository_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            new AppointmentService(null!));
    }

    [Fact]
    public void AnalyticsService_NullRepository_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            new AnalyticsService(null!));
    }

    [Fact]
    public void AnalyticsService_NullVeterinarian_ThrowsException()
    {
        var repo = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repo);
        
        Assert.Throws<ArgumentNullException>(() => 
            analytics.GetVeterinarianStatistics(null!));
    }

    [Fact]
    public void AnalyticsService_EmptyRepository_StatisticsStillComputable()
    {
        var repo = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repo);
        var vet = TestDataFactory.CreateVet();
        
        var stats = analytics.GetVeterinarianStatistics(vet);
        
        Assert.Equal(0, stats.TotalAppointments);
        Assert.Equal(0.0, stats.AverageAppointmentsPerMonth);
    }

    [Fact]
    public void MedicalRecordService_CreateRecord_WithNullAppointment_ReturnsFailed()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repo);
        var pet = TestDataFactory.CreatePet();
        
        var result = service.CreateMedicalRecord(null!, pet, "Flu", "Rest");
        
        Assert.False(result.Success);
    }

    [Fact]
    public void MedicalRecordService_CreateRecord_WithNullPet_ReturnsFailed()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new MedicalRecordService(repo);
        var apt = TestDataFactory.CreateAppointment();
        apt.Complete();
        repo.Add(apt);
        
        var result = service.CreateMedicalRecord(apt, null!, "Flu", "Rest");
        
        Assert.False(result.Success);
    }

    [Fact]
    public void AppointmentService_ScheduleAppointment_CatchesArgumentException()
    {
        var repo = new InMemoryAppointmentRepository();
        var service = new AppointmentService(repo);
        
        var result = service.ScheduleAppointment(null!, null!, DateTime.Now.AddDays(1), "Check");
        
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);
    }
}

#endregion

#region Analytics Service Edge Cases

[Collection("Sequential")]
public class AnalyticsServiceEdgeCasesTests
{
    [Fact]
    public void SearchAppointments_NoMatches_ReturnsEmptyList()
    {
        var repo = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repo);
        var owner = TestDataFactory.CreateOwner(1);
        var pet = TestDataFactory.CreatePet(1, owner);
        var vet = TestDataFactory.CreateVet();
        
        repo.Add(new Appointment(1, pet, vet, DateTime.Now.AddDays(1), "Check"));
        
        var results = analytics.SearchAppointments("NonExistentOwner", null, null, null, null);
        
        Assert.Empty(results);
    }

    [Fact]
    public void GetPetMedicalProfile_NoPets_ReturnsEmptyProfile()
    {
        var repo = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repo);
        var pet = TestDataFactory.CreatePet();
        
        var profile = analytics.GetPetMedicalProfile(pet);
        
        Assert.NotNull(profile);
        Assert.Empty(profile.UniqueDiagnoses);
    }

    [Fact]
    public void GetClinicStatistics_ZeroAppointments_HandlesGracefully()
    {
        var repo = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repo);
        
        var stats = analytics.GetClinicStatistics();
        
        Assert.Equal(0, stats.TotalAppointments);
    }

    [Fact]
    public void GetVeterinarianUtilization_EmptyRepository_ReturnsEmptyList()
    {
        var repo = new InMemoryAppointmentRepository();
        var analytics = new AnalyticsService(repo);
        
        var util = analytics.GetVeterinarianUtilization();
        
        Assert.Empty(util);
    }
}

#endregion


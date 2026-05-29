using VetClinic.Console;
using VetClinic.Application;
using VetClinic.Application.Strategies;
using VetClinic.Infrastructure;
using VetClinic.Infrastructure.Persistence;
using VetClinic.Domain;

// Create demo data
var owners = DemoDataFactory.CreateSampleOwners();
var pets = DemoDataFactory.CreateSamplePets(owners);
var veterinarians = DemoDataFactory.CreateSampleVeterinarians();

// Lab 35: Setup file-based persistence
var dataStore = new JsonDataStore();
IAppointmentRepository repository = new FileBasedAppointmentRepository(dataStore);

// Initialize repository (loads data from file if exists)
try
{
    if (repository is FileBasedAppointmentRepository fileRepo)
    {
        await fileRepo.InitializeAsync();
    }
}
catch (Exception ex)
{
    System.Console.WriteLine($"  Warning: Could not load persisted data: {ex.Message}");
    System.Console.WriteLine("   Starting with empty repository.\n");
}

// Setup services with dependency injection
var appointmentService = new AppointmentService(repository);
var medicalRecordService = new MedicalRecordService(repository);
var analyticsService = new AnalyticsService(repository);
var diagnosisAnalyzer = new DiagnosisAnalysisService(new KeywordBasedSeverityScorer());

// Run the application
var clinic = new ClinicApp(
    appointmentService, 
    medicalRecordService, 
    analyticsService, 
    diagnosisAnalyzer,
    repository,
    owners, 
    pets, 
    veterinarians);

await clinic.RunAsync();



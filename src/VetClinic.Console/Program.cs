using VetClinic.Console;
using VetClinic.Application;
using VetClinic.Infrastructure;
using VetClinic.Domain;

// Create demo data
var owners = DemoDataFactory.CreateSampleOwners();
var pets = DemoDataFactory.CreateSamplePets(owners);
var veterinarians = DemoDataFactory.CreateSampleVeterinarians();

// Setup dependency injection manually (for Iteration 1, no DI container)
IAppointmentRepository repository = new InMemoryAppointmentRepository();
var appointmentService = new AppointmentService(repository);

// Run the application
var clinic = new ClinicApp(appointmentService, owners, pets, veterinarians);
clinic.Run();

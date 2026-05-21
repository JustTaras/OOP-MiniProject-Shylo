namespace VetClinic.Console;

using VetClinic.Domain;
using VetClinic.Application;
using VetClinic.Application.Strategies;
using VetClinic.Infrastructure.Persistence;

/// <summary>
/// Main console application for VetClinic (Lab 35 Extended)
/// Demonstrates multiple use cases: appointment scheduling, medical records, analytics
/// </summary>
public class ClinicApp
{
    private readonly AppointmentService _appointmentService;
    private readonly MedicalRecordService _medicalRecordService;
    private readonly AnalyticsService _analyticsService;
    private readonly DiagnosisAnalysisService _diagnosisAnalyzer;
    private readonly IAppointmentRepository _repository;
    private readonly List<Owner> _owners;
    private readonly List<Pet> _pets;
    private readonly List<Veterinarian> _veterinarians;

    public ClinicApp(
        AppointmentService appointmentService,
        MedicalRecordService medicalRecordService,
        AnalyticsService analyticsService,
        DiagnosisAnalysisService diagnosisAnalyzer,
        IAppointmentRepository repository,
        List<Owner> owners,
        List<Pet> pets,
        List<Veterinarian> veterinarians)
    {
        _appointmentService = appointmentService;
        _medicalRecordService = medicalRecordService;
        _analyticsService = analyticsService;
        _diagnosisAnalyzer = diagnosisAnalyzer;
        _repository = repository;
        _owners = owners;
        _pets = pets;
        _veterinarians = veterinarians;
    }

    public async Task RunAsync()
    {
        bool running = true;
        while (running)
        {
            DisplayMainMenu();
            string? choice = System.Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ScheduleAppointment();
                    break;
                case "2":
                    ViewAllAppointments();
                    break;
                case "3":
                    ViewPetAppointments();
                    break;
                case "4":
                    ViewVeterinarianAppointments();
                    break;
                case "5":
                    CompleteAppointmentAndRecord();
                    break;
                case "6":
                    ViewPetMedicalHistory();
                    break;
                case "7":
                    ViewAnalytics();
                    break;
                case "8":
                    await PersistDataAsync();
                    break;
                case "9":
                    running = false;
                    System.Console.WriteLine("\n✅ Saving data before exit...");
                    if (_repository is FileBasedAppointmentRepository fileRepo)
                    {
                        try
                        {
                            await fileRepo.PersistAsync(_owners, _pets, _veterinarians);
                            System.Console.WriteLine("✅ Data saved successfully!");
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine($"❌ Error saving data: {ex.Message}");
                        }
                    }
                    System.Console.WriteLine("\nThank you for using VetClinic! Goodbye!\n");
                    break;
                default:
                    System.Console.WriteLine("\n❌ Invalid choice. Please try again.\n");
                    break;
            }
        }
    }

    private void DisplayMainMenu()
    {
        System.Console.WriteLine("\n" + new string('=', 60));
        System.Console.WriteLine("      🏥 VET CLINIC MANAGEMENT SYSTEM - LAB 35 🏥");
        System.Console.WriteLine(new string('=', 60));
        System.Console.WriteLine("📅 APPOINTMENTS:");
        System.Console.WriteLine("  1. Schedule New Appointment");
        System.Console.WriteLine("  2. View All Appointments");
        System.Console.WriteLine("  3. View Pet Appointments");
        System.Console.WriteLine("  4. View Veterinarian Appointments");
        System.Console.WriteLine("\n💊 MEDICAL RECORDS (Lab 35):");
        System.Console.WriteLine("  5. Complete Appointment & Create Medical Record");
        System.Console.WriteLine("  6. View Pet Medical History");
        System.Console.WriteLine("\n📊 ANALYTICS & REPORTS (Lab 35):");
        System.Console.WriteLine("  7. View Analytics & Statistics");
        System.Console.WriteLine("\n💾 DATA MANAGEMENT (Lab 35):");
        System.Console.WriteLine("  8. Save Data to File");
        System.Console.WriteLine("  9. Exit (Save & Close)");
        System.Console.Write("\nChoose option (1-9): ");
    }

    // =============== APPOINTMENT OPERATIONS ===============

    private void ScheduleAppointment()
    {
        System.Console.WriteLine("\n" + new string('-', 60));
        System.Console.WriteLine("📅 SCHEDULE NEW APPOINTMENT");
        System.Console.WriteLine(new string('-', 60));

        try
        {
            System.Console.WriteLine("\n🐾 Available Pets:");
            for (int i = 0; i < _pets.Count; i++)
            {
                System.Console.WriteLine($"  {i + 1}. {_pets[i].Name} ({_pets[i].Species}) - Owner: {_pets[i].Owner.Name}");
            }

            System.Console.Write("Select pet number: ");
            if (!int.TryParse(System.Console.ReadLine(), out int petChoice) || petChoice < 1 || petChoice > _pets.Count)
            {
                System.Console.WriteLine("❌ Invalid pet selection.");
                return;
            }
            var selectedPet = _pets[petChoice - 1];

            System.Console.WriteLine("\n👨‍⚕️ Available Veterinarians:");
            for (int i = 0; i < _veterinarians.Count; i++)
            {
                System.Console.WriteLine($"  {i + 1}. Dr. {_veterinarians[i].Name} ({_veterinarians[i].Specialization})");
            }

            System.Console.Write("Select veterinarian number: ");
            if (!int.TryParse(System.Console.ReadLine(), out int vetChoice) || vetChoice < 1 || vetChoice > _veterinarians.Count)
            {
                System.Console.WriteLine("❌ Invalid veterinarian selection.");
                return;
            }
            var selectedVet = _veterinarians[vetChoice - 1];

            System.Console.Write("Enter appointment date (yyyy-MM-dd): ");
            if (!DateTime.TryParse(System.Console.ReadLine(), out DateTime appointmentDate))
            {
                System.Console.WriteLine("❌ Invalid date format.");
                return;
            }

            System.Console.Write("Enter appointment time (HH:mm): ");
            if (!TimeSpan.TryParse(System.Console.ReadLine(), out TimeSpan appointmentTime))
            {
                System.Console.WriteLine("❌ Invalid time format.");
                return;
            }

            var appointmentDateTime = appointmentDate.Add(appointmentTime);

            System.Console.Write("Enter reason for appointment: ");
            string? reason = System.Console.ReadLine();

            if (string.IsNullOrWhiteSpace(reason))
            {
                System.Console.WriteLine("❌ Reason cannot be empty.");
                return;
            }

            var result = _appointmentService.ScheduleAppointment(selectedPet, selectedVet, appointmentDateTime, reason);

            if (result.Success)
            {
                System.Console.WriteLine($"\n✅ {result.Message}");
                System.Console.WriteLine($"   Appointment Details:");
                System.Console.WriteLine($"   • ID: {result.Data?.Id}");
                System.Console.WriteLine($"   • Pet: {selectedPet.Name}");
                System.Console.WriteLine($"   • Owner: {selectedPet.Owner.Name}");
                System.Console.WriteLine($"   • Veterinarian: Dr. {selectedVet.Name}");
                System.Console.WriteLine($"   • DateTime: {appointmentDateTime:yyyy-MM-dd HH:mm}");
                System.Console.WriteLine($"   • Reason: {reason}");
            }
            else
            {
                System.Console.WriteLine($"\n❌ Error: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\n❌ Unexpected error: {ex.Message}");
        }
    }

    private void ViewAllAppointments()
    {
        System.Console.WriteLine("\n" + new string('-', 60));
        System.Console.WriteLine("📋 ALL APPOINTMENTS");
        System.Console.WriteLine(new string('-', 60));

        var appointments = _repository.GetAll();

        if (appointments.Count > 0)
        {
            foreach (var appointment in appointments)
            {
                DisplayAppointmentDetails(appointment);
            }
            System.Console.WriteLine($"Total appointments: {appointments.Count}");
        }
        else
        {
            System.Console.WriteLine("ℹ️ No appointments scheduled yet.");
        }
    }

    private void ViewPetAppointments()
    {
        System.Console.WriteLine("\n" + new string('-', 60));
        System.Console.WriteLine("🐾 VIEW PET APPOINTMENTS");
        System.Console.WriteLine(new string('-', 60));

        System.Console.WriteLine("\nAvailable Pets:");
        for (int i = 0; i < _pets.Count; i++)
        {
            System.Console.WriteLine($"  {i + 1}. {_pets[i].Name}");
        }

        System.Console.Write("Select pet number: ");
        if (!int.TryParse(System.Console.ReadLine(), out int petChoice) || petChoice < 1 || petChoice > _pets.Count)
        {
            System.Console.WriteLine("❌ Invalid pet selection.");
            return;
        }

        var selectedPet = _pets[petChoice - 1];
        var appointments = _repository.GetByPet(selectedPet);

        if (appointments.Count > 0)
        {
            System.Console.WriteLine($"\n📅 Appointments for {selectedPet.Name}:");
            foreach (var appointment in appointments)
            {
                DisplayAppointmentDetails(appointment);
            }
        }
        else
        {
            System.Console.WriteLine($"\nℹ️ No appointments scheduled for {selectedPet.Name}.");
        }
    }

    private void ViewVeterinarianAppointments()
    {
        System.Console.WriteLine("\n" + new string('-', 60));
        System.Console.WriteLine("👨‍⚕️ VIEW VETERINARIAN APPOINTMENTS");
        System.Console.WriteLine(new string('-', 60));

        System.Console.WriteLine("\nAvailable Veterinarians:");
        for (int i = 0; i < _veterinarians.Count; i++)
        {
            System.Console.WriteLine($"  {i + 1}. Dr. {_veterinarians[i].Name} ({_veterinarians[i].Specialization})");
        }

        System.Console.Write("Select veterinarian number: ");
        if (!int.TryParse(System.Console.ReadLine(), out int vetChoice) || vetChoice < 1 || vetChoice > _veterinarians.Count)
        {
            System.Console.WriteLine("❌ Invalid veterinarian selection.");
            return;
        }

        var selectedVet = _veterinarians[vetChoice - 1];
        var appointments = _repository.GetByVeterinarian(selectedVet);

        if (appointments.Count > 0)
        {
            System.Console.WriteLine($"\n📅 Appointments for Dr. {selectedVet.Name}:");
            foreach (var appointment in appointments)
            {
                DisplayAppointmentDetails(appointment);
            }
        }
        else
        {
            System.Console.WriteLine($"\nℹ️ No appointments scheduled for Dr. {selectedVet.Name}.");
        }
    }

    // =============== MEDICAL RECORD OPERATIONS (Lab 35) ===============

    private void CompleteAppointmentAndRecord()
    {
        System.Console.WriteLine("\n" + new string('-', 60));
        System.Console.WriteLine("💊 COMPLETE APPOINTMENT & CREATE MEDICAL RECORD");
        System.Console.WriteLine(new string('-', 60));

        try
        {
            var scheduledAppointments = _repository.GetAll()
                .Where(a => a.Status == AppointmentStatus.Scheduled)
                .ToList();

            if (scheduledAppointments.Count == 0)
            {
                System.Console.WriteLine("\nℹ️ No scheduled appointments to complete.");
                return;
            }

            System.Console.WriteLine("\n📅 Scheduled Appointments:");
            for (int i = 0; i < scheduledAppointments.Count; i++)
            {
                var apt = scheduledAppointments[i];
                System.Console.WriteLine($"  {i + 1}. {apt.Pet.Name} with Dr. {apt.Veterinarian.Name} - {apt.AppointmentDateTime:yyyy-MM-dd HH:mm}");
            }

            System.Console.Write("Select appointment to complete: ");
            if (!int.TryParse(System.Console.ReadLine(), out int aptChoice) || aptChoice < 1 || aptChoice > scheduledAppointments.Count)
            {
                System.Console.WriteLine("❌ Invalid appointment selection.");
                return;
            }

            var appointment = scheduledAppointments[aptChoice - 1];

            // Complete appointment
            appointment.Complete();
            _repository.Update(appointment);

            System.Console.WriteLine($"\n✅ Appointment marked as completed for {appointment.Pet.Name}");

            // Create medical record
            System.Console.Write("Enter diagnosis (max 500 chars): ");
            string? diagnosis = System.Console.ReadLine();

            System.Console.Write("Enter treatment details (max 1000 chars): ");
            string? treatment = System.Console.ReadLine();

            var recordResult = _medicalRecordService.CreateMedicalRecord(appointment, appointment.Pet, diagnosis ?? "", treatment ?? "");

            if (recordResult.Success)
            {
                System.Console.WriteLine($"\n✅ Medical record created successfully!");
                System.Console.WriteLine($"   Record ID: {recordResult.Data?.Id}");
                System.Console.WriteLine($"   Pet: {appointment.Pet.Name}");
                System.Console.WriteLine($"   Diagnosis: {diagnosis}");
                System.Console.WriteLine($"   Treatment: {treatment}");

                // Analyze diagnosis severity
                var analysis = _diagnosisAnalyzer.AnalyzeDiagnosis(diagnosis ?? "");
                System.Console.WriteLine($"\n📊 Diagnosis Analysis:");
                System.Console.WriteLine($"   Severity: {analysis.SeverityLevel} (Score: {analysis.SeverityScore}/100)");
                System.Console.WriteLine($"   Scoring: {analysis.ScoringStrategy}");
            }
            else
            {
                System.Console.WriteLine($"\n❌ Error creating medical record: {recordResult.Message}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\n❌ Unexpected error: {ex.Message}");
        }
    }

    private void ViewPetMedicalHistory()
    {
        System.Console.WriteLine("\n" + new string('-', 60));
        System.Console.WriteLine("📋 PET MEDICAL HISTORY");
        System.Console.WriteLine(new string('-', 60));

        try
        {
            System.Console.WriteLine("\n🐾 Available Pets:");
            for (int i = 0; i < _pets.Count; i++)
            {
                System.Console.WriteLine($"  {i + 1}. {_pets[i].Name}");
            }

            System.Console.Write("Select pet: ");
            if (!int.TryParse(System.Console.ReadLine(), out int petChoice) || petChoice < 1 || petChoice > _pets.Count)
            {
                System.Console.WriteLine("❌ Invalid pet selection.");
                return;
            }

            var pet = _pets[petChoice - 1];
            var profile = _analyticsService.GetPetMedicalProfile(pet);

            System.Console.WriteLine($"\n🏥 Medical Profile for {pet.Name}");
            System.Console.WriteLine($"   Species: {pet.Species}");
            System.Console.WriteLine($"   Owner: {pet.Owner.Name}");
            System.Console.WriteLine($"   Total Appointments: {profile.TotalAppointments}");
            System.Console.WriteLine($"   Completed Appointments: {profile.CompletedAppointments}");
            System.Console.WriteLine($"   Last Visit: {(profile.LastVisitDate == DateTime.MinValue ? "Never" : profile.LastVisitDate.ToString("yyyy-MM-dd"))}");

            if (profile.UniqueDiagnoses.Count > 0)
            {
                System.Console.WriteLine($"\n📋 Diagnoses History:");
                foreach (var diagnosis in profile.UniqueDiagnoses)
                {
                    System.Console.WriteLine($"   • {diagnosis}");
                }
            }

            if (profile.MedicalRecords.Count > 0)
            {
                System.Console.WriteLine($"\n📂 Medical Records:");
                foreach (var record in profile.MedicalRecords)
                {
                    System.Console.WriteLine($"\n   Record ID: {record.Id}");
                    System.Console.WriteLine($"   Visit Date: {record.VisitDate:yyyy-MM-dd}");
                    System.Console.WriteLine($"   Diagnosis: {record.Diagnosis}");
                    System.Console.WriteLine($"   Treatment: {record.Treatment}");
                }
            }
            else
            {
                System.Console.WriteLine($"\nℹ️ No medical records found for {pet.Name}.");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\n❌ Error: {ex.Message}");
        }
    }

    // =============== ANALYTICS OPERATIONS (Lab 35) ===============

    private void ViewAnalytics()
    {
        System.Console.WriteLine("\n" + new string('-', 60));
        System.Console.WriteLine("📊 CLINIC ANALYTICS & STATISTICS");
        System.Console.WriteLine(new string('-', 60));

        System.Console.WriteLine("\n1. Clinic Overview");
        System.Console.WriteLine("2. Veterinarian Statistics");
        System.Console.WriteLine("3. Search Appointments");
        System.Console.WriteLine("4. Veterinarian Utilization");

        System.Console.Write("Select report (1-4): ");
        string? choice = System.Console.ReadLine();

        switch (choice)
        {
            case "1":
                ShowClinicOverview();
                break;
            case "2":
                ShowVeterinarianStats();
                break;
            case "3":
                SearchAppointments();
                break;
            case "4":
                ShowVeterinarianUtilization();
                break;
            default:
                System.Console.WriteLine("❌ Invalid choice.");
                break;
        }
    }

    private void ShowClinicOverview()
    {
        System.Console.WriteLine("\n" + new string('-', 60));
        System.Console.WriteLine("📊 CLINIC OVERVIEW");
        System.Console.WriteLine(new string('-', 60));

        var stats = _analyticsService.GetClinicStatistics();

        System.Console.WriteLine($"\n📈 SUMMARY:");
        System.Console.WriteLine($"   Total Appointments: {stats.TotalAppointments}");
        System.Console.WriteLine($"   Completed: {stats.CompletedAppointments}");
        System.Console.WriteLine($"   Cancelled: {stats.CancelledAppointments}");
        System.Console.WriteLine($"   Scheduled: {stats.ScheduledAppointments}");
        System.Console.WriteLine($"   Completion Rate: {stats.CompletionRate}%");
        System.Console.WriteLine($"   Cancellation Rate: {stats.CancellationRate}%");

        if (stats.MostActiveVeterinarians.Count > 0)
        {
            System.Console.WriteLine($"\n👨‍⚕️ MOST ACTIVE VETERINARIANS:");
            foreach (dynamic vet in stats.MostActiveVeterinarians.Cast<dynamic>().Take(5))
            {
                System.Console.WriteLine($"   • {vet.Vet.Name}: {vet.Count} appointments");
            }
        }

        if (stats.MostVisitedPets.Count > 0)
        {
            System.Console.WriteLine($"\n🐾 MOST VISITED PETS:");
            foreach (dynamic pet in stats.MostVisitedPets.Cast<dynamic>().Take(5))
            {
                System.Console.WriteLine($"   • {pet.Pet.Name}: {pet.Count} appointments");
            }
        }
    }

    private void ShowVeterinarianStats()
    {
        System.Console.WriteLine("\n" + new string('-', 60));
        System.Console.WriteLine("👨‍⚕️ VETERINARIAN STATISTICS");
        System.Console.WriteLine(new string('-', 60));

        System.Console.WriteLine("\nAvailable Veterinarians:");
        for (int i = 0; i < _veterinarians.Count; i++)
        {
            System.Console.WriteLine($"  {i + 1}. Dr. {_veterinarians[i].Name}");
        }

        System.Console.Write("Select veterinarian: ");
        if (!int.TryParse(System.Console.ReadLine(), out int vetChoice) || vetChoice < 1 || vetChoice > _veterinarians.Count)
        {
            System.Console.WriteLine("❌ Invalid selection.");
            return;
        }

        var vet = _veterinarians[vetChoice - 1];
        var stats = _analyticsService.GetVeterinarianStatistics(vet);

        System.Console.WriteLine($"\n📊 Stats for Dr. {vet.Name}:");
        System.Console.WriteLine($"   Total Appointments: {stats.TotalAppointments}");
        System.Console.WriteLine($"   Completed: {stats.CompletedAppointments}");
        System.Console.WriteLine($"   Cancelled: {stats.CancelledAppointments}");
        System.Console.WriteLine($"   Upcoming: {stats.UpcomingAppointments}");
        System.Console.WriteLine($"   Avg Per Month: {stats.AverageAppointmentsPerMonth}");

        if (stats.MostCommonDiagnoses.Count > 0)
        {
            System.Console.WriteLine($"\n💊 Most Common Diagnoses:");
            foreach (dynamic d in stats.MostCommonDiagnoses.Cast<dynamic>())
            {
                System.Console.WriteLine($"   • {d.Diagnosis}: {d.Count} cases");
            }
        }
    }

    private void SearchAppointments()
    {
        System.Console.WriteLine("\n" + new string('-', 60));
        System.Console.WriteLine("🔍 SEARCH APPOINTMENTS");
        System.Console.WriteLine(new string('-', 60));

        System.Console.Write("Owner name (or press Enter to skip): ");
        string? ownerName = System.Console.ReadLine();

        System.Console.Write("Pet name (or press Enter to skip): ");
        string? petName = System.Console.ReadLine();

        System.Console.Write("Status (S=Scheduled, C=Completed, A=Cancelled, or press Enter for all): ");
        string? statusInput = System.Console.ReadLine();
        AppointmentStatus? status = statusInput?.ToUpper() switch
        {
            "S" => AppointmentStatus.Scheduled,
            "C" => AppointmentStatus.Completed,
            "A" => AppointmentStatus.Cancelled,
            _ => null
        };

        var results = _analyticsService.SearchAppointments(ownerName, petName, status);

        if (results.Count > 0)
        {
            System.Console.WriteLine($"\n📋 Found {results.Count} matching appointments:");
            foreach (var apt in results)
            {
                DisplayAppointmentDetails(apt);
            }
        }
        else
        {
            System.Console.WriteLine("\nℹ️ No appointments found matching your criteria.");
        }
    }

    private void ShowVeterinarianUtilization()
    {
        System.Console.WriteLine("\n" + new string('-', 60));
        System.Console.WriteLine("📊 VETERINARIAN UTILIZATION");
        System.Console.WriteLine(new string('-', 60));

        var utilization = _analyticsService.GetVeterinarianUtilization();

        System.Console.WriteLine($"\n{new string('-', 60)}");
        System.Console.WriteLine($"{"Veterinarian",-25} {"Total",-10} {"Completed",-12} {"Utilization",-13}");
        System.Console.WriteLine(new string('-', 60));

        foreach (var vet in utilization)
        {
            System.Console.WriteLine($"{vet.Veterinarian.Name,-25} {vet.TotalAppointments,-10} {vet.CompletedAppointments,-12} {vet.UtilizationRate:F2}%");
        }
    }

    // =============== DATA PERSISTENCE (Lab 35) ===============

    private async Task PersistDataAsync()
    {
        System.Console.WriteLine("\n" + new string('-', 60));
        System.Console.WriteLine("💾 SAVE DATA TO FILE");
        System.Console.WriteLine(new string('-', 60));

        try
        {
            if (_repository is not FileBasedAppointmentRepository fileRepo)
            {
                System.Console.WriteLine("❌ Repository does not support persistence.");
                return;
            }

            await fileRepo.PersistAsync(_owners, _pets, _veterinarians);
            System.Console.WriteLine("\n✅ Data saved successfully to JSON file!");
            System.Console.WriteLine("   Location: %APPDATA%\\VetClinic\\clinic_data.json");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\n❌ Error saving data: {ex.Message}");
        }
    }

    // =============== HELPER METHODS ===============

    private void DisplayAppointmentDetails(Appointment appointment)
    {
        System.Console.WriteLine($"\n  ID: {appointment.Id}");
        System.Console.WriteLine($"  Pet: {appointment.Pet.Name} ({appointment.Pet.Species})");
        System.Console.WriteLine($"  Owner: {appointment.Pet.Owner.Name}");
        System.Console.WriteLine($"  Veterinarian: Dr. {appointment.Veterinarian.Name} ({appointment.Veterinarian.Specialization})");
        System.Console.WriteLine($"  DateTime: {appointment.AppointmentDateTime:yyyy-MM-dd HH:mm}");
        System.Console.WriteLine($"  Reason: {appointment.Reason}");
        System.Console.WriteLine($"  Status: {appointment.Status}");
    }
}

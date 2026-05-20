namespace VetClinic.Console;

using VetClinic.Domain;
using VetClinic.Application;

/// <summary>
/// Main console application for VetClinic
/// Demonstrates the vertical slice: User Input → Service → Domain Rules → Repository → Output
/// </summary>
public class ClinicApp
{
    private readonly AppointmentService _appointmentService;
    private readonly List<Owner> _owners;
    private readonly List<Pet> _pets;
    private readonly List<Veterinarian> _veterinarians;

    public ClinicApp(AppointmentService appointmentService, List<Owner> owners, 
                    List<Pet> pets, List<Veterinarian> veterinarians)
    {
        _appointmentService = appointmentService;
        _owners = owners;
        _pets = pets;
        _veterinarians = veterinarians;
    }

    public void Run()
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
                    running = false;
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
        System.Console.WriteLine("\n" + new string('=', 50));
        System.Console.WriteLine("      🏥 VET CLINIC MANAGEMENT SYSTEM 🏥");
        System.Console.WriteLine(new string('=', 50));
        System.Console.WriteLine("1. Schedule Appointment");
        System.Console.WriteLine("2. View All Appointments");
        System.Console.WriteLine("3. View Pet Appointments");
        System.Console.WriteLine("4. View Veterinarian Appointments");
        System.Console.WriteLine("5. Exit");
        System.Console.Write("Choose option (1-5): ");
    }

    private void ScheduleAppointment()
    {
        System.Console.WriteLine("\n" + new string('-', 50));
        System.Console.WriteLine("📅 SCHEDULE NEW APPOINTMENT");
        System.Console.WriteLine(new string('-', 50));

        try
        {
            // Display available pets
            System.Console.WriteLine("\n🐾 Available Pets:");
            for (int i = 0; i < _pets.Count; i++)
            {
                System.Console.WriteLine($"  {i + 1}. {_pets[i].Name} ({_pets[i].Species})");
            }

            System.Console.Write("Select pet number: ");
            if (!int.TryParse(System.Console.ReadLine(), out int petChoice) || petChoice < 1 || petChoice > _pets.Count)
            {
                System.Console.WriteLine("❌ Invalid pet selection.");
                return;
            }
            var selectedPet = _pets[petChoice - 1];

            // Display available veterinarians
            System.Console.WriteLine("\n👨‍⚕️ Available Veterinarians:");
            for (int i = 0; i < _veterinarians.Count; i++)
            {
                System.Console.WriteLine($"  {i + 1}. {_veterinarians[i].Name} ({_veterinarians[i].Specialization})");
            }

            System.Console.Write("Select veterinarian number: ");
            if (!int.TryParse(System.Console.ReadLine(), out int vetChoice) || vetChoice < 1 || vetChoice > _veterinarians.Count)
            {
                System.Console.WriteLine("❌ Invalid veterinarian selection.");
                return;
            }
            var selectedVet = _veterinarians[vetChoice - 1];

            // Get appointment date and time
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

            // Get reason for appointment
            System.Console.Write("Enter reason for appointment: ");
            string? reason = System.Console.ReadLine();

            if (string.IsNullOrWhiteSpace(reason))
            {
                System.Console.WriteLine("❌ Reason cannot be empty.");
                return;
            }

            // Schedule appointment through service (includes all validations)
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
        System.Console.WriteLine("\n" + new string('-', 50));
        System.Console.WriteLine("📋 ALL APPOINTMENTS");
        System.Console.WriteLine(new string('-', 50));

        var result = _appointmentService.GetAllAppointments();

        if (result.Success && result.Data?.Count > 0)
        {
            foreach (var appointment in result.Data)
            {
                DisplayAppointmentDetails(appointment);
            }
        }
        else
        {
            System.Console.WriteLine("ℹ️ No appointments scheduled yet.");
        }
    }

    private void ViewPetAppointments()
    {
        System.Console.WriteLine("\n" + new string('-', 50));
        System.Console.WriteLine("🐾 VIEW PET APPOINTMENTS");
        System.Console.WriteLine(new string('-', 50));

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
        var result = _appointmentService.GetAppointmentsByPet(selectedPet);

        if (result.Success && result.Data?.Count > 0)
        {
            System.Console.WriteLine($"\n📅 Appointments for {selectedPet.Name}:");
            foreach (var appointment in result.Data)
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
        System.Console.WriteLine("\n" + new string('-', 50));
        System.Console.WriteLine("👨‍⚕️ VIEW VETERINARIAN APPOINTMENTS");
        System.Console.WriteLine(new string('-', 50));

        System.Console.WriteLine("\nAvailable Veterinarians:");
        for (int i = 0; i < _veterinarians.Count; i++)
        {
            System.Console.WriteLine($"  {i + 1}. {_veterinarians[i].Name} ({_veterinarians[i].Specialization})");
        }

        System.Console.Write("Select veterinarian number: ");
        if (!int.TryParse(System.Console.ReadLine(), out int vetChoice) || vetChoice < 1 || vetChoice > _veterinarians.Count)
        {
            System.Console.WriteLine("❌ Invalid veterinarian selection.");
            return;
        }

        var selectedVet = _veterinarians[vetChoice - 1];
        var result = _appointmentService.GetAppointmentsByVeterinarian(selectedVet);

        if (result.Success && result.Data?.Count > 0)
        {
            System.Console.WriteLine($"\n📅 Appointments for Dr. {selectedVet.Name}:");
            foreach (var appointment in result.Data)
            {
                DisplayAppointmentDetails(appointment);
            }
        }
        else
        {
            System.Console.WriteLine($"\nℹ️ No appointments scheduled for Dr. {selectedVet.Name}.");
        }
    }

    private void DisplayAppointmentDetails(Appointment appointment)
    {
        System.Console.WriteLine($"\n  ID: {appointment.Id}");
        System.Console.WriteLine($"  Pet: {appointment.Pet.Name} ({appointment.Pet.Species})");
        System.Console.WriteLine($"  Owner: {appointment.Pet.Owner.Name}");
        System.Console.WriteLine($"  Veterinarian: Dr. {appointment.Veterinarian.Name} ({appointment.Veterinarian.Specialization})");
        System.Console.WriteLine($"  DateTime: {appointment.AppointmentDateTime:yyyy-MM-dd HH:mm}");
        System.Console.WriteLine($"  Reason: {appointment.Reason}");
        System.Console.WriteLine($"  Status: {appointment.Status}");
        System.Console.WriteLine();
    }
}

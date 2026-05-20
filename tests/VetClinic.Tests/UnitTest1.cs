namespace VetClinic.Tests;

using Xunit;
using VetClinic.Domain;
using VetClinic.Application;
using VetClinic.Infrastructure;

/// <summary>
/// Unit tests for Owner domain model
/// Tests encapsulation, validation, and invariants
/// </summary>
public class OwnerTests
{
    [Fact]
    public void Owner_WithValidData_CreatesSuccessfully()
    {
        // Arrange & Act
        var owner = new Owner(1, "John Smith", "+1-555-0101", "john@email.com");

        // Assert
        Assert.Equal(1, owner.Id);
        Assert.Equal("John Smith", owner.Name);
        Assert.Equal("+1-555-0101", owner.PhoneNumber);
        Assert.Equal("john@email.com", owner.Email);
    }

    [Fact]
    public void Owner_WithNegativeId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Owner(-1, "John Smith", "+1-555-0101", "john@email.com")
        );
    }

    [Fact]
    public void Owner_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Owner(1, "", "+1-555-0101", "john@email.com")
        );
    }

    [Fact]
    public void Owner_WithInvalidEmail_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Owner(1, "John Smith", "+1-555-0101", "invalidemail")
        );
    }
}

/// <summary>
/// Unit tests for Pet domain model
/// Tests encapsulation, validation, and invariants
/// </summary>
public class PetTests
{
    private Owner CreateValidOwner() => new(1, "John Smith", "+1-555-0101", "john@email.com");

    [Fact]
    public void Pet_WithValidData_CreatesSuccessfully()
    {
        // Arrange
        var owner = CreateValidOwner();

        // Act
        var pet = new Pet(1, "Fluffy", Species.Cat, "Persian", 36, owner);

        // Assert
        Assert.Equal(1, pet.Id);
        Assert.Equal("Fluffy", pet.Name);
        Assert.Equal(Species.Cat, pet.Species);
        Assert.Equal("Persian", pet.Breed);
        Assert.Equal(36, pet.AgeInMonths);
        Assert.Equal(owner, pet.Owner);
    }

    [Fact]
    public void Pet_WithNegativeAge_ThrowsArgumentException()
    {
        // Arrange
        var owner = CreateValidOwner();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Pet(1, "Fluffy", Species.Cat, "Persian", -1, owner)
        );
    }

    [Fact]
    public void Pet_WithUnrealisticAge_ThrowsArgumentException()
    {
        // Arrange
        var owner = CreateValidOwner();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Pet(1, "Fluffy", Species.Cat, "Persian", 700, owner)
        );
    }

    [Fact]
    public void Pet_WithoutOwner_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Pet(1, "Fluffy", Species.Cat, "Persian", 36, null!)
        );
    }

    [Fact]
    public void Pet_UpdateAge_WithValidAge_Succeeds()
    {
        // Arrange
        var owner = CreateValidOwner();
        var pet = new Pet(1, "Fluffy", Species.Cat, "Persian", 36, owner);

        // Act
        pet.UpdateAge(48);

        // Assert
        Assert.Equal(48, pet.AgeInMonths);
    }

    [Fact]
    public void Pet_UpdateAge_WithDecreasingAge_ThrowsArgumentException()
    {
        // Arrange
        var owner = CreateValidOwner();
        var pet = new Pet(1, "Fluffy", Species.Cat, "Persian", 36, owner);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => pet.UpdateAge(24));
    }
}

/// <summary>
/// Unit tests for Appointment domain model
/// Tests business rules and invariants
/// </summary>
public class AppointmentTests
{
    private Owner CreateValidOwner() => new(1, "John Smith", "+1-555-0101", "john@email.com");
    private Pet CreateValidPet() => new(1, "Fluffy", Species.Cat, "Persian", 36, CreateValidOwner());
    private Veterinarian CreateValidVet() => new(1, "Dr. Sarah Wilson", "Small Animals", "VET-001");

    [Fact]
    public void Appointment_WithFutureDateTime_CreatesSuccessfully()
    {
        // Arrange
        var pet = CreateValidPet();
        var vet = CreateValidVet();
        var futureTime = DateTime.Now.AddDays(1);

        // Act
        var appointment = new Appointment(1, pet, vet, futureTime, "Regular checkup");

        // Assert
        Assert.Equal(1, appointment.Id);
        Assert.Equal(pet, appointment.Pet);
        Assert.Equal(vet, appointment.Veterinarian);
        Assert.Equal(futureTime, appointment.AppointmentDateTime);
        Assert.Equal("Regular checkup", appointment.Reason);
        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
    }

    [Fact]
    public void Appointment_WithPastDateTime_ThrowsArgumentException()
    {
        // Arrange
        var pet = CreateValidPet();
        var vet = CreateValidVet();
        var pastTime = DateTime.Now.AddDays(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Appointment(1, pet, vet, pastTime, "Regular checkup")
        );
    }

    [Fact]
    public void Appointment_WithoutPet_ThrowsArgumentNullException()
    {
        // Arrange
        var vet = CreateValidVet();
        var futureTime = DateTime.Now.AddDays(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Appointment(1, null!, vet, futureTime, "Regular checkup")
        );
    }

    [Fact]
    public void Appointment_Complete_WithScheduledStatus_Succeeds()
    {
        // Arrange
        var pet = CreateValidPet();
        var vet = CreateValidVet();
        var futureTime = DateTime.Now.AddDays(1);
        var appointment = new Appointment(1, pet, vet, futureTime, "Regular checkup");

        // Act
        appointment.Complete();

        // Assert
        Assert.Equal(AppointmentStatus.Completed, appointment.Status);
    }

    [Fact]
    public void Appointment_Cancel_WithScheduledStatus_Succeeds()
    {
        // Arrange
        var pet = CreateValidPet();
        var vet = CreateValidVet();
        var futureTime = DateTime.Now.AddDays(1);
        var appointment = new Appointment(1, pet, vet, futureTime, "Regular checkup");

        // Act
        appointment.Cancel();

        // Assert
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    [Fact]
    public void Appointment_Cancel_WithCompletedStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var pet = CreateValidPet();
        var vet = CreateValidVet();
        var futureTime = DateTime.Now.AddDays(1);
        var appointment = new Appointment(1, pet, vet, futureTime, "Regular checkup");
        appointment.Complete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => appointment.Cancel());
    }
}

/// <summary>
/// Unit tests for AppointmentService
/// Tests business logic, validation, and repository integration
/// </summary>
public class AppointmentServiceTests
{
    private Owner CreateValidOwner() => new(1, "John Smith", "+1-555-0101", "john@email.com");
    private Pet CreateValidPet() => new(1, "Fluffy", Species.Cat, "Persian", 36, CreateValidOwner());
    private Veterinarian CreateValidVet() => new(1, "Dr. Sarah Wilson", "Small Animals", "VET-001");

    [Fact]
    public void ScheduleAppointment_WithValidData_Succeeds()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var service = new AppointmentService(repository);
        var pet = CreateValidPet();
        var vet = CreateValidVet();
        var futureTime = DateTime.Now.AddDays(1);

        // Act
        var result = service.ScheduleAppointment(pet, vet, futureTime, "Regular checkup");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(pet, result.Data.Pet);
        Assert.Equal(vet, result.Data.Veterinarian);
    }

    [Fact]
    public void ScheduleAppointment_WithPastDateTime_Fails()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var service = new AppointmentService(repository);
        var pet = CreateValidPet();
        var vet = CreateValidVet();
        var pastTime = DateTime.Now.AddDays(-1);

        // Act
        var result = service.ScheduleAppointment(pet, vet, pastTime, "Regular checkup");

        // Assert
        Assert.False(result.Success);
        Assert.True(result.Message.Contains("future", StringComparison.OrdinalIgnoreCase) || 
                    result.Message.Contains("past", StringComparison.OrdinalIgnoreCase) ||
                    result.Message.Contains("time", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ScheduleAppointment_WithUnavailableVet_Fails()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var service = new AppointmentService(repository);
        var pet1 = CreateValidPet();
        var pet2 = new Pet(2, "Rex", Species.Dog, "German Shepherd", 60, CreateValidOwner());
        var vet = CreateValidVet();
        var appointmentTime = DateTime.Now.AddDays(1).Date.Add(new TimeSpan(14, 0, 0));

        // Schedule first appointment
        service.ScheduleAppointment(pet1, vet, appointmentTime, "Checkup");

        // Act - Try to schedule second appointment at same time with same vet
        var result = service.ScheduleAppointment(pet2, vet, appointmentTime, "Checkup");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not available", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetAllAppointments_WithMultipleAppointments_ReturnsAll()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var service = new AppointmentService(repository);
        var pet1 = CreateValidPet();
        var pet2 = new Pet(2, "Rex", Species.Dog, "German Shepherd", 60, CreateValidOwner());
        var vet1 = CreateValidVet();
        var vet2 = new Veterinarian(2, "Dr. Michael Chen", "Exotic Animals", "VET-002");

        service.ScheduleAppointment(pet1, vet1, DateTime.Now.AddDays(1), "Checkup");
        service.ScheduleAppointment(pet2, vet2, DateTime.Now.AddDays(2), "Vaccination");

        // Act
        var result = service.GetAllAppointments();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.Data?.Count);
    }

    [Fact]
    public void GetAppointmentsByPet_WithValidPet_ReturnsOnlyPetAppointments()
    {
        // Arrange
        var repository = new InMemoryAppointmentRepository();
        var service = new AppointmentService(repository);
        var pet1 = CreateValidPet();
        var pet2 = new Pet(2, "Rex", Species.Dog, "German Shepherd", 60, CreateValidOwner());
        var vet1 = CreateValidVet();
        var vet2 = new Veterinarian(2, "Dr. Michael Chen", "Exotic Animals", "VET-002");

        service.ScheduleAppointment(pet1, vet1, DateTime.Now.AddDays(1), "Checkup");
        service.ScheduleAppointment(pet2, vet2, DateTime.Now.AddDays(2), "Vaccination");
        service.ScheduleAppointment(pet1, vet2, DateTime.Now.AddDays(3), "Vaccination");

        // Act
        var result = service.GetAppointmentsByPet(pet1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.Data?.Count);
        Assert.All(result.Data!, apt => Assert.Equal(pet1.Id, apt.Pet.Id));
    }
}

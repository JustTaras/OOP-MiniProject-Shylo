namespace VetClinic.Console;

using VetClinic.Domain;
using VetClinic.Application;

/// <summary>
/// Demo data factory for testing purposes
/// </summary>
public static class DemoDataFactory
{
    private static int _ownerId = 0;
    private static int _petId = 0;
    private static int _vetId = 0;

    public static Owner CreateOwner(string name, string phone, string email)
    {
        return new Owner(++_ownerId, name, phone, email);
    }

    public static Pet CreatePet(string name, Species species, string breed, int ageInMonths, Owner owner)
    {
        return new Pet(++_petId, name, species, breed, ageInMonths, owner);
    }

    public static Veterinarian CreateVeterinarian(string name, string specialization, string license)
    {
        return new Veterinarian(++_vetId, name, specialization, license);
    }

    public static List<Owner> CreateSampleOwners()
    {
        var owners = new List<Owner>
        {
            CreateOwner("John Smith", "+1-555-0101", "john.smith@email.com"),
            CreateOwner("Mary Johnson", "+1-555-0102", "mary.johnson@email.com"),
            CreateOwner("Robert Brown", "+1-555-0103", "robert.brown@email.com")
        };
        return owners;
    }

    public static List<Pet> CreateSamplePets(List<Owner> owners)
    {
        var pets = new List<Pet>
        {
            CreatePet("Fluffy", Species.Cat, "Persian", 36, owners[0]),
            CreatePet("Rex", Species.Dog, "German Shepherd", 60, owners[1]),
            CreatePet("Tweety", Species.Bird, "Parakeet", 24, owners[2])
        };
        return pets;
    }

    public static List<Veterinarian> CreateSampleVeterinarians()
    {
        var vets = new List<Veterinarian>
        {
            CreateVeterinarian("Dr. Sarah Wilson", "Small Animals", "VET-001"),
            CreateVeterinarian("Dr. Michael Chen", "Exotic Animals", "VET-002"),
            CreateVeterinarian("Dr. Emily Davis", "Surgery", "VET-003")
        };
        return vets;
    }
}

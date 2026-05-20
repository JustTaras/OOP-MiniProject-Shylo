namespace VetClinic.Domain;

/// <summary>
/// Represents a pet in the clinic
/// </summary>
public class Pet
{
    public int Id { get; }
    public string Name { get; }
    public Species Species { get; }
    public string Breed { get; }
    public int AgeInMonths { get; private set; }
    public Owner Owner { get; }

    /// <summary>
    /// Creates a new Pet with validation
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when data is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown when owner is null</exception>
    public Pet(int id, string name, Species species, string breed, int ageInMonths, Owner owner)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be positive", nameof(id));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Pet name cannot be empty", nameof(name));
        
        if (name.Length > 50)
            throw new ArgumentException("Pet name is too long (max 50 characters)", nameof(name));
        
        if (string.IsNullOrWhiteSpace(breed))
            throw new ArgumentException("Breed cannot be empty", nameof(breed));
        
        if (breed.Length > 50)
            throw new ArgumentException("Breed is too long (max 50 characters)", nameof(breed));
        
        if (ageInMonths < 0)
            throw new ArgumentException("Age cannot be negative", nameof(ageInMonths));
        
        if (ageInMonths > 600) // ~50 years
            throw new ArgumentException("Age seems unrealistic", nameof(ageInMonths));
        
        if (owner == null)
            throw new ArgumentNullException(nameof(owner), "Pet must have an owner");

        Id = id;
        Name = name.Trim();
        Species = species;
        Breed = breed.Trim();
        AgeInMonths = ageInMonths;
        Owner = owner;
    }

    /// <summary>
    /// Update pet's age (in months)
    /// </summary>
    public void UpdateAge(int newAgeInMonths)
    {
        if (newAgeInMonths < AgeInMonths)
            throw new ArgumentException("Age cannot decrease", nameof(newAgeInMonths));
        
        if (newAgeInMonths > 600)
            throw new ArgumentException("Age seems unrealistic", nameof(newAgeInMonths));
        
        AgeInMonths = newAgeInMonths;
    }

    public override string ToString()
    {
        return $"{Name} ({Species}, {Breed}), {AgeInMonths} months old, Owner: {Owner.Name}";
    }
}

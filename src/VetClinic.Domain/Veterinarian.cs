namespace VetClinic.Domain;

/// <summary>
/// Represents a veterinarian
/// </summary>
public class Veterinarian
{
    public int Id { get; }
    public string Name { get; }
    public string Specialization { get; }
    public string LicenseNumber { get; }

    /// <summary>
    /// Creates a new Veterinarian with validation
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when data is invalid</exception>
    public Veterinarian(int id, string name, string specialization, string licenseNumber)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be positive", nameof(id));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (name.Length > 100)
            throw new ArgumentException("Name is too long (max 100 characters)", nameof(name));
        
        if (string.IsNullOrWhiteSpace(specialization))
            throw new ArgumentException("Specialization cannot be empty", nameof(specialization));
        
        if (specialization.Length > 100)
            throw new ArgumentException("Specialization is too long (max 100 characters)", nameof(specialization));
        
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("License number cannot be empty", nameof(licenseNumber));
        
        if (licenseNumber.Length > 50)
            throw new ArgumentException("License number is too long (max 50 characters)", nameof(licenseNumber));

        Id = id;
        Name = name.Trim();
        Specialization = specialization.Trim();
        LicenseNumber = licenseNumber.Trim();
    }

    public override string ToString()
    {
        return $"Dr. {Name} ({Specialization}) - License: {LicenseNumber}";
    }
}

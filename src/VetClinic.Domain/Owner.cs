namespace VetClinic.Domain;

/// <summary>
/// Represents an owner of pets
/// </summary>
public class Owner
{
    public int Id { get; }
    public string Name { get; }
    public string PhoneNumber { get; }
    public string Email { get; }

    /// <summary>
    /// Creates a new Owner with validation
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when data is invalid</exception>
    public Owner(int id, string name, string phoneNumber, string email)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be positive", nameof(id));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (name.Length > 100)
            throw new ArgumentException("Name is too long (max 100 characters)", nameof(name));
        
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
        
        if (phoneNumber.Length < 5 || phoneNumber.Length > 20)
            throw new ArgumentException("Phone number must be 5-20 characters", nameof(phoneNumber));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        
        if (email.Length > 100 || !email.Contains("@"))
            throw new ArgumentException("Invalid email format", nameof(email));

        Id = id;
        Name = name.Trim();
        PhoneNumber = phoneNumber.Trim();
        Email = email.Trim();
    }

    public override string ToString()
    {
        return $"Owner: {Name} ({PhoneNumber})";
    }
}

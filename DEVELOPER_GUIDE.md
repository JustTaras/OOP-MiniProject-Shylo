# Посібник розробника - Архітектура VetClinic

## Огляд

Це посібник пояснює архітектуру, паттерни та як розширити систему.

---

## Структура проекту

```
src/
├── VetClinic.Domain/              # Сутності та правила
│   ├── Appointment.cs             # Запис із машиною станів
│   ├── MedicalRecord.cs           # Медичний запис
│   ├── Pet, Owner, Veterinarian   # Основні сутності
│   └── Species, AppointmentStatus # Перерахування
│
├── VetClinic.Application/         # Послуги & логіка
│   ├── AppointmentService         # Управління записами
│   ├── MedicalRecordService       # Медичні записи
│   ├── AnalyticsService           # 5 LINQ запитів
│   └── Result<T>                  # Обробка помилок
│
├── VetClinic.Infrastructure/      # Персистентність
│   ├── IAppointmentRepository     # Интерфейс
│   ├── FileBasedRepository        # JSON файли
│   └── InMemoryRepository         # Для тестів
│
└── VetClinic.Console/             # UI (9 меню)
    ├── ClinicApp.cs               # Головна логіка
    └── DemoDataFactory.cs         # Тестові дані
```

---

## 4-шарова архітектура

```
UI (Console)
    ↓ IAppointmentService
Послуги (AppointmentService, AnalyticsService)
    ↓ IAppointmentRepository
Домен (Appointment, Pet, Owner, Veterinarian)
    ↓ Персистентність
Сховище (JSON файли)
```

---

## 7 Паттернів

### 1. Repository Pattern
**Назва**: IAppointmentRepository  
**Мета**: Абстракція доступу до даних  
**Реалізації**: FileBasedRepository, InMemoryRepository

### 2. Strategy Pattern
**Назва**: IDiagnosisSeverityScorer  
**Мета**: Вибір алгоритму в runtime  
**Реалізації**: KeywordBasedScorer, LengthBasedScorer

### 3. Result Pattern
**Мета**: Функціональна обробка помилок  
**Приклад**: `Result<T>.Ok()`, `Result<T>.Fail()`

### 4. Dependency Injection
**Мета**: Слаба зв'язність  
**Метод**: Injection через конструктор

### 5. DTO Pattern
**Мета**: Передача даних між шарами  
**Приклади**: VeterinarianStatistics, PetMedicalProfile

### 6. Template Method
**Мета**: Шаблон LINQ запитів  
**Приклад**: Where → GroupBy → OrderBy → Select

### 7. Factory Pattern
**Назва**: DiagnosisScorerFactory  
**Мета**: Централізоване створення об'єктів

---

## Розширення системи

### Додати новий сервіс

```csharp
public class MyService
{
    private readonly IAppointmentRepository _repository;
    
    public MyService(IAppointmentRepository repository)
    {
        _repository = repository;
    }
    
    public Result<T> DoSomething() { /* ... */ }
}
```

### Додати新ого scorer'а

```csharp
public class CustomScorer : IDiagnosisSeverityScorer
{
    public double CalculateSeverity(string diagnosis) 
    { 
        // Ваша логіка
    }
}
```

### Додати LINQ запит

```csharp
public Result<IReadOnlyList<T>> MyQuery()
{
    var result = _repository.GetAll()
        .Where(...)
        .GroupBy(...)
        .OrderBy(...)
        .Select(...)
        .ToList();
    return Result<IReadOnlyList<T>>.Ok(result);
}
```

---

## Бізнес-правила (7)

1. Запис у майбутньому
2. Ветеринар доступний
3. Діагноз обов'язковий (1-500 символів)
4. Лікування обов'язкове (1-1000 символів)
5. Тільки завершені записи → медичні записи
6. Стан-машина: Scheduled → Completed/Cancelled
7. Атомарні JSON записи

---

## Тестування

**Типи тестів**: Unit (84), Integration (8), Fault (15)  
**Покриття**: Domain 82%, Application 75%, Infrastructure 56%  
**Виконання**: ~363 мс, 100% успіх

```bash
dotnet test
dotnet test /p:CollectCoverage=true
```

---

## LINQ Запити (5)

1. **Vet Stats** - GroupBy рік-місяць, Count, Average
2. **Pet Profiles** - Діагнози за тваринами
3. **Advanced Search** - Chained Where filters
4. **Clinic Stats** - Multiple GroupBy операції
5. **Utilization** - % роботи ветеринарів

---

## Рекомендації

✅ Завжди ін'єктіруйте залежності  
✅ Повертайте Result<T> для бізнес помилок  
✅ Кидайте винятки для помилок програмування  
✅ Пишіть XML документацію на публічні API  
✅ Тестуйте доменні інваріанти
}
```

**Patterns Used**:
- Result<T>: Functional error handling without exceptions
- Dependency Injection: Services accept IAppointmentRepository
- Business Rule Validation: Pre-conditions checked before operations

#### AnalyticsService (5 LINQ Queries)
```csharp
public class AnalyticsService
{
    // Query 1: Veterinarian statistics with workload analysis
    public VeterinarianStatistics GetVeterinarianStatistics(Veterinarian vet)
    
    // Query 2: Pet medical profile with diagnosis trends
    public PetMedicalProfile GetPetMedicalProfile(Pet pet)
    
    // Query 3: Advanced multi-criteria search
    public IReadOnlyList<Appointment> SearchAppointments(...)
    
    // Query 4: Clinic-wide statistics with aggregations
    public ClinicStatistics GetClinicStatistics()
    
    // Query 5: Veterinarian utilization analysis
    public IReadOnlyList<VeterinarianUtilization> GetVeterinarianUtilization()
}
```

#### Strategy Pattern: DiagnosisSeverityScorer
```csharp
public interface IDiagnosisSeverityScorer
{
    double CalculateSeverity(string diagnosis);
}

// Implementation 1: Keyword-based
public class KeywordBasedScorer : IDiagnosisSeverityScorer
{
    // Identifies severity keywords

public double CalculateSeverity(string diagnosis)
    {
        // Check for severity keywords: critical, urgent, severe, etc.
    }
}

// Implementation 2: Length-based
public class LengthBasedScorer : IDiagnosisSeverityScorer
{
    public double CalculateSeverity(string diagnosis)
    {
        // Longer diagnoses = more complex = higher severity
    }
}

// Factory
public static class DiagnosisScorerFactory
{
    public static IDiagnosisSeverityScorer Create(string strategy)
    {
        return strategy switch
        {
            "keyword" => new KeywordBasedScorer(),
            "length" => new LengthBasedScorer(),
            _ => throw new ArgumentException("Unknown strategy")
        };
    }
}
```

**When to Modify**:
- Adding new use cases (new services)
- Changing business rules (modify services)
- Adding new strategies (extend Strategy pattern)

---

### 3. Infrastructure Layer (VetClinic.Infrastructure)

**Responsibility**: Implement data persistence and external integrations.

**Key Component**: IAppointmentRepository

```csharp
public interface IAppointmentRepository
{
    // CRUD operations
    void Add(Appointment appointment);
    IReadOnlyList<Appointment> GetAll();
    Appointment? GetById(int id);
    void Remove(int id);
    
    // Specialized queries
    IReadOnlyList<Appointment> GetByPet(Pet pet);
    IReadOnlyList<Appointment> GetByVeterinarian(Veterinarian vet);
    bool IsVeterinarianAvailable(Veterinarian vet, DateTime dateTime);
    
    // Medical records
    void AddMedicalRecord(MedicalRecord record);
    IReadOnlyList<MedicalRecord> GetAllMedicalRecords();
    IReadOnlyList<MedicalRecord> GetMedicalRecordsByPet(Pet pet);
    int GetNextMedicalRecordId();
}
```

**Current Implementations**:

#### FileBasedAppointmentRepository
- Reads/writes to JSON file
- Asynchronous I/O (`PersistAsync`)
- Data corruption detection
- Atomic writes for integrity

#### InMemoryAppointmentRepository
- Stores data in List<T>
- Used for testing
- Thread-safe for test isolation

**When to Modify**:
- Changing persistence storage (JSON → database)
- Adding new query methods
- Implementing caching

---

### 4. Console Layer (VetClinic.Console)

**Responsibility**: User interface and menu handling.

**Key Classes**:

#### ClinicApp
- Main menu display and option routing
- User input validation
- Data display formatting

#### DemoDataFactory
```csharp
public class DemoDataFactory
{
    public static (List<Owner> owners, List<Pet> pets, List<Veterinarian> vets) 
        CreateSampleData();
}
```

---

## Design Patterns Used

### 1. Repository Pattern
**What**: Abstract data access behind interface  
**Why**: Decouples services from storage implementation  
**Where**: `IAppointmentRepository` with multiple implementations  

```csharp
// Services depend on abstraction
public AppointmentService(IAppointmentRepository repo) { }

// Can swap implementations
var fileRepo = new FileBasedAppointmentRepository("data.json");
var memoryRepo = new InMemoryAppointmentRepository();
```

### 2. Strategy Pattern
**What**: Define interchangeable algorithms  
**Why**: Choose severity scorer at runtime  
**Where**: `IDiagnosisSeverityScorer` with KeywordBased/LengthBased  

```csharp
var scorer = DiagnosisScorerFactory.Create("keyword");
double severity = scorer.CalculateSeverity("Severe infection");
```

### 3. Result Pattern
**What**: Return Success/Failure instead of exceptions  
**Why**: Distinguish business errors from programming errors  
**Where**: All service methods return `Result<T>`  

```csharp
public Result<Appointment> ScheduleAppointment(...)
{
    // Business error (expected)
    if (!available)
        return Result<Appointment>.Fail("Not available");
    
    try
    {
        // ... logic ...
    }
    catch (ArgumentException ex)
    {
        return Result<Appointment>.Fail($"Invalid: {ex.Message}");
    }
}
```

### 4. Dependency Injection
**What**: Services receive dependencies via constructors  
**Why**: Decouples components, enables testing  
**Where**: All services accept interfaces  

```csharp
public AppointmentService(IAppointmentRepository repository)
{
    _repository = repository;  // Injected dependency
}

// Usage
var repo = new InMemoryAppointmentRepository();
var service = new AppointmentService(repo);
```

### 5. Data Transfer Object (DTO)
**What**: Separate objects for different purposes  
**Why**: UI shows different data than persistence layer  
**Where**: `VeterinarianStatistics`, `PetMedicalProfile`, `ClinicStatistics`  

```csharp
public class VeterinarianStatistics  // DTO for UI display
{
    public Veterinarian Veterinarian { get; set; }
    public int TotalAppointments { get; set; }
    public List<object> MostCommonDiagnoses { get; set; }
}
```

---

## How to Extend the System

### Scenario 1: Add a New Service (e.g., VeterinarianService)

**Steps**:

1. **Create the service class in Application layer**:
```csharp
// src/VetClinic.Application/VeterinarianService.cs
public class VeterinarianService
{
    private readonly IAppointmentRepository _repository;
    
    public VeterinarianService(IAppointmentRepository repository)
    {
        _repository = repository;
    }
    
    public Result<VeterinarianSchedule> GetSchedule(Veterinarian vet, DateTime date)
    {
        // Implement business logic
        var appointments = _repository.GetByVeterinarian(vet)
            .Where(a => a.AppointmentDateTime.Date == date.Date)
            .ToList();
        
        return Result<VeterinarianSchedule>.Ok(
            new VeterinarianSchedule { Vet = vet, Appointments = appointments });
    }
}
```

2. **Register in Program.cs**:
```csharp
// src/VetClinic.Console/Program.cs
var vetService = new VeterinarianService(repository);
```

3. **Add menu option in ClinicApp**:
```csharp
case "10":
    ViewVeterinarianSchedule();
    break;

private void ViewVeterinarianSchedule()
{
    // Get user input, call service, display results
}
```

### Scenario 2: Add a New Repository Implementation (Database)

**Steps**:

1. **Create database repository implementing IAppointmentRepository**:
```csharp
// src/VetClinic.Infrastructure/Persistence/SqlServerRepository.cs
public class SqlServerRepository : IAppointmentRepository
{
    private readonly string _connectionString;
    
    public SqlServerRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public void Add(Appointment appointment)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // Insert appointment into database
        }
    }
    
    public IReadOnlyList<Appointment> GetAll()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            // Query all appointments from database
        }
    }
    
    // Implement remaining interface methods...
}
```

2. **Update Program.cs to use database**:
```csharp
// Old: File-based persistence
// var repository = new FileBasedAppointmentRepository("appointments.json");

// New: Database
var connectionString = "Server=localhost;Database=VetClinic;...";
var repository = new SqlServerRepository(connectionString);
```

3. **No changes needed to services!** They depend on `IAppointmentRepository` interface.

### Scenario 3: Add a New Strategy

**Steps**:

1. **Create new scorer implementing IDiagnosisSeverityScorer**:
```csharp
// src/VetClinic.Application/Strategies/MedicationBasedScorer.cs
public class MedicationBasedScorer : IDiagnosisSeverityScorer
{
    private readonly List<string> _severeConditions = new()
    {
        "cancer", "heart disease", "kidney failure", "diabetes"
    };
    
    public double CalculateSeverity(string diagnosis)
    {
        var lowerDiagnosis = diagnosis.ToLower();
        
        foreach (var condition in _severeConditions)
        {
            if (lowerDiagnosis.Contains(condition))
                return 9.0;  // Very severe
        }
        
        return 3.0;  // Moderate
    }
}
```

2. **Update factory**:
```csharp
public static class DiagnosisScorerFactory
{
    public static IDiagnosisSeverityScorer Create(string strategy)
    {
        return strategy switch
        {
            "keyword" => new KeywordBasedScorer(),
            "length" => new LengthBasedScorer(),
            "medication" => new MedicationBasedScorer(),  // NEW
            _ => throw new ArgumentException("Unknown strategy")
        };
    }
}
```

### Scenario 4: Add a New LINQ Query to AnalyticsService

**Steps**:

```csharp
// Query 6: Get pets by age range
public IReadOnlyList<Pet> GetPetsByAgeRange(int minAge, int maxAge)
{
    var now = DateTime.Now;
    
    return _repository.GetAll()
        .Select(a => a.Pet)  // Get unique pets
        .Distinct()
        .Where(p =>
        {
            int age = (now.Year - p.DateOfBirth.Year);
            return age >= minAge && age <= maxAge;
        })
        .OrderBy(p => p.DateOfBirth)
        .ToList()
        .AsReadOnly();
}
```

---

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "VetClinic.Tests.AppointmentServiceTests"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Test Organization

- **Domain Tests**: Entity invariants, state transitions
- **Service Tests**: Business logic, LINQ queries
- **Integration Tests**: File persistence, round-trip
- **Fault Tests**: Error scenarios, null handling

### Test Data Factory

```csharp
// tests/VetClinic.Tests/TestDataFactory.cs
public class TestDataFactory
{
    public static Owner CreateOwner() { ... }
    public static Pet CreatePet(int id, Owner owner) { ... }
    public static Appointment CreateAppointment(int id, Pet pet, Vet vet) { ... }
}

// Usage in tests
var owner = TestDataFactory.CreateOwner();
var pet = TestDataFactory.CreatePet(1, owner);
```

---

## Code Quality Standards

### Naming Conventions
- **Classes**: PascalCase (e.g., `AppointmentService`)
- **Methods**: PascalCase (e.g., `GetAppointmentsByPet`)
- **Variables**: camelCase (e.g., `selectedPet`)
- **Private fields**: _camelCase (e.g., `_repository`)

### XML Documentation
- All public methods and classes should have XML comments
- Include `<summary>`, `<param>`, `<returns>` tags
- Document business rules and edge cases

```csharp
/// <summary>
/// Schedule a new appointment with availability validation
/// </summary>
/// <param name="pet">The pet for the appointment</param>
/// <param name="veterinarian">The assigned veterinarian</param>
/// <param name="appointmentDateTime">Future date and time</param>
/// <param name="reason">Reason for visit (max 500 chars)</param>
/// <returns>Result with created appointment or error message</returns>
public Result<Appointment> ScheduleAppointment(Pet pet, Veterinarian veterinarian,
                                               DateTime appointmentDateTime, string reason)
```

### SOLID Principles Compliance

- **Single Responsibility**: Each service has one reason to change
- **Open/Closed**: Strategies can be extended without modifying existing code
- **Liskov Substitution**: Repositories are interchangeable
- **Interface Segregation**: Services depend on focused interfaces
- **Dependency Inversion**: Services depend on abstractions, not concrete classes

---

## Performance Considerations

### Current Limits
- **Appointments**: Efficient up to 10,000 records
- **Medical Records**: Efficient up to 50,000 records
- **Veterinarians**: Efficient up to 100 (typical clinic size)

### Optimization Strategies

**If search becomes slow**:
- Implement database with indexes
- Add caching layer (Redis)
- Use pagination for large result sets

**If aggregation is slow**:
- Cache statistics (24-hour TTL)
- Use database materialized views
- Implement PLINQ for parallel processing

**If persistence is slow**:
- Add compression to JSON
- Implement incremental backups
- Consider NoSQL database

See [docs/performance-analysis.md](docs/performance-analysis.md) for detailed analysis.

---

## Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| "IAppointmentRepository not found" | Ensure interface is in Infrastructure project |
| "Service constructor ambiguous" | Provide explicit type when registering dependency |
| "JSON deserialization fails" | Check data file format matches expected schema |
| "Tests fail in CI but pass locally" | Check timezone settings, use UTC |

---

## Related Documentation

- [USER_GUIDE.md](USER_GUIDE.md) - For end users
- [docs/release-plan.md](docs/release-plan.md) - Release scope and decisions
- [docs/test-strategy.md](docs/test-strategy.md) - Testing approach
- [TESTING.md](TESTING.md) - Test execution details
- [docs/performance-analysis.md](docs/performance-analysis.md) - Performance metrics

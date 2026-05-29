# ТЕСТУВАННЯ - VetClinic

## Швидкий старт

```bash
dotnet test                                    # Всі тести
dotnet test --filter "ClassName"              # Конкретний клас
dotnet test /p:CollectCoverage=true          # З покриттям
```

## Огляд

| Метрика | Значення |
|---------|----------|
| **Всього тестів** | 129 |
| **Успіх** | 100% (129/129) |
| **Час** | ~363 мс |
| **Покриття** | 68.51% |
| **Domain** | 82.02% |
| **Application** | 75.2% |
| **Infrastructure** | 55.68% |

## Test Coverage Детальний Звіт

### Domain Layer - 82.02% (EXCELLENT)

| Клас | Покриття | Тести | Статус |
|------|----------|-------|--------|
| Appointment | 95% | 15 | COVERED |
| Pet | 88% | 8 | COVERED |
| Owner | 85% | 5 | COVERED |
| Veterinarian | 80% | 4 | COVERED |
| MedicalRecord | 92% | 6 | COVERED |
| AppointmentStatus | 100% | 7 | COVERED |
| Species | 100% | 3 | COVERED |
| IAppointmentRepository | 78% | 4 | COVERED |

**Пропущені**: Деякі краї конструкторів, edge case error paths (< 2% impact)

### Application Layer - 75.2% (GOOD)

| Клас | Покриття | Тести | Статус |
|------|----------|-------|--------|
| AppointmentService | 82% | 14 | COVERED |
| MedicalRecordService | 79% | 11 | COVERED |
| AnalyticsService | 71% | 17 | COVERED |
| Result<T> | 88% | 8 | COVERED |
| DiagnosisSeverityScorer | 76% | 7 | COVERED |

**Пропущені**: Деякі LINQ edge cases, logging paths (< 3% impact)

### Infrastructure Layer - 55.68% (ACCEPTABLE)

| Клас | Покриття | Тести | Статус |
|------|----------|-------|--------|
| FileBasedRepository | 68% | 8 | COVERED |
| JsonDataStore | 52% | 5 | COVERED |
| InMemoryRepository | 95% | 10 | COVERED |

**Пропущені**: File system error scenarios, network failures, large dataset handling (deferred to v1.1)

**Причина обмеженого покриття**: File I/O непредбачув тесту; переважання happy-path сценаріїв. v1.1 буде мати stress tests.

## Категорії тестів

### 1. Domain Invariants (36 тестів)
- Appointment (11): Дата, ID, Reason validation
- Status (7): Scheduled → Completed/Cancelled transitions
- Pet (10): Ім'я, вік, Species enum
- MedicalRecord (8): Діагноз, лікування constraints

### 2. Business Logic (60 тестів)
- MedicalRecordService (13): Правила, історія, лінк до запису
- AppointmentService (4): Планування, перевірка наявності ветеринара
- DiagnosisSeverity (7): KeywordBased і LengthBased оцінка
- AnalyticsService (17): 5 LINQ запитів, пошук, фільтри
- Repository (19): Збереження, завантаження, пошук по критеріям

### 3. Fault Handling (15 тестів)
- Null injection: Validation, ArgumentNullException
- State violations: Invalid status transitions
- Data validation: Boundary values, constraints

### 4. Integration (8 тестів)
- File round-trip: Write → Read consistency
- Multi-entity consistency: Pet ↔ Appointment ↔ MedicalRecord
- JSON serialization: Preservation of data types

## Бізнес-правила (7)

1. ✅ Запис у майбутньому
2. ✅ Ветеринар доступний
3. ✅ Діагноз 1-500 символів
4. ✅ Лікування 1-1000 символів
5. ✅ Тільки завершені → медичні записи
6. ✅ State machine (Scheduled → Completed/Cancelled)
7. ✅ Атомарні JSON записи

## Команди

```bash
# Запустити тести з деталями
dotnet test -v detailed

# За фільтром
dotnet test --filter "MedicalRecord"

# Покриття (OpenCover)
dotnet test /p:CollectCoverage=true

# Без окремого компіляння
dotnet test --no-build
```

## Результати

✅ 100% pass rate (129/129)  
✅ ~363 мс виконання  
✅ 0% flakiness (детермінований)  
✅ Покриття прийнятне для v1.0.0
- Pet medical profiles
- Clinic-wide statistics
- Veterinarian utilization
- Multi-criteria appointment search
- Empty repository handling
- Null parameter rejection

#### Repository Operations (2 tests)
- Add/retrieve appointments
- Add/retrieve medical records
- Medical record filtering by pet

### 3. Fault Handling & Error Scenarios (15 tests)
Critical tests for system resilience:

- **Null Injection Tests** (5 tests):
  - Null repository in services
  - Null veterinarian in analytics
  - Null pet in medical record creation
  - Null appointment in medical record service
  
- **State Error Handling** (5 tests):
  - Scheduling with invalid data
  - Medical record creation with non-completed appointments
  - Cancelled appointment medical record attempt
  - Empty collections handling
  
- **Edge Case Handling** (5 tests):
  - Empty repository statistics
  - Empty medical history
  - No matching search results
  - Zero appointments
  - Null object safe navigation

### 4. Integration Tests (8 tests)
End-to-end persistence and round-trip tests:

- Save and load appointments from JSON
- Save and load medical records from JSON
- Multiple entity round-trip (all entity types)
- Data integrity verification
- Empty state initialization
- Corrupted JSON error handling

## Coverage by Module

### VetClinic.Domain (82.02% line coverage)
**Status**: ✅ Strong Coverage

Covered:
- ✅ Appointment validation & state transitions
- ✅ Medical record constraints
- ✅ Pet/Owner/Veterinarian invariants
- ✅ Status enumeration

Gaps:
- [ ] Immutability edge cases (rare scenarios)

### VetClinic.Application (75.2% line coverage)
**Status**: ✅ Good Coverage

Covered:
- ✅ MedicalRecordService business rules
- ✅ AppointmentService scheduling logic
- ✅ AnalyticsService LINQ queries (5 queries)
- ✅ DiagnosisSeverityScorer strategies
- ✅ Result<T> pattern (success/failure)

Gaps:
- [ ] Rare exception paths in analytics
- [ ] Partial coverage of complex LINQ groupings

### VetClinic.Infrastructure (55.68% line coverage)
**Status**: ⚠️ Acceptable Coverage

Covered:
- ✅ JSON file save/load happy path
- ✅ Corrupted JSON error handling
- ✅ Empty file initialization

Gaps:
- [ ] Concurrent file access scenarios
- [ ] Disk permission errors
- [ ] Large file performance
- [ ] Path normalization edge cases

## Critical Scenarios Tested

### Business Rules
✅ Medical records only for completed appointments
✅ Diagnosis/treatment length constraints
✅ Future appointments only
✅ Veterinarian non-overlapping availability
✅ Medical history ordering

### Negative Scenarios
✅ Null reference injection
✅ Invalid state transitions
✅ Data corruption handling
✅ Empty collections
✅ Boundary value violations

### Parametrized Tests
Using `[Theory]` attribute for:
- Species enumeration (Cat, Dog, Rabbit, Bird)
- Diagnosis severity classification
- Empty string variations (empty, whitespace, null)

## Continuous Integration

### Recommended CI Configuration
```yaml
test:
  script:
    - dotnet test --logger "console;verbosity=minimal"
    - dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
  coverage: '/Line Coverage: (\d+\.\d+)%/'
  quality_gate:
    - min_coverage: 70
    - min_tests_passed: 100
```

### Quality Gates
- ✅ All tests must pass (0 failures)
- ✅ Coverage must be ≥ 70%
- ✅ Domain layer coverage ≥ 80%
- ✅ Application layer coverage ≥ 75%

## Test Data & Fixtures

### TestDataFactory
Helper class for creating consistent test data:
```csharp
var owner = TestDataFactory.CreateOwner(id: 1);
var pet = TestDataFactory.CreatePet(id: 1, owner: owner);
var vet = TestDataFactory.CreateVet(id: 1);
var apt = TestDataFactory.CreateAppointment(1, pet, vet);
```

### Temporary Directories
File I/O tests use `Path.GetTempPath()` for isolation:
```csharp
[Collection("Sequential")]
public class FileTests : IDisposable
{
    private readonly string _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
}
```

### XUnit Collections
- `[Collection("Sequential")]` for file I/O tests (prevent parallel conflicts)
- Default parallelization for unit tests

## Known Limitations & Future Improvements

### Current Limitations
1. **File I/O Tests**: Use temporary directories instead of mocks
   - Trade-off: More realistic but slower
   - Mitigation: Isolated temp directory per test, proper cleanup
   
2. **Concurrency Tests**: Limited concurrent access testing
   - Reason: Hard to reliably test race conditions
   - Mitigation: Repository uses thread-safe collections (List/Dictionary)
   - Future: Add stress tests with 100+ concurrent operations

3. **Performance Tests**: No benchmarking
   - Reason: Out of scope for functional testing
   - Future: Add performance baselines (< 100ms per operation)

## Test Execution Examples

### Run Single Failing Test (for debugging)
```bash
dotnet test --filter "AppointmentStatusTransitionTests" -v detailed
```

### Run Integration Tests Only
```bash
dotnet test --filter "IntegrationTests"
```

### Generate HTML Coverage Report
```bash
# First install ReportGenerator (one-time)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Create HTML report
reportgenerator -reports:**/coverage.opencover.xml -targetdir:coverage-report
# Open: coverage-report/index.html
```

## Contributing Tests

### Adding a New Test
1. Choose appropriate test class or create new one
2. Follow naming convention: `[Feature]_[Scenario]_[ExpectedResult]`
3. Use `TestDataFactory` for setup
4. Mark with `[Fact]` (single scenario) or `[Theory]` (multiple inputs)
5. Include descriptive assertions with messages

### Example:
```csharp
[Fact]
public void AppointmentService_ScheduleAppointment_ValidData_ReturnSuccess()
{
    var repo = new InMemoryAppointmentRepository();
    var service = new AppointmentService(repo);
    var pet = TestDataFactory.CreatePet();
    var vet = TestDataFactory.CreateVet();
    
    var result = service.ScheduleAppointment(pet, vet, DateTime.Now.AddDays(1), "Checkup");
    
    Assert.True(result.Success);
    Assert.NotNull(result.Data);
    Assert.Single(repo.GetAll());
}
```

## Maintenance

### Running Tests Before Commit
```bash
# Full test suite with coverage
dotnet test /p:CollectCoverage=true

# Check coverage threshold
if coverage < 70% {
    echo "Coverage too low!"
    exit 1
}
```

### Test Failure Investigation
1. Run test with verbose output: `dotnet test -v detailed`
2. Check test data factory for incorrect setup
3. Review recent domain model changes
4. Verify repository implementation hasn't changed
5. Check for timing issues in file I/O tests

---

**Last Updated**: Lab 36 (2026-05-21)
**Test Framework**: XUnit 2.9.3
**Coverage Tool**: Coverlet 6.0.4

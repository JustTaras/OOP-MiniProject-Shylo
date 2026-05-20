# Iteration 2 (Lab 35) Plan - Business Logic & Persistence

## 🎯 Iteration Goals

Transform Lab 34 baseline ("demo with one operation") into a managed application solution:
1. Implement file-based persistence with JSON (async/await)
2. Extend business logic with 3+ complete use cases
3. Add design patterns for extensibility
4. Implement LINQ queries for analytics
5. Enhance console interface for new scenarios
6. Cover with 12+ unit tests

---

## 📋 Use Cases from Backlog Moving to Implementation

### UC1: Schedule Appointment (COMPLETED in Lab 34 - Continue Support)
- User schedules pet visit with veterinarian
- System validates availability and domain rules
- Appointment created and persisted
- **Lab 35 Extension**: Support persistence, recover from file on startup

### UC2: Complete Appointment & Create Medical Record (NEW)
- User marks appointment as completed
- User enters diagnosis and treatment
- System creates medical record linked to appointment
- Medical record is persisted
- **Business Rules**:
  - Cannot complete cancelled appointment
  - Diagnosis cannot be empty
  - Treatment notes should be detailed
  - Medical record must reference appointment ID and pet

### UC3: View Pet Medical History (NEW)
- User selects a pet
- System displays all appointments and associated medical records
- Results filtered to completed appointments only
- Can search by date range
- **Business Rules**:
  - Only show completed appointments with records
  - Sort by date (newest first)
  - Display diagnosis and treatment clearly

### UC4: Generate Veterinarian Statistics (NEW)
- User selects a veterinarian
- System calculates statistics:
  - Total appointments (all time)
  - Completed appointments count
  - Average appointment per month
  - Most common diagnoses
  - Recent activity
- **Business Rules**:
  - Statistics only from completed appointments
  - Should filter by date range option

---

## 🏛️ Classes from Lab 34 Remaining Unchanged

| Class | Reason |
|-------|--------|
| `Owner` | Complete encapsulation, no extension needed |
| `Pet` | Complete with species enum, age validation |
| `Veterinarian` | Has specialization and license validation |
| `Species` | Enum type, complete |
| `AppointmentStatus` | Enum type, complete |
| `Appointment.cs` | Already has Complete()/Cancel() methods ready |
| `MedicalRecord.cs` | Domain class prepared for Lab 35 use |

---

## 🔌 Planned Extension Points

### 1. **Strategy Pattern for Medical Record Scoring**
**Rationale**: Different clinics may have different rules for:
- Diagnosis severity scoring
- Treatment complexity rating
- Patient risk assessment

**Why needed without pattern**: Switching between scoring algorithms would require modifying MedicalRecordService or adding large if/else chains in console.

**New behavior on Lab 37**: Alternative scoring strategies (cost-based, urgency-based, complexity-based) can be added without modifying existing code.

**Implementation**: 
```csharp
public interface IDiagnosisScorer
{
    int CalculateSeverity(string diagnosis);
}
```

### 2. **Repository Pattern Enhancement - Async Persistence**
**Current**: InMemoryAppointmentRepository (synchronous, in-memory)
**Lab 35**: Add `IDataStore<Appointment>` and `IDataStore<MedicalRecord>` for JSON file I/O

**Why needed**: 
- Swap JSON for XML, SQLite, or SQL later without changing business logic
- Async operations for realistic I/O handling

---

## ⚠️ Risks & Identified Issues

| Risk | Impact | Mitigation |
|------|--------|-----------|
| **Logic Duplication** | Filtering/sorting repeated in console and service | Use LINQ queries in service layer, console only displays |
| **Weak Persistence Interface** | Loading from file might lose or corrupt data | Strict validation during deserialization, rollback mechanism |
| **Test Coverage Gaps** | New async code not tested | Add integration tests for persistence, mock IDataStore in unit tests |
| **ID Generation** | Simple incrementing IDs conflict on reload | Implement ID conflict detection, use timestamp-based or UUID later |
| **Circular Dependencies** | Appointment ↔ MedicalRecord ↔ Persistence | Keep domain layer pure (no dependencies on persistence), pass IDs only when needed |
| **Date/Time Serialization** | DateTime formats may differ JSON ↔ C# | Use ISO 8601 format consistently |

---

## 📝 Minimum Business Rules (Must be in code, tests, and docs)

1. **Cannot complete a cancelled appointment**
   - Code: `Appointment.Complete()` method
   - Test: `CompleteAppointmentTests`

2. **Medical records require non-empty diagnosis**
   - Code: `MedicalRecord` constructor validation
   - Test: `MedicalRecordValidationTests`

3. **Veterinarian cannot have overlapping appointments**
   - Code: `IsVeterinarianAvailable()` in repository
   - Test: `VeterinarianAvailabilityTests`

4. **Pets must be associated with an owner**
   - Code: `Pet` constructor validation
   - Test: `PetValidationTests`

5. **Medical history should only show completed appointments**
   - Code: `GetMedicalHistoryForPet()` uses status filter
   - Test: `MedicalHistoryFilteringTests`

---

## 🏗️ Architectural Decisions

### Persistence Layer Structure
```
Infrastructure/
├── Persistence/
│   ├── IDataStore<T>           (async contract)
│   ├── JsonDataStore<T>        (JSON file implementation)
│   ├── AppointmentDataStore    (wrapper for typed access)
│   └── MedicalRecordDataStore  (wrapper for typed access)
└── Repositories/
    └── FileBasedAppointmentRepository  (combines in-memory + async persistence)
```

### Service Layer Expansion
```
Application/
├── AppointmentService          (existing - maintains API)
├── MedicalRecordService        (NEW - medical record operations)
├── AnalyticsService            (NEW - statistics & queries)
└── Strategies/
    └── IDiagnosisScorer        (pluggable scoring)
```

### Console Layer
```
Console/
├── Menus/
│   ├── AppointmentMenu        (existing)
│   ├── MedicalRecordMenu      (NEW)
│   └── AnalyticsMenu          (NEW)
└── Handlers/ (NEW)
    └── ConsoleHandlers.cs     (delegates for menu actions)
```

---

## 📊 Expected Test Count

| Category | Count | Total |
|----------|-------|-------|
| Domain invariants (existing) | 5 | 5 |
| Existing service tests | 7 | 7 |
| MedicalRecord operations | 4 | 4 |
| Persistence (load/save/errors) | 5 | 5 |
| Veterinarian statistics | 3 | 3 |
| **TOTAL** | | **≥12** |

---

## ✅ Acceptance Criteria for Lab 35 Completion

- [ ] Branch `lab35-business-and-persistence` created
- [ ] JSON persistence works: Save & load appointments/medical records
- [ ] 3 use cases fully functional in console menu
- [ ] Minimum 5 business rules coded and tested
- [ ] LINQ queries working: filtering, sorting, aggregation (4+ queries)
- [ ] Strategy pattern or Factory pattern implemented with documented extensibility
- [ ] 12+ unit tests passing
- [ ] All tests passing: `dotnet test`
- [ ] Documentation updated (iteration-2.md, class diagrams, backlog)
- [ ] Code builds cleanly: `dotnet build`
- [ ] Console app runs without errors, loads persisted data

---

## 📅 Implementation Order

1. ✅ Create this plan
2. 🔄 Implement IDataStore<T> and JsonDataStore<T> (async)
3. 🔄 Refactor repository to use async persistence
4. 🔄 Create MedicalRecordService with business rules
5. 🔄 Add AnalyticsService with LINQ queries
6. 🔄 Implement Strategy pattern for diagnosis scoring
7. 🔄 Extend console UI (new menus)
8. 🔄 Add 12+ unit tests
9. 🔄 Update documentation & diagrams
10. 🔄 Verify all tests pass
11. 🔄 Create docs/iteration-2.md handoff report

---

**Plan created**: May 20, 2026  
**Target completion**: Lab 35 (within scheduled deadline)  
**Status**: Ready for implementation

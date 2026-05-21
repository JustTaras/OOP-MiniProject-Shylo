# Product Backlog

## Iteration 1 (Lab 34) - Baseline ✅ COMPLETE

### Must Have
- [x] Domain model with 5-8 classes (Owner, Pet, Veterinarian, Appointment, MedicalRecord)
- [x] Enums for type safety (Species, AppointmentStatus)
- [x] Domain invariants - prevent invalid object creation
- [x] Encapsulation - properties with private setters and validation
- [x] Repository interface (IAppointmentRepository)
- [x] In-memory repository implementation
- [x] Application service (AppointmentService) with business logic
- [x] Result pattern for error handling
- [x] Console UI menu with vertical slice
- [x] One complete user scenario: Schedule Appointment
- [x] Unit tests (minimum 15)
- [x] Documentation (vision.md, backlog.md, diagrams)
- [x] CI/CD pipeline (.github/workflows/dotnet.yml)
- [x] .gitignore configuration

### Nice to Have
- [x] Multiple test classes covering different layers
- [x] DemoDataFactory for sample data
- [x] Detailed error messages to users
- [x] View appointments by pet/veterinarian
- [x] Veterinarian availability checking

---

## Iteration 2 (Lab 35) - Business Logic, Medical Records & Analytics ✅ COMPLETE

### Must Have
- [x] Create medical records after appointment completion
- [x] View medical history for a pet (sorted by date, newest first)
- [x] Display diagnosis and treatment in medical records
- [x] Link appointments to medical records via AppointmentId
- [x] Extended service methods for medical record operations
- [x] Business rules: Cannot create record for non-completed appointment
- [x] Diagnosis/Treatment validation (length, non-empty)
- [x] Tests for medical record operations (45 total tests, 100% passing)
- [x] Update docs/iteration-1.md → docs/iteration-2.md
- [x] JSON persistence with asynchronous I/O
- [x] LINQ-based analytics queries (5+ queries)
- [x] Strategy Pattern implementation (2 severity scorers + factory)

### Nice to Have
- [x] Medical record search by date range
- [x] Medical statistics (most common diagnoses, vet workload analysis)
- [x] Console UI extensions (9 menu options instead of 5)
- [x] Multi-criteria appointment search (owner, pet, status, date)
- [x] Veterinarian utilization metrics
- [x] Strategy runtime swapping without code changes

### Completed Status
✅ **All Must Have**: Completed
✅ **All Nice to Have**: Completed (exceeded expectations)
✅ **Test Coverage**: 45/45 tests passing
✅ **Build Status**: All 5 projects compile
✅ **Documentation**: iteration-2.md with handoff notes

---

## Iteration 3 (Lab 36) - Database Persistence & Integration Tests 📋 PLANNED

### Must Have
- [ ] Set up SQL Server or PostgreSQL database
- [ ] Entity Framework Core integration
- [ ] Database schema design & migrations (preserve data model)
- [ ] SQL-based repository implementation (IAppointmentRepository adapter)
- [ ] Connection string configuration & secrets management
- [ ] Data seed with sample clinics & veterinarians
- [ ] Integration tests with real database (not in-memory)
- [ ] Performance optimization (indexing, query optimization)
- [ ] Concurrent access handling (transactions, locks)

### Nice to Have
- [ ] Database backup/restore functionality
- [ ] Audit logging for record changes (who, when, what changed)
- [ ] Data validation in database constraints (replicate domain rules)
- [ ] Stored procedures for complex queries
- [ ] View objects for reporting

### Expected Challenges (from Lab 35)
- Large datasets: Current LINQ may slow with 10K+ records → need pagination
- Concurrent appointments: File-based repo not thread-safe → need transactions
- Data corruption: Test with manually corrupted JSON files
- Timezone handling: Use UTC instead of DateTime.Now

---

## Iteration 4 (Lab 37) - Advanced Features & UI 📋 PLANNED

### Must Have
- [ ] Windows Forms or WPF GUI application
- [ ] User authentication (login/roles)
- [ ] Vet/Receptionist/Admin role-based access control
- [ ] Advanced reporting dashboard
- [ ] Appointment rescheduling
- [ ] Payment tracking for services
- [ ] Report export (PDF, CSV)

### Nice to Have
- [ ] Email notifications for appointments
- [ ] Backup & restore from UI
- [ ] Analytics dashboard with charts
- [ ] SMS reminders
- [ ] Appointment confirmations

---

## Potential Future Extensions (Lab 38+) 💡 OPTIONAL

- [ ] Design Patterns: Observer (notifications), Decorator (service add-ons)
- [ ] Additional Strategy implementations (more severity scorers)
- [ ] Delegates/Events for menu actions
- [ ] IDisposable for resource management
- [ ] RESTful API with ASP.NET Core
- [ ] Microservices architecture
- [ ] Mobile app (MAUI)
- [ ] Real-time appointments (SignalR)

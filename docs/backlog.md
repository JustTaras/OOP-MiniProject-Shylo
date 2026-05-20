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

## Iteration 2 (Lab 35) - Medical Records & Extended Views 📋 PLANNED

### Must Have
- [ ] Create medical records after appointment completion
- [ ] View medical history for a pet
- [ ] Display diagnosis and treatment in medical records
- [ ] Link appointments to medical records
- [ ] Extended service methods for medical record operations
- [ ] Tests for medical record operations (10+ new tests)
- [ ] Update iteration-1.md → iteration-2.md

### Nice to Have
- [ ] Medical record search by date range
- [ ] Export appointment/medical history
- [ ] Medical statistics (most common diagnoses)

---

## Iteration 3 (Lab 36) - Persistence Layer 📋 PLANNED

### Must Have
- [ ] Set up SQL Server or PostgreSQL database
- [ ] Entity Framework Core integration
- [ ] Database schema design & migrations
- [ ] SQL-based repository implementation (IAppointmentRepository)
- [ ] Connection string configuration
- [ ] Data seed with sample clinics
- [ ] Integration tests with real database
- [ ] Performance optimization (indexing, query optimization)

### Nice to Have
- [ ] Database backup/restore functionality
- [ ] Audit logging for record changes
- [ ] Data validation in database constraints

---

## Iteration 4 (Lab 37) - Advanced Features & UI 📋 PLANNED

### Must Have
- [ ] Windows Forms or WPF GUI application
- [ ] User authentication (login/roles)
- [ ] Vet/Receptionist/Admin role-based access
- [ ] Advanced reporting (appointments per vet, busiest days, etc.)
- [ ] Appointment rescheduling
- [ ] Payment tracking for services

### Nice to Have
- [ ] Email notifications for appointments
- [ ] Backup & restore from UI
- [ ] Analytics dashboard
- [ ] SMS reminders

---

## Potential Future Extensions (Lab 38+) 💡 OPTIONAL

- [ ] Design Patterns: Strategy (pricing), Observer (notifications), Decorator (service add-ons), Facade (complex queries)
- [ ] Factory pattern for appointment/record creation
- [ ] Delegates/Events for menu actions
- [ ] IDisposable for resource management
- [ ] Operator overloading for Pet/Appointment comparisons
- [ ] RESTful API with ASP.NET Core
- [ ] Microservices architecture
- [ ] Mobile app (Xamarin/MAUI)
- [ ] Real-time appointments (SignalR)

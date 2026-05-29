# Беклог продукту

## Ітерація 1 (Lab 34) - Основа ✅ ГОТОВО

- [x] Домен: 5+ класів
- [x] Перерахування: Species, AppointmentStatus
- [x] Інваріанти домену
- [x] Repository паттерн
- [x] AppointmentService
- [x] UI: 5 меню опцій
- [x] 15+ тестів
- [x] CI/CD pipeline

---

## Ітерація 2 (Lab 35) - Логіка & Аналітика ✅ ГОТОВО

- [x] Медичні записи (пов'язані з записами)
- [x] Історія хвороб (датована)
- [x] 5 LINQ запитів (аналітика)
- [x] JSON персистентність
- [x] Strategy паттерн (DiagnosisSeverityScorer)
- [x] UI: 9 меню опцій
- [x] 45 тестів
- [x] Розширена документація

---

## Ітерація 3 (Lab 36) - Тестування & Якість ✅ ГОТОВО

- [x] 84 нових тестів (129 всього)
- [x] 68.51% покриття коду
- [x] Integration тести
- [x] Fault handling тести
- [x] 100% success rate
- [x] Бенчмарки продуктивності

---

## Ітерація 4 (Lab 37) - Релік & Документація ✅ ГОТОВО

- [x] План релізу v1.0.0
- [x] Фінальний рефакторинг
- [x] 13 документів
- [x] DEMO сценарій
- [x] Defense Q&A
- [x] Аналіз продуктивності

---

## Відстрочено (v2.0+)

- [ ] SQL Server база даних
- [ ] GUI (WPF)
- [ ] Автентикація
- [ ] API
- [ ] Шифрування
- [ ] Мульти-клініка підтримка
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

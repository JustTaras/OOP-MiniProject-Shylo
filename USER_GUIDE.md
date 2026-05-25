# Посібник користувача - VetClinic

## Огляд

VetClinic - система управління ветеринарною клініцею для управління записами, медичними записами та аналітикою.

---

## Старт

**Вимоги**: .NET 7.0+

```bash
cd OOP-MiniProject-Shylo
dotnet restore
dotnet build
dotnet run --project src/VetClinic.Console
```

---

## Основні функції (9 опцій меню)

### 1. Планування запису
- Оберіть тварину → ветеринара → дату/час → причину
- Система перевіряє наявність ветеринара
- ✅ Запис створено

### 2. Переглянути всі записи
- Список всіх записів із статусом
- Деталі: ID, тварина, ветеринар, дата, причина

### 3. Записи тварини
- Оберіть тварину → всі її записи

### 4. Записи ветеринара
- Оберіть ветеринара → всі його записи

### 5. Завершити запис & Медичний запис
- Оберіть запис → позначте як завершено
- Введіть діагноз (≤ 500 символів)
- Введіть лікування (≤ 1000 символів)
- Рівень тяжкості обчислюється автоматично

### 6. Медична історія
- Оберіть тварину → історія всіх медичних записів
- Датовано, з діагнозом та лікуванням

### 7. Аналітика & Статистика
- **A**: Статистика клініки (загальна кількість, ветеринари)
- **B**: Навантаження ветеринарів
- **C**: Профілі тварин (діагнози)
- **D**: Розширений пошук

### 8. Зберегти дані у файл
- Збереження в JSON

### 9. Вихід
- Збереження та закриття

---

## Приклади

**Заплануване запис**: 2024-12-25 14:30  
**Діагноз**: "Щеплення" → Рівень: Низький  
**Медичний запис**: Автоматично пов'язано

---

## Рекомендації

✅ Завершуйте записи перед створенням медичних записів  
✅ Регулярно зберігайте дані (опція 8)  
✅ Медичні записи - для завершених записів тільки  

---

## Вирішення проблем

| Проблема | Рішення |
|----------|---------|
| "Не можна створити запис" | Дата має бути в майбутньому |
| "Ветеринар недоступний" | Виберіть іншу дату/час |
| "Неможливо завершити запис" | Запис вже завершено/скасовано |

**Validation Rules**:
- Appointment must be in the future
- Veterinarian must be available at that time
- Reason cannot be empty (max 500 characters)

---

### 2. View All Appointments

**Purpose**: See a list of all scheduled appointments.

**Steps**:
1. Select **Option 2** from the main menu
2. Application displays all appointments in the system

**Example Output**:
```
============================================================
📋 ALL APPOINTMENTS
============================================================
ID: 1 | Pet: Buddy | Vet: Dr. Sarah Johnson | Status: Scheduled
       Time: 2024-12-25 14:30 | Reason: Annual checkup

ID: 2 | Pet: Whiskers | Vet: Dr. Mike Chen | Status: Completed
       Time: 2024-12-10 10:00 | Reason: Vaccination

Total appointments: 2
```

**Status Legend**:
- **Scheduled**: Appointment is booked, awaiting execution
- **In Progress**: Appointment is currently happening
- **Completed**: Appointment has finished
- **Cancelled**: Appointment was cancelled

---

### 3. View Pet Appointments

**Purpose**: See all appointments for a specific pet.

**Steps**:
1. Select **Option 3** from the main menu
2. Choose a pet from the list
3. View all appointments for that pet, including status and details

**Example Output**:
```
============================================================
🐾 APPOINTMENTS FOR: Buddy
============================================================
Appointment ID: 1 | Status: Completed | Date: 2024-12-10 10:00
Reason: Annual checkup
Veterinarian: Dr. Sarah Johnson

Appointment ID: 2 | Status: Scheduled | Date: 2024-12-25 14:30
Reason: Vaccination
Veterinarian: Dr. Sarah Johnson

Total appointments for Buddy: 2
```

---

### 4. View Veterinarian Appointments

**Purpose**: See all appointments for a specific veterinarian.

**Steps**:
1. Select **Option 4** from the main menu
2. Choose a veterinarian from the list
3. View all their appointments

**Example Output**:
```
============================================================
👨‍⚕️ APPOINTMENTS FOR: Dr. Sarah Johnson
============================================================
Appointment ID: 1 | Pet: Buddy | Owner: John Smith
   Status: Completed | Time: 2024-12-10 10:00

Appointment ID: 2 | Pet: Fluffy | Owner: Sarah Brown
   Status: Scheduled | Time: 2024-12-25 14:30

Total appointments: 2
Completion Rate: 50%
```

---

### 5. Complete Appointment & Create Medical Record

**Purpose**: Mark an appointment as completed and immediately create a medical record with diagnosis and treatment details.

**Steps**:
1. Select **Option 5** from the main menu
2. Choose an appointment to complete:
   ```
   📋 Select appointment to complete:
     1. Appointment ID: 1 - Buddy with Dr. Sarah Johnson
     2. Appointment ID: 2 - Whiskers with Dr. Mike Chen
   ```
3. Enter diagnosis (e.g., "Mild ear infection"):
   - Cannot be empty
   - Maximum 500 characters
4. Enter treatment plan (e.g., "Prescribed ear drops, 2x daily for 7 days"):
   - Cannot be empty
   - Maximum 1000 characters

**Business Rule**: You can ONLY create medical records for COMPLETED appointments.

**Example Output**:
```
✅ Appointment completed successfully
✅ Medical record created for Buddy on 2024-12-10
   Diagnosis: Mild ear infection
   Treatment: Prescribed ear drops, 2x daily for 7 days
```

---

### 6. View Pet Medical History

**Purpose**: See all medical records for a specific pet, sorted by visit date (newest first).

**Steps**:
1. Select **Option 6** from the main menu
2. Choose a pet from the list
3. View their complete medical history

**Example Output**:
```
============================================================
💊 MEDICAL HISTORY FOR: Buddy
============================================================
Visit Date: 2024-12-10
Diagnosis: Mild ear infection
Treatment: Prescribed ear drops, 2x daily for 7 days

Visit Date: 2024-11-15
Diagnosis: Annual health check - all normal
Treatment: Updated vaccinations, wellness consultation

Total visits: 2
```

**Information Shown**:
- Visit date (creation date of the medical record)
- Diagnosis
- Treatment plan
- Unique diagnoses count
- Last visit date

---

### 7. View Analytics & Statistics

**Purpose**: Generate and view clinic-wide analytics and reports.

**Steps**:
1. Select **Option 7** from the main menu
2. Choose an analytics option:
   ```
   📊 ANALYTICS MENU
   ============================================================
     A. Clinic Statistics
     B. Veterinarian Workload Analysis
     C. Pet Medical Profiles
     D. Search Appointments (Advanced)
     E. Back to Main Menu
   ```

#### Option A: Clinic Statistics

Shows clinic-wide metrics:
```
Clinic Statistics:
• Total Appointments: 25
• Completed: 18 (72%)
• Cancelled: 2 (8%)
• Scheduled: 5 (20%)

🏆 Most Active Veterinarians:
  1. Dr. Sarah Johnson: 12 appointments
  2. Dr. Mike Chen: 8 appointments
  3. Dr. Lisa Park: 5 appointments

🐾 Most Visited Pets:
  1. Buddy (Dog): 5 appointments
  2. Whiskers (Cat): 4 appointments

📋 Most Common Reasons:
  1. Annual checkup: 8
  2. Vaccination: 6
  3. Dental cleaning: 4
```

#### Option B: Veterinarian Workload Analysis

Shows per-veterinarian metrics:
```
Dr. Sarah Johnson Statistics:
• Total Appointments: 12
• Completed: 9
• Average per Month: 3.2
• Utilization Rate: 75%

🔍 Top Diagnoses Treated:
  1. Ear infection: 3
  2. Dental disease: 2
```

#### Option C: Pet Medical Profiles

Shows per-pet medical summary:
```
Buddy (Dog) Profile:
• Total Appointments: 5
• Completed: 4
• Last Visit: 2024-12-10

Diagnoses History:
  1. Ear infection
  2. Skin allergy
  3. Normal health check
```

#### Option D: Search Appointments (Advanced)

Search by multiple criteria:
```
Advanced Appointment Search:
Enter owner name (leave blank to skip): John
Enter pet name (leave blank to skip): 
Select status (Scheduled/Completed/Cancelled/All): Completed
Enter start date (yyyy-MM-dd, leave blank to skip): 2024-01-01
Enter end date (yyyy-MM-dd, leave blank to skip): 2024-12-31

Results:
Appointment ID: 1 | Pet: Buddy | Status: Completed
  Date: 2024-12-10 10:00 | Reason: Annual checkup
```

---

### 8. Save Data to File

**Purpose**: Manually save all data to a JSON file for backup or data export.

**Steps**:
1. Select **Option 8** from the main menu
2. Application automatically saves all data to `appointments.json`

**Example Output**:
```
✅ Data saved successfully!
   File: appointments.json
   Size: 4.2 KB
   Entities: 25 appointments, 18 medical records
```

**Data Saved**:
- All appointments
- All medical records
- Pet information
- Owner information
- Veterinarian information

---

### 9. Exit

**Purpose**: Close the application while saving all data.

**Steps**:
1. Select **Option 9** from the main menu
2. Application saves all data before closing
3. Goodbye message is displayed

---

## Tips & Best Practices

### Best Practices

1. **Regular Backups**:
   - Use Option 8 regularly to save data
   - Data automatically saves on exit
   - Create manual backups of `appointments.json` file

2. **Data Entry**:
   - Dates must be in `yyyy-MM-dd` format
   - Times must be in `HH:mm` format (24-hour)
   - Appointment times cannot be in the past
   - Keep diagnosis and treatment descriptions detailed but concise

3. **Managing Appointments**:
   - Always check vet availability before scheduling
   - Complete appointments promptly to create medical records
   - Use medical history for follow-up diagnoses

4. **Using Analytics**:
   - Check clinic statistics monthly for workload distribution
   - Monitor veterinarian utilization to prevent burnout
   - Review pet medical profiles for chronic conditions
   - Use search to find specific appointments quickly

### Troubleshooting

| Issue | Solution |
|-------|----------|
| "Invalid pet selection" | Ensure you select a valid number from the list |
| "Veterinarian is not available" | Choose a different time for the appointment |
| "Cannot create medical record" | Appointment must be marked as "Completed" first |
| "Diagnosis is too long" | Maximum 500 characters - shorten your text |
| "Data not saving" | Check file permissions in the application directory |
| "Date format error" | Use format `yyyy-MM-dd` (e.g., `2024-12-25`) |

---

## Data Privacy & Backup

### Data Storage

- All data is stored in `appointments.json` file in the application directory
- No data is transmitted to external servers
- Data is stored on your local machine only

### Backing Up Your Data

```bash
# Manual backup (copy the JSON file)
cp appointments.json appointments.backup.json

# Or use the application's save feature (Option 8)
```

### Restoring from Backup

```bash
# Replace the current file with your backup
cp appointments.backup.json appointments.json
```

---

## Support & FAQ

### How do I reset the sample data?

The application loads demo data on first run. To reset:
```bash
# Delete the JSON file
rm appointments.json

# Restart the application
dotnet run --project src/VetClinic.Console
```

### Can I export data to CSV or PDF?

Currently, data can be exported as JSON only. For alternative formats, use the DEVELOPER_GUIDE.md to extend the application.

### How many appointments can the system handle?

The current version works well with up to 10,000 appointments. For larger datasets, consult DEVELOPER_GUIDE.md for database integration recommendations.

### Can multiple users use the application simultaneously?

No, the current version stores data in a single JSON file. For multi-user support, see DEVELOPER_GUIDE.md for database migration guidance.

---

## Next Steps

For developers interested in extending VetClinic, see [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md).

For technical details and architecture, see [docs/release-plan.md](docs/release-plan.md).

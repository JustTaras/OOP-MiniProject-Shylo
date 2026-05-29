namespace VetClinic.Application;

using VetClinic.Domain;

/// <summary>
/// Analytics and reporting service (Lab 35)
/// Provides LINQ-based queries for clinic statistics and analytics
/// </summary>
public class AnalyticsService
{
    private readonly IAppointmentRepository _repository;

    public AnalyticsService(IAppointmentRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    // LINQ Query 1: Veterinarian statistics
    /// <summary>
    /// Get comprehensive statistics for a veterinarian including workload analysis and recent activity.
    /// 
    /// LINQ Techniques Demonstrated:
    /// - Where: Filter by veterinarian and appointment status
    /// - GroupBy: Aggregate appointments by year-month
    /// - Count: Tally appointments by status
    /// - Average: Calculate average appointments per month
    /// - OrderByDescending/Take: Get most common diagnoses
    /// 
    /// Business Rule: Statistics calculated only from completed appointments
    /// </summary>
    /// <param name="vet">The veterinarian to analyze</param>
    /// <returns>Comprehensive statistics including appointment counts, trends, and diagnoses</returns>
    /// <exception cref="ArgumentNullException">Thrown if vet is null</exception>
    public VeterinarianStatistics GetVeterinarianStatistics(Veterinarian vet)
    {
        if (vet == null)
            throw new ArgumentNullException(nameof(vet));

        // Get all appointments for this veterinarian
        var allAppointments = _repository.GetByVeterinarian(vet);
        
        // Filter to only completed appointments for statistical analysis
        var completedAppointments = allAppointments
            .Where(a => a.Status == AppointmentStatus.Completed)
            .ToList();

        // Count appointments by status for dashboard display
        var totalAppointments = allAppointments.Count;
        var completedCount = completedAppointments.Count;
        var cancelledCount = allAppointments.Count(a => a.Status == AppointmentStatus.Cancelled);
        var upcomingCount = allAppointments.Count(a => a.Status == AppointmentStatus.Scheduled);

        // Calculate average workload: group completed appointments by year-month, 
        // count appointments in each month, then average (useful for capacity planning)
        var appointmentsByMonth = completedAppointments.Count > 0
            ? completedAppointments
                .GroupBy(a => new { a.AppointmentDateTime.Year, a.AppointmentDateTime.Month })
                .Select(g => g.Count())
                .Average()
            : 0;

        // Get most common diagnoses for this vet (cross-reference appointments with medical records)
        // This requires nested lookup since MedicalRecord links to Appointment, not directly to Vet
        var medicalRecords = _repository.GetAllMedicalRecords()
            .Where(r => 
            {
                var apt = _repository.GetById(r.AppointmentId);
                return apt != null && apt.Veterinarian.Id == vet.Id;
            })
            .ToList();

        // Group diagnoses by text, count occurrences, get top 5 most common
        // (useful for identifying specialist focus areas)
        var mostCommonDiagnoses = medicalRecords
            .GroupBy(r => r.Diagnosis)
            .OrderByDescending(g => g.Count())
            .Select(g => new { Diagnosis = g.Key, Count = g.Count() })
            .Take(5)
            .ToList();

        // Get recent activity: sort completed appointments by most recent, take last 5
        // (useful for status overview and recent workload assessment)
        var recentAppointments = completedAppointments
            .OrderByDescending(a => a.AppointmentDateTime)
            .Take(5)
            .ToList();

        return new VeterinarianStatistics
        {
            Veterinarian = vet,
            TotalAppointments = totalAppointments,
            CompletedAppointments = completedCount,
            CancelledAppointments = cancelledCount,
            UpcomingAppointments = upcomingCount,
            AverageAppointmentsPerMonth = Math.Round(appointmentsByMonth, 2),
            MostCommonDiagnoses = mostCommonDiagnoses.Cast<object>().ToList(),
            RecentCompletedAppointments = recentAppointments
        };
    }

    // LINQ Query 2: Pet medical history with filtering
    /// <summary>
    /// Get detailed pet medical profile including all treatments and diagnosis trends.
    /// 
    /// LINQ Techniques Demonstrated:
    /// - Where: Filter completed appointments and medical records by pet
    /// - OrderByDescending: Chronologically sort appointments and records
    /// - Select: Project diagnoses from records
    /// - Distinct: Get unique diagnoses
    /// - GroupBy: Count diagnosis frequency
    /// 
    /// </summary>
    /// <param name="pet">The pet to profile</param>
    /// <returns>Complete medical profile with appointment history and diagnosis trends</returns>
    /// <exception cref="ArgumentNullException">Thrown if pet is null</exception>
    public PetMedicalProfile GetPetMedicalProfile(Pet pet)
    {
        if (pet == null)
            throw new ArgumentNullException(nameof(pet));

        var allAppointments = _repository.GetByPet(pet);
        var completedAppointments = allAppointments
            .Where(a => a.Status == AppointmentStatus.Completed)
            .OrderByDescending(a => a.AppointmentDateTime)
            .ToList();

        var medicalRecords = _repository.GetAllMedicalRecords()
            .Where(r => r.Pet.Id == pet.Id)
            .OrderByDescending(r => r.VisitDate)
            .ToList();

        // Get all unique diagnoses for this pet
        var diagnoses = medicalRecords
            .Select(r => r.Diagnosis)
            .Distinct()
            .ToList();

        // Get diagnosis frequency (for analytics)
        var diagnosisFrequency = medicalRecords
            .GroupBy(r => r.Diagnosis)
            .Select(g => new { Diagnosis = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToList();

        var lastVisitDate = medicalRecords.FirstOrDefault()?.VisitDate ?? DateTime.MinValue;
        var totalVisits = completedAppointments.Count;

        return new PetMedicalProfile
        {
            Pet = pet,
            TotalAppointments = allAppointments.Count,
            CompletedAppointments = completedAppointments.Count,
            UniqueDiagnoses = diagnoses,
            DiagnosisFrequency = diagnosisFrequency.Cast<object>().ToList(),
            LastVisitDate = lastVisitDate,
            MedicalRecords = medicalRecords
        };
    }

    // LINQ Query 3: Search appointments by multiple criteria
    /// <summary>
    /// Advanced search for appointments using flexible multi-criteria filtering.
    /// All parameters are optional; only provided filters are applied.
    /// 
    /// LINQ Techniques Demonstrated:
    /// - AsEnumerable: LINQ-to-Objects evaluation
    /// - Where: Chain multiple conditional filters
    /// - StringComparison.OrdinalIgnoreCase: Case-insensitive matching
    /// - OrderByDescending: Sort by most recent appointments first
    /// 
    /// Performance Note: AsEnumerable is intentional here to allow dynamic predicate chaining.
    /// For large datasets (10K+ records), consider pagination or indexed database queries.
    /// </summary>
    /// <param name="ownerName">Optional: Filter by owner name (partial match, case-insensitive)</param>
    /// <param name="petName">Optional: Filter by pet name (partial match, case-insensitive)</param>
    /// <param name="status">Optional: Filter by appointment status</param>
    /// <param name="dateFrom">Optional: Filter appointments on or after this date</param>
    /// <param name="dateTo">Optional: Filter appointments on or before this date</param>
    /// <returns>Filtered and sorted list of appointments</returns>
    public IReadOnlyList<Appointment> SearchAppointments(
        string? ownerName = null,
        string? petName = null,
        AppointmentStatus? status = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = _repository.GetAll().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(ownerName))
        {
            query = query.Where(a => a.Pet.Owner.Name.Contains(ownerName, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(petName))
        {
            query = query.Where(a => a.Pet.Name.Contains(petName, StringComparison.OrdinalIgnoreCase));
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(a => a.AppointmentDateTime >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(a => a.AppointmentDateTime <= dateTo.Value);
        }

        return query
            .OrderByDescending(a => a.AppointmentDateTime)
            .ToList()
            .AsReadOnly();
    }

    // LINQ Query 4: Aggregate statistics
    /// <summary>
    /// Get clinic-wide statistics aggregating all appointments and patterns.
    /// Provides comprehensive view of clinic operations for reporting and analysis.
    /// 
    /// LINQ Techniques Demonstrated:
    /// - GroupBy: Aggregate by veterinarian, pet, appointment reason, and time period
    /// - Count/Where: Calculate completion and cancellation rates
    /// - OrderByDescending/Take: Extract top items (busiest vets, most visited pets)
    /// - Select with calculated fields: Compute percentages and aggregations
    /// 
    /// Performance Note: This query iterates the appointment collection 4 times.
    /// For datasets > 100K appointments, cache results or optimize with database views.
    /// </summary>
    /// <returns>Complete clinic statistics including rates, trends, and rankings</returns>
    public ClinicStatistics GetClinicStatistics()
    {
        // Partition appointments by status for independent analysis
        var allAppointments = _repository.GetAll();
        var completedAppointments = allAppointments.Where(a => a.Status == AppointmentStatus.Completed).ToList();
        var cancelledAppointments = allAppointments.Where(a => a.Status == AppointmentStatus.Cancelled).ToList();
        var scheduledAppointments = allAppointments.Where(a => a.Status == AppointmentStatus.Scheduled).ToList();

        // Ranking 1: Group appointments by veterinarian, count per vet, get top 10
        // Useful for identifying workload distribution and staffing needs
        var vetStats = allAppointments
            .GroupBy(a => a.Veterinarian)
            .Select(g => new { Vet = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(10)
            .ToList();

        // Ranking 2: Group appointments by pet, count visits per pet, get top 10
        // Identifies frequent visitors and repeat customers (loyalty indicator)
        var petStats = allAppointments
            .GroupBy(a => a.Pet)
            .Select(g => new { Pet = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(10)
            .ToList();

        // Ranking 3: Group appointments by reason/complaint, count per reason, get top 10
        // Shows most common health issues and guides resource allocation for supplies/expertise
        var reasonStats = allAppointments
            .GroupBy(a => a.Reason)
            .Select(g => new { Reason = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(10)
            .ToList();

        // Trend analysis: Group completed appointments by month (last 12 months)
        // Format as "YYYY-MM" for easy reading and graphing
        var appointmentsByMonth = completedAppointments
            .Where(a => a.AppointmentDateTime >= DateTime.Now.AddMonths(-12))
            .GroupBy(a => new { a.AppointmentDateTime.Year, a.AppointmentDateTime.Month })
            .Select(g => new { YearMonth = $"{g.Key.Year}-{g.Key.Month:D2}", Count = g.Count() })
            .OrderBy(g => g.YearMonth)
            .ToList();

        return new ClinicStatistics
        {
            TotalAppointments = allAppointments.Count,
            CompletedAppointments = completedAppointments.Count,
            CancelledAppointments = cancelledAppointments.Count,
            ScheduledAppointments = scheduledAppointments.Count,
            CompletionRate = allAppointments.Count > 0 
                ? Math.Round((double)completedAppointments.Count / allAppointments.Count * 100, 2)
                : 0,
            CancellationRate = allAppointments.Count > 0
                ? Math.Round((double)cancelledAppointments.Count / allAppointments.Count * 100, 2)
                : 0,
            MostActiveVeterinarians = vetStats.Cast<object>().ToList(),
            MostVisitedPets = petStats.Cast<object>().ToList(),
            MostCommonReasons = reasonStats.Cast<object>().ToList(),
            AppointmentsByMonth = appointmentsByMonth.Cast<object>().ToList()
        };
    }

    // LINQ Query 5: Utilization analysis
    /// <summary>
    /// Analyze veterinarian utilization rates showing engagement and availability patterns.
    /// Calculates completion ratio for each veterinarian to assess workload and efficiency.
    /// 
    /// LINQ Techniques Demonstrated:
    /// - HashSet: Eliminate duplicate veterinarians from appointments
    /// - Select: Transform vet references to detailed utilization metrics
    /// - Count with predicate: Calculate conditional statistics
    /// - OrderByDescending: Sort by highest utilization first
    /// 
    /// Business Insight: 100% utilization = all assigned appointments completed (no cancellations).
    /// Low utilization may indicate understaffing or cancellations; high utilization indicates efficiency.
    /// </summary>
    /// <returns>Utilization metrics for all veterinarians, ordered by completion rate</returns>
    public IReadOnlyList<VeterinarianUtilization> GetVeterinarianUtilization()
    {
        var vets = new HashSet<Veterinarian>(
            _repository.GetAll().Select(a => a.Veterinarian));

        var utilization = vets
            .Select(vet =>
            {
                var appointments = _repository.GetByVeterinarian(vet);
                var completed = appointments.Count(a => a.Status == AppointmentStatus.Completed);
                var total = appointments.Count;
                var utilizationRate = total > 0 ? Math.Round((double)completed / total * 100, 2) : 0;

                return new VeterinarianUtilization
                {
                    Veterinarian = vet,
                    TotalAppointments = total,
                    CompletedAppointments = completed,
                    UtilizationRate = utilizationRate
                };
            })
            .OrderByDescending(v => v.UtilizationRate)
            .ToList()
            .AsReadOnly();

        return utilization;
    }
}

// ============= STATISTICS DATA CLASSES =============

/// <summary>
/// Veterinarian statistics for reporting
/// </summary>
public class VeterinarianStatistics
{
    public Veterinarian Veterinarian { get; set; } = null!;
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public double AverageAppointmentsPerMonth { get; set; }
    public List<object> MostCommonDiagnoses { get; set; } = [];
    public List<Appointment> RecentCompletedAppointments { get; set; } = [];
}

/// <summary>
/// Pet medical profile
/// </summary>
public class PetMedicalProfile
{
    public Pet Pet { get; set; } = null!;
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public List<string> UniqueDiagnoses { get; set; } = [];
    public List<object> DiagnosisFrequency { get; set; } = [];
    public DateTime LastVisitDate { get; set; }
    public IReadOnlyList<MedicalRecord> MedicalRecords { get; set; } = null!;
}

/// <summary>
/// Clinic-wide statistics
/// </summary>
public class ClinicStatistics
{
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int ScheduledAppointments { get; set; }
    public double CompletionRate { get; set; }
    public double CancellationRate { get; set; }
    public List<object> MostActiveVeterinarians { get; set; } = [];
    public List<object> MostVisitedPets { get; set; } = [];
    public List<object> MostCommonReasons { get; set; } = [];
    public List<object> AppointmentsByMonth { get; set; } = [];
}

/// <summary>
/// Veterinarian utilization metrics
/// </summary>
public class VeterinarianUtilization
{
    public Veterinarian Veterinarian { get; set; } = null!;
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public double UtilizationRate { get; set; }
}

namespace VetClinic.Application.Strategies;

using VetClinic.Domain;

/// <summary>
/// Strategy pattern interface for calculating diagnosis severity
/// Allows different scoring algorithms to be plugged in without modifying existing code
/// Lab 35: Demonstrates extensibility without modifying core business logic
/// </summary>
public interface IDiagnosisSeverityScorer
{
    /// <summary>
    /// Calculate severity score for a diagnosis (0-100)
    /// </summary>
    /// <param name="diagnosis">The diagnosis description</param>
    /// <returns>Severity score from 0 (no concern) to 100 (critical)</returns>
    int CalculateSeverityScore(string diagnosis);

    /// <summary>
    /// Get human-readable severity level
    /// </summary>
    string GetSeverityLevel(int score);

    /// <summary>
    /// Get description of the scoring strategy
    /// </summary>
    string GetDescription();
}

/// <summary>
/// Default strategy: Basic keyword-based diagnosis severity scoring
/// Looks for common keywords in diagnosis to estimate severity
/// </summary>
public class KeywordBasedSeverityScorer : IDiagnosisSeverityScorer
{
    private static readonly Dictionary<string, int> SeverityKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        // Critical conditions (80-100)
        { "fracture", 85 },
        { "hemorrhage", 90 },
        { "trauma", 80 },
        { "sepsis", 95 },
        { "emergency", 90 },
        { "critical", 95 },
        { "severe", 80 },
        
        // Serious conditions (60-79)
        { "infection", 70 },
        { "inflammation", 65 },
        { "surgery", 75 },
        { "disease", 65 },
        { "cancer", 85 },
        { "tumor", 80 },
        { "paralysis", 85 },
        
        // Moderate conditions (40-59)
        { "allergic", 45 },
        { "allergy", 45 },
        { "sprain", 50 },
        { "strain", 50 },
        { "dermatitis", 40 },
        { "arthritis", 55 },
        { "ulcer", 60 },
        
        // Minor conditions (1-39)
        { "scratch", 15 },
        { "cut", 20 },
        { "minor", 20 },
        { "mild", 25 },
        { "rash", 30 },
        { "itch", 20 }
    };

    public int CalculateSeverityScore(string diagnosis)
    {
        if (string.IsNullOrWhiteSpace(diagnosis))
            return 0;

        int maxScore = 0;

        // Find highest-severity keyword in diagnosis
        foreach (var keyword in SeverityKeywords.Keys)
        {
            if (diagnosis.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                maxScore = Math.Max(maxScore, SeverityKeywords[keyword]);
            }
        }

        // If no keywords matched, assign moderate score (not diagnosed, so safe)
        return maxScore > 0 ? maxScore : 50;
    }

    public string GetSeverityLevel(int score)
    {
        return score switch
        {
            >= 80 => "CRITICAL",
            >= 60 => "SERIOUS",
            >= 40 => "MODERATE",
            >= 20 => "MINOR",
            _ => "NEGLIGIBLE"
        };
    }

    public string GetDescription()
    {
        return "Keyword-based severity scoring - analyzes diagnosis text for severity indicators";
    }
}

/// <summary>
/// Alternative strategy: Length-based severity scoring
/// Assumes longer, more detailed diagnoses indicate more complex/severe conditions
/// </summary>
public class LengthBasedSeverityScorer : IDiagnosisSeverityScorer
{
    public int CalculateSeverityScore(string diagnosis)
    {
        if (string.IsNullOrWhiteSpace(diagnosis))
            return 0;

        // Simple heuristic: longer diagnosis = more complex
        // 0-50 chars = minor (20), 50-150 chars = moderate (50), 150+ chars = serious (80)
        int length = diagnosis.Length;

        return length switch
        {
            < 50 => 20,
            < 150 => 50,
            _ => 80
        };
    }

    public string GetSeverityLevel(int score)
    {
        return score switch
        {
            >= 80 => "SERIOUS",
            >= 50 => "MODERATE",
            _ => "MINOR"
        };
    }

    public string GetDescription()
    {
        return "Length-based severity scoring - assumes longer diagnoses indicate more complex conditions";
    }
}

/// <summary>
/// Service that uses diagnosis severity scoring strategy
/// Allows swapping scoring algorithms at runtime
/// </summary>
public class DiagnosisAnalysisService
{
    private IDiagnosisSeverityScorer _severityScorer;

    public DiagnosisAnalysisService(IDiagnosisSeverityScorer? severityScorer = null)
    {
        _severityScorer = severityScorer ?? new KeywordBasedSeverityScorer();
    }

    /// <summary>
    /// Change scoring strategy at runtime
    /// Demonstrates the Strategy pattern's flexibility
    /// </summary>
    public void SetSeverityScorer(IDiagnosisSeverityScorer scorer)
    {
        _severityScorer = scorer ?? throw new ArgumentNullException(nameof(scorer));
    }

    /// <summary>
    /// Analyze a diagnosis using current strategy
    /// </summary>
    public DiagnosisAnalysis AnalyzeDiagnosis(string diagnosis)
    {
        if (string.IsNullOrWhiteSpace(diagnosis))
            throw new ArgumentException("Diagnosis cannot be empty", nameof(diagnosis));

        int score = _severityScorer.CalculateSeverityScore(diagnosis);
        string level = _severityScorer.GetSeverityLevel(score);

        return new DiagnosisAnalysis
        {
            Diagnosis = diagnosis,
            SeverityScore = score,
            SeverityLevel = level,
            ScoringStrategy = _severityScorer.GetDescription()
        };
    }

    /// <summary>
    /// Get current scorer description
    /// </summary>
    public string GetCurrentScorerDescription()
    {
        return _severityScorer.GetDescription();
    }
}

/// <summary>
/// Result of diagnosis analysis
/// </summary>
public class DiagnosisAnalysis
{
    public string Diagnosis { get; set; } = string.Empty;
    public int SeverityScore { get; set; }
    public string SeverityLevel { get; set; } = string.Empty;
    public string ScoringStrategy { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Diagnosis: {Diagnosis}\n" +
               $"  Severity: {SeverityLevel} (Score: {SeverityScore}/100)\n" +
               $"  Scoring: {ScoringStrategy}";
    }
}

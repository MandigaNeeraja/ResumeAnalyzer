namespace ResumeAnalyzer.DTOs.Settings
{
    public class SettingsResponseDto
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Organization { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool EmailNewResumes { get; set; }

        public bool NotifyMatchComplete { get; set; }

        public bool WeeklyAnalyticsReport { get; set; }

        public int MinMatchScore { get; set; }
    }

    public class UpdateProfileDto
    {
        public string Name { get; set; } = string.Empty;

        public string Organization { get; set; } = string.Empty;
    }

    public class UpdatePreferencesDto
    {
        public bool EmailNewResumes { get; set; }

        public bool NotifyMatchComplete { get; set; }

        public bool WeeklyAnalyticsReport { get; set; }

        public int MinMatchScore { get; set; }
    }
}

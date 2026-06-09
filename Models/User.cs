namespace ResumeAnalyzer.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "HR";

        public string Organization { get; set; } = string.Empty;

        public bool EmailNewResumes { get; set; } = true;

        public bool NotifyMatchComplete { get; set; } = true;

        public bool WeeklyAnalyticsReport { get; set; }

        public int MinMatchScore { get; set; } = 60;
    }
}
namespace ResumeAnalyzer.DTOs.AI
{
    public class ResumeParseResponse
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Education { get; set; } = string.Empty;

        public string CurrentDesignation { get; set; } = string.Empty;

        public int ExperienceYears { get; set; }

        public List<string> Skills { get; set; } = new();
    }
}
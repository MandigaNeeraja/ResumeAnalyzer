namespace ResumeAnalyzer.DTOs.Candidate
{
    public class CandidateResponseDto
    {
        public int CandidateId { get; set; }
        public int? JobId { get; set; }
        public string? JobTitle { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? LinkedIn { get; set; }
        public string Experience { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public double? ATSScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? HRRemarks { get; set; }
        public string? ManagerRemarks { get; set; }
        public List<AvailabilitySlotDto> ManagerAvailability { get; set; } = new();
        public DateTime CreatedDate { get; set; }
    }
}

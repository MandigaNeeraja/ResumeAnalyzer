namespace ResumeAnalyzer.DTOs.Candidate
{
    public class CandidateWorkflowDto
    {
        public string? Remarks { get; set; }

        public List<AvailabilitySlotDto>? AvailabilitySlots { get; set; }
    }
}

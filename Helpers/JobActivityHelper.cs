using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.Enums;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Helpers
{
    public static class JobActivityHelper
    {
        public static async Task LogAsync(
            AppDbContext context,
            int jobId,
            JobActivityType type,
            string description,
            int? performedBy = null,
            int? candidateId = null,
            int? interviewId = null)
        {
            context.JobActivityLogs.Add(new JobActivityLog
            {
                JobId = jobId,
                ActivityType = type,
                Description = description,
                PerformedBy = performedBy,
                RelatedCandidateId = candidateId,
                RelatedInterviewId = interviewId,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }

        public static string FormatActivityType(JobActivityType type) => type switch
        {
            JobActivityType.JobCreated => "Job Created",
            JobActivityType.JobUpdated => "Job Updated",
            JobActivityType.ResumeUploaded => "Resume Uploaded",
            JobActivityType.CandidateShortlisted => "Candidate Shortlisted",
            JobActivityType.InterviewScheduled => "Interview Scheduled",
            JobActivityType.JobClosed => "Job Closed",
            JobActivityType.JobReopened => "Job Reopened",
            JobActivityType.JobPutOnHold => "Job Put On Hold",
            JobActivityType.CandidateRejected => "Candidate Rejected",
            JobActivityType.CandidateOnHold => "Candidate On Hold",
            JobActivityType.SentToManager => "Sent To Manager",
            _ => type.ToString()
        };
    }
}

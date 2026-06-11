using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Candidate> Candidates => Set<Candidate>();
        public DbSet<Resume> Resumes => Set<Resume>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<JobSkill> JobSkills => Set<JobSkill>();
        public DbSet<User> Users => Set<User>();
        public DbSet<CandidateJobMatch> CandidateJobMatches => Set<CandidateJobMatch>();
        public DbSet<Interview> Interviews => Set<Interview>();
        public DbSet<InterviewFeedback> InterviewFeedbacks => Set<InterviewFeedback>();
        public DbSet<JobActivityLog> JobActivityLogs => Set<JobActivityLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CandidateSkill>()
                .HasKey(cs => new { cs.CandidateId, cs.SkillId });

            modelBuilder.Entity<JobSkill>()
                .HasKey(js => new { js.JobId, js.SkillId });

            modelBuilder.Entity<Candidate>()
                .HasOne(c => c.Job)
                .WithMany(j => j.Candidates)
                .HasForeignKey(c => c.JobId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CandidateJobMatch>()
                .HasOne(m => m.Candidate)
                .WithMany()
                .HasForeignKey(m => m.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CandidateJobMatch>()
                .HasOne(m => m.Job)
                .WithMany()
                .HasForeignKey(m => m.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Job>()
                .HasOne(j => j.Creator)
                .WithMany()
                .HasForeignKey(j => j.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Interview>()
                .HasOne(i => i.Candidate)
                .WithMany(c => c.Interviews)
                .HasForeignKey(i => i.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Interview>()
                .HasOne(i => i.Job)
                .WithMany(j => j.Interviews)
                .HasForeignKey(i => i.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Interview>()
                .HasOne(i => i.Scheduler)
                .WithMany()
                .HasForeignKey(i => i.ScheduledBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<InterviewFeedback>()
                .HasKey(f => f.FeedbackId);

            modelBuilder.Entity<Interview>()
                .HasOne(i => i.Feedback)
                .WithOne(f => f.Interview)
                .HasForeignKey<InterviewFeedback>(f => f.InterviewId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InterviewFeedback>()
                .HasOne(f => f.Candidate)
                .WithMany(c => c.InterviewFeedbacks)
                .HasForeignKey(f => f.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InterviewFeedback>()
                .HasOne(f => f.Manager)
                .WithMany()
                .HasForeignKey(f => f.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<JobActivityLog>()
                .HasOne(a => a.Job)
                .WithMany(j => j.ActivityLogs)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobActivityLog>()
                .HasOne(a => a.Performer)
                .WithMany()
                .HasForeignKey(a => a.PerformedBy)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

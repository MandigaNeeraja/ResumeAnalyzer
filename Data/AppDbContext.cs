//using Microsoft.EntityFrameworkCore;
//using ResumeAnalyzer.Models;
//using static System.Runtime.InteropServices.JavaScript.JSType;


//namespace ResumeAnalyzer.Data
//{
//    public class AppDbContext : DbContext
//    {
//        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
//        {
//        }
//        //Why use => Set<T>() instead of public DbSet<T> Property { get; set; }?
//        //No backing field needed — it directly calls Set<T>() from the base DbContext.
//        //Read-only property — prevents accidental reassignment of the DbSet.
//        //Cleaner code — avoids boilerplate.
//        public DbSet<Candidate> Candidates => Set<Candidate>();
//        public DbSet<Resume> Resumes => Set<Resume>();
//        public DbSet<Skill> Skills => Set<Skill>();
//        public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
//        public DbSet<Job> Jobs => Set<Job>();


//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            //Candidate(1)<->(1) Resume
//            modelBuilder.Entity<Candidate>()
//                .HasOne(c => c.Resume)
//                .WithOne(r => r.Candidate)
//                .HasForeignKey<Resume>(r => r.CandidateId);

//            //Candidate(n)<->(n) Skill via CandidateSkill

//            modelBuilder.Entity<CandidateSkill>()
//                .HasKey(cs => new { cs.CandidateId, cs.SkillId });

//            modelBuilder.Entity<CandidateSkill>()
//                .HasOne(cs => cs.Candidate)
//                .WithMany(c => c.CandidateSkills)
//                .HasForeignKey(cs => cs.CandidateId);

//            modelBuilder.Entity<CandidateSkill>()
//                .HasOne(cs => cs.Skill)
//                .WithMany(s => s.CandidateSkills)
//                .HasForeignKey(cs => cs.SkillId);


//        }
//    }
//}
using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
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

        public DbSet<CandidateJobMatch> CandidateJobMatches
            => Set<CandidateJobMatch>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CandidateSkill>()
                .HasKey(cs => new
                {
                    cs.CandidateId,
                    cs.SkillId
                });

            modelBuilder.Entity<JobSkill>()
                .HasKey(js => new
                {
                    js.JobId,
                    js.SkillId
                });
        }
    }
}
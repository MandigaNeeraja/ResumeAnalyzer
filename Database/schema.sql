-- Resume Analyzer Recruitment Management System Schema
-- SQL Server

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(MAX) NOT NULL,
    Email NVARCHAR(450) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role NVARCHAR(MAX) NOT NULL DEFAULT 'HR',
    Organization NVARCHAR(MAX) NOT NULL DEFAULT '',
    EmailNewResumes BIT NOT NULL DEFAULT 1,
    NotifyMatchComplete BIT NOT NULL DEFAULT 1,
    WeeklyAnalyticsReport BIT NOT NULL DEFAULT 0,
    MinMatchScore INT NOT NULL DEFAULT 60
);

CREATE TABLE Jobs (
    JobId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(MAX) NOT NULL,
    Designation NVARCHAR(MAX) NOT NULL DEFAULT '',
    Department NVARCHAR(MAX) NOT NULL DEFAULT '',
    Location NVARCHAR(MAX) NOT NULL DEFAULT '',
    EmploymentType NVARCHAR(MAX) NOT NULL DEFAULT '',
    Experience NVARCHAR(MAX) NOT NULL DEFAULT '',
    Description NVARCHAR(MAX) NOT NULL,
    CreatedBy INT NULL REFERENCES Users(UserId) ON DELETE SET NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Skills (
    SkillId INT IDENTITY(1,1) PRIMARY KEY,
    SkillName NVARCHAR(MAX) NOT NULL
);

CREATE TABLE JobSkills (
    JobId INT NOT NULL REFERENCES Jobs(JobId) ON DELETE CASCADE,
    SkillId INT NOT NULL REFERENCES Skills(SkillId) ON DELETE CASCADE,
    PRIMARY KEY (JobId, SkillId)
);

CREATE TABLE Candidates (
    CandidateId INT IDENTITY(1,1) PRIMARY KEY,
    JobId INT NULL REFERENCES Jobs(JobId) ON DELETE SET NULL,
    FullName NVARCHAR(MAX) NOT NULL,
    Email NVARCHAR(MAX) NULL,
    Phone NVARCHAR(MAX) NOT NULL DEFAULT '',
    LinkedIn NVARCHAR(MAX) NULL,
    ExperienceYears INT NULL,
    Education NVARCHAR(MAX) NULL,
    CurrentDesignation NVARCHAR(MAX) NULL,
    ATSScore FLOAT NULL,
    Status INT NOT NULL DEFAULT 0,
    HRRemarks NVARCHAR(MAX) NULL,
    ManagerRemarks NVARCHAR(MAX) NULL,
    ManagerAvailability NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Resumes (
    ResumeId INT IDENTITY(1,1) PRIMARY KEY,
    CandidateId INT NULL UNIQUE REFERENCES Candidates(CandidateId) ON DELETE CASCADE,
    FileName NVARCHAR(MAX) NOT NULL,
    FilePath NVARCHAR(MAX) NOT NULL,
    ExtractedText NVARCHAR(MAX) NULL,
    ParsedAt DATETIME2 NOT NULL,
    ParseStatus NVARCHAR(MAX) NOT NULL,
    ParseErrorMessage NVARCHAR(MAX) NULL
);

CREATE TABLE CandidateSkills (
    CandidateId INT NOT NULL REFERENCES Candidates(CandidateId) ON DELETE CASCADE,
    SkillId INT NOT NULL REFERENCES Skills(SkillId) ON DELETE CASCADE,
    Source NVARCHAR(MAX) NOT NULL DEFAULT 'Resume',
    PRIMARY KEY (CandidateId, SkillId)
);

CREATE TABLE CandidateJobMatches (
    CandidateJobMatchId INT IDENTITY(1,1) PRIMARY KEY,
    CandidateId INT NOT NULL REFERENCES Candidates(CandidateId) ON DELETE CASCADE,
    JobId INT NOT NULL REFERENCES Jobs(JobId) ON DELETE CASCADE,
    ATSScore FLOAT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Interviews (
    InterviewId INT IDENTITY(1,1) PRIMARY KEY,
    CandidateId INT NOT NULL REFERENCES Candidates(CandidateId) ON DELETE CASCADE,
    JobId INT NOT NULL REFERENCES Jobs(JobId),
    InterviewDate DATETIME2 NOT NULL,
    InterviewTime TIME NOT NULL,
    InterviewType INT NOT NULL,
    MeetingLink NVARCHAR(MAX) NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    ScheduledBy INT NULL REFERENCES Users(UserId) ON DELETE SET NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE InterviewFeedbacks (
    FeedbackId INT IDENTITY(1,1) PRIMARY KEY,
    InterviewId INT NOT NULL UNIQUE REFERENCES Interviews(InterviewId) ON DELETE CASCADE,
    CandidateId INT NOT NULL REFERENCES Candidates(CandidateId),
    ManagerId INT NOT NULL REFERENCES Users(UserId),
    TechnicalKnowledgeRating INT NOT NULL,
    ProblemSolvingRating INT NOT NULL,
    CommunicationRating INT NOT NULL,
    Comments NVARCHAR(MAX) NOT NULL,
    Decision INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- CandidateStatus: 0=Applied, 1=HRScreening, 2=SentToManager, 3=InterviewScheduled,
--                  4=InterviewCompleted, 5=TechnicalSelected, 6=TechnicalRejected, 7=Hired, 8=Rejected
-- InterviewType: 0=HRRound, 1=TechnicalRound, 2=FinalRound
-- InterviewStatus: 0=Scheduled, 1=Completed, 2=Cancelled, 3=Rescheduled
-- FeedbackDecision: 0=Selected, 1=Rejected

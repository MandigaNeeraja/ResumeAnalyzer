namespace ResumeAnalyzer.Services
{
    public class SkillExtractionService
    {
        private readonly List<string> _masterSkills =
        [
            "C#",
            "C++11",
            ".NET",
            "ASP.NET",
            "ASP.NET Core",
            "React",
            "Angular",
            "Java",
            "Spring Boot",
            "C++",
            "C++11",
            "C++14",
            "C++17",
            "Linux",
            "STL",
            "Kafka",
            "SQL",
            "Network Programming",
            "Multithreading",
            "IPC Programming",
            "System Programming",
            "Python",
            "SQL",
            "SQL Server",
            "MySQL",
            "MongoDB",
            "PostgreSQL",
            "Docker",
            "Kubernetes",
            "Azure",
            "AWS",
            "JavaScript",
            "TypeScript",
            "Node.js",
            "NestJS",
            "Git",
            "GitHub",
            "HTML",
            "CSS",
            "Tailwind"
        ];

        public List<string> ExtractSkills(string text)
        {
            return _masterSkills
                .Where(skill =>
                    text.Contains(
                        skill,
                        StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToList();
        }
    }
}
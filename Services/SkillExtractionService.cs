using System.Text.RegularExpressions;

namespace ResumeAnalyzer.Services
{
    /// <summary>
    /// ATS-style skill extraction: taxonomy with canonical names, aliases,
    /// word-boundary matching, and normalization (similar to keyword parsers in enterprise ATS).
    /// </summary>
    public class SkillExtractionService
    {
        private static readonly Dictionary<string, string[]> SkillTaxonomy = BuildTaxonomy();

        private static Dictionary<string, string[]> BuildTaxonomy()
        {
            var entries = new (string canonical, string[] aliases)[]
            {
                (".NET", [".net", "dotnet", "dot net", "asp.net core", "asp.net", "aspnet"]),
                ("C#", ["c#", "csharp", "c sharp"]),
                ("Java", ["java", "java ee", "java se", "j2ee"]),
                ("Python", ["python", "python3", "py"]),
                ("JavaScript", ["javascript", "js", "ecmascript"]),
                ("TypeScript", ["typescript", "ts"]),
                ("React", ["react", "react.js", "reactjs", "react native", "react-native"]),
                ("Angular", ["angular", "angularjs", "angular.js"]),
                ("Vue.js", ["vue", "vue.js", "vuejs"]),
                ("Node.js", ["node.js", "nodejs", "node"]),
                ("Spring Boot", ["spring boot", "springboot", "spring framework", "spring"]),
                ("ASP.NET Core", ["asp.net core", "aspnet core"]),
                ("SQL", ["sql", "t-sql", "pl/sql", "sql queries"]),
                ("SQL Server", ["sql server", "mssql", "microsoft sql server", "ms sql"]),
                ("PostgreSQL", ["postgresql", "postgres", "psql"]),
                ("MySQL", ["mysql"]),
                ("MongoDB", ["mongodb", "mongo"]),
                ("Redis", ["redis"]),
                ("Elasticsearch", ["elasticsearch", "elastic search", "elk"]),
                ("Docker", ["docker", "containerization", "containers"]),
                ("Kubernetes", ["kubernetes", "k8s", "kubectl"]),
                ("AWS", ["aws", "amazon web services", "ec2", "s3", "lambda"]),
                ("Azure", ["azure", "microsoft azure", "azure devops"]),
                ("GCP", ["gcp", "google cloud", "google cloud platform"]),
                ("CI/CD", ["ci/cd", "cicd", "continuous integration", "continuous delivery", "continuous deployment"]),
                ("Git", ["git", "github", "gitlab", "bitbucket", "version control"]),
                ("REST API", ["rest api", "restful", "rest", "web api"]),
                ("GraphQL", ["graphql"]),
                ("Microservices", ["microservices", "micro services", "service-oriented"]),
                ("HTML", ["html", "html5"]),
                ("CSS", ["css", "css3", "scss", "sass", "less"]),
                ("Tailwind CSS", ["tailwind", "tailwind css", "tailwindcss"]),
                ("Entity Framework", ["entity framework", "ef core", "entity framework core"]),
                ("LINQ", ["linq"]),
                ("RabbitMQ", ["rabbitmq", "rabbit mq"]),
                ("Kafka", ["kafka", "apache kafka"]),
                ("Agile", ["agile", "scrum", "kanban", "safe"]),
                ("Jira", ["jira", "confluence"]),
                ("Selenium", ["selenium", "selenium webdriver"]),
                ("Cypress", ["cypress"]),
                ("Jest", ["jest"]),
                ("xUnit", ["xunit", "nunit", "mstest"]),
                ("JUnit", ["junit", "testng"]),
                ("Power BI", ["power bi", "powerbi"]),
                ("Excel", ["excel", "microsoft excel", "spreadsheets"]),
                ("Tableau", ["tableau"]),
                ("Machine Learning", ["machine learning", "ml", "deep learning", "neural networks"]),
                ("TensorFlow", ["tensorflow"]),
                ("PyTorch", ["pytorch"]),
                ("Data Analysis", ["data analysis", "data analytics", "analytics"]),
                ("Pandas", ["pandas"]),
                ("NumPy", ["numpy"]),
                ("Spark", ["spark", "apache spark", "pyspark"]),
                ("Hadoop", ["hadoop"]),
                ("ETL", ["etl", "data pipeline", "data pipelines"]),
                ("Linux", ["linux", "unix", "ubuntu", "centos"]),
                ("Bash", ["bash", "shell scripting", "shell script"]),
                ("C++", ["c++", "cpp"]),
                ("Go", ["golang", " go "]),
                ("Rust", ["rust"]),
                ("Swift", ["swift", "ios development"]),
                ("Kotlin", ["kotlin", "android development"]),
                ("Flutter", ["flutter", "dart"]),
                ("Figma", ["figma"]),
                ("UI/UX", ["ui/ux", "ui ux", "user experience", "user interface", "ux design", "ui design"]),
                ("DevOps", ["devops", "dev sec ops", "devsecops"]),
                ("Terraform", ["terraform", "infrastructure as code", "iac"]),
                ("Ansible", ["ansible"]),
                ("Jenkins", ["jenkins"]),
                ("GitHub Actions", ["github actions"]),
                ("OAuth", ["oauth", "oauth2", "openid connect", "oidc"]),
                ("JWT", ["jwt", "json web token"]),
                ("SSO", ["sso", "single sign-on"]),
                ("SAP", ["sap", "sap hana"]),
                ("Salesforce", ["salesforce", "apex", "lightning"]),
                ("SEO", ["seo", "search engine optimization"]),
                ("Digital Marketing", ["digital marketing", "sem", "ppc", "google ads"]),
                ("Project Management", ["project management", "pmp", "stakeholder management"]),
                ("Communication", ["communication skills", "verbal communication", "written communication"]),
                ("Leadership", ["leadership", "team leadership", "people management"]),
                ("Problem Solving", ["problem solving", "analytical skills", "critical thinking"]),
            };

            var taxonomy = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var (canonical, aliases) in entries)
                taxonomy[canonical] = aliases;
            return taxonomy;
        }

        public List<string> ExtractSkills(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return [];

            var normalized = NormalizeText(text);
            var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (canonical, aliases) in SkillTaxonomy)
            {
                if (MatchesSkill(normalized, canonical) ||
                    aliases.Any(alias => MatchesSkill(normalized, alias)))
                {
                    found.Add(canonical);
                }
            }

            ExtractCertificationsAndTools(normalized, found);

            return found.OrderBy(s => s).ToList();
        }

        private static string NormalizeText(string text)
        {
            text = text.Replace('\u00A0', ' ');
            text = Regex.Replace(text, @"[^\w\s/+#.\-]", " ");
            text = Regex.Replace(text, @"\s+", " ");
            return $" {text.ToLowerInvariant()} ";
        }

        private static bool MatchesSkill(string normalizedText, string term)
        {
            term = term.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(term))
                return false;

            if (term.Contains(' ') || term.Contains('/') || term.Contains('.'))
                return normalizedText.Contains($" {term} ", StringComparison.Ordinal);

            if (term.Length <= 2)
                return Regex.IsMatch(normalizedText, $@"(?<!\w){Regex.Escape(term)}(?!\w)", RegexOptions.IgnoreCase);

            return Regex.IsMatch(normalizedText, $@"(?<!\w){Regex.Escape(term)}(?!\w)", RegexOptions.IgnoreCase);
        }

        private static void ExtractCertificationsAndTools(string normalized, HashSet<string> found)
        {
            var patterns = new Dictionary<string, string>
            {
                ["AWS Certified"] = @"aws\s+certified",
                ["Azure Certified"] = @"azure\s+certified|microsoft\s+certified",
                ["PMP"] = @"\bpmp\b|project\s+management\s+professional",
                ["ITIL"] = @"\bitil\b",
                ["Six Sigma"] = @"six\s+sigma",
                ["CPA"] = @"\bcpa\b",
                ["MBA"] = @"\bmba\b",
            };

            foreach (var (skill, pattern) in patterns)
            {
                if (Regex.IsMatch(normalized, pattern, RegexOptions.IgnoreCase))
                    found.Add(skill);
            }
        }
    }
}

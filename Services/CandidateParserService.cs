using System.Text.RegularExpressions;

namespace ResumeAnalyzer.Services
{
    public class CandidateParserService
    {
        private static readonly string[] SkipNameFragments =
        [
            "Senior", "Junior", "Lead", "Staff", "Principal",
            "Software", "Developer", "Engineer", "Manager",
            "Director", "Analyst", "Consultant", "Specialist",
            "Resume", "Curriculum", "Vitae", "Profile", "Summary",
            "Objective", "Experience", "Education", "Skills", "Contact",
            "Phone", "Email", "Address", "LinkedIn", "Portfolio",
            "Professional", "Personal", "Information", "Details"
        ];

        private static readonly string[] EmailBlacklist =
        [
            "example.com", "email.com", "test.com", "domain.com",
            "yoursite.com", "sample.com"
        ];

        public string ExtractEmail(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            var matches = Regex.Matches(
                text,
                @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}",
                RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var email = match.Value.Trim().ToLowerInvariant();

                if (EmailBlacklist.Any(b => email.Contains(b)))
                    continue;

                if (email.Length > 5 && email.Contains('@'))
                    return match.Value.Trim();
            }

            return "";
        }

        public string ExtractPhone(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            var patterns = new[]
            {
                @"(\+91[- ]?)?[6-9]\d{9}",
                @"\+?1[-.\s]?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}",
                @"\+?\d{1,3}[-.\s]?\(?\d{2,4}\)?[-.\s]?\d{3,4}[-.\s]?\d{3,4}",
                @"\b\d{3}[-.\s]\d{3}[-.\s]\d{4}\b"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern);
                if (match.Success)
                    return match.Value.Trim();
            }

            return "";
        }

        public string ExtractName(string text, string? fileName = null)
        {
            var strategies = new Func<string>[]
            {
                () => ExtractNameBeforeEmail(text),
                () => ExtractNameBeforePhone(text),
                () => ExtractNameFromHeader(text),
                () => ExtractNameFromFilename(fileName)
            };

            foreach (var strategy in strategies)
            {
                var name = strategy();
                if (IsValidName(name))
                    return name;
            }

            return "Unknown";
        }

        public int? ExtractExperienceYears(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var match = Regex.Match(
                text,
                @"(\d{1,2})\+?\s*(?:years?|yrs?)\s+(?:of\s+)?experience",
                RegexOptions.IgnoreCase);

            if (match.Success && int.TryParse(match.Groups[1].Value, out var years))
                return years;

            match = Regex.Match(
                text,
                @"experience\s*[:\-]?\s*(\d{1,2})\+?\s*(?:years?|yrs?)",
                RegexOptions.IgnoreCase);

            if (match.Success && int.TryParse(match.Groups[1].Value, out years))
                return years;

            return null;
        }

        private string ExtractNameBeforeEmail(string text)
        {
            var email = ExtractEmail(text);
            if (string.IsNullOrEmpty(email))
                return "";

            var index = text.IndexOf(email, StringComparison.OrdinalIgnoreCase);
            if (index <= 0)
                return "";

            var beforeEmail = text[..index];
            return FindBestNameMatch(beforeEmail, takeFromEnd: true);
        }

        private string ExtractNameBeforePhone(string text)
        {
            var phone = ExtractPhone(text);
            if (string.IsNullOrEmpty(phone))
                return "";

            var index = text.IndexOf(phone, StringComparison.Ordinal);
            if (index <= 0)
                return "";

            var beforePhone = text[..index];
            return FindBestNameMatch(beforePhone, takeFromEnd: true);
        }

        private string ExtractNameFromHeader(string text)
        {
            var lines = text
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Take(8)
                .ToList();

            foreach (var line in lines)
            {
                if (line.Contains('@') || Regex.IsMatch(line, @"\d{3}.*\d{3}"))
                    continue;

                if (line.Length > 60)
                    continue;

                var name = FindBestNameMatch(line, takeFromEnd: false);
                if (IsValidName(name))
                    return name;
            }

            return "";
        }

        private string ExtractNameFromFilename(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "";

            var baseName = Path.GetFileNameWithoutExtension(fileName);
            baseName = Regex.Replace(baseName, @"(?i)(resume|cv|curriculum|vitae)", "");
            baseName = baseName.Replace('_', ' ').Replace('-', ' ').Trim();

            if (string.IsNullOrWhiteSpace(baseName))
                return "";

            var words = baseName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 1)
                .Take(4)
                .ToArray();

            if (words.Length < 2)
                return "";

            return string.Join(" ",
                words.Select(w =>
                    char.ToUpperInvariant(w[0]) +
                    w[1..].ToLowerInvariant()));
        }

        private string FindBestNameMatch(string text, bool takeFromEnd)
        {
            var matches = Regex.Matches(
                text,
                @"\b([A-Z][a-z]+(?:\s+[A-Z][a-z]+){1,3})\b");

            if (matches.Count == 0)
            {
                matches = Regex.Matches(
                    text,
                    @"\b([A-Z]{2,}(?:\s+[A-Z]{2,}){1,2})\b");

                if (matches.Count > 0)
                {
                    var raw = takeFromEnd ? matches[^1].Value : matches[0].Value;
                    return ToTitleCase(raw);
                }

                return "";
            }

            var candidates = matches
                .Cast<Match>()
                .Select(m => m.Value.Trim())
                .Where(IsValidName)
                .ToList();

            if (candidates.Count == 0)
                return "";

            return takeFromEnd ? candidates[^1] : candidates[0];
        }

        private static string ToTitleCase(string value)
        {
            return string.Join(" ",
                value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(w =>
                        char.ToUpperInvariant(w[0]) +
                        w[1..].ToLowerInvariant()));
        }

        private static bool IsValidName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (name.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                return false;

            if (name.Any(c => char.IsDigit(c) || c == '@'))
                return false;

            if (SkipNameFragments.Any(skip =>
                    name.Contains(skip, StringComparison.OrdinalIgnoreCase)))
                return false;

            var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return words.Length >= 2 && words.Length <= 4;
        }
    }
}

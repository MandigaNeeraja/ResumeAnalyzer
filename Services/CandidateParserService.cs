using System.Text.RegularExpressions;

namespace ResumeAnalyzer.Services
{
    public class CandidateParserService
    {
        private static readonly HashSet<string> SkipNameWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "Senior", "Junior", "Lead", "Staff", "Principal", "Associate", "Assistant",
            "Software", "Developer", "Engineer", "Manager", "Director", "Analyst",
            "Consultant", "Specialist", "Architect", "Designer", "Administrator",
            "Frontend", "Backend", "Full", "Stack", "Web", "Mobile", "Data",
            "Resume", "Curriculum", "Vitae", "Profile", "Summary", "Objective",
            "Experience", "Education", "Skills", "Contact", "Phone", "Email",
            "Address", "LinkedIn", "Portfolio", "Professional", "Personal",
            "Information", "Details", "Technical", "Project", "Projects",
            "Name", "Candidate"
        };

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
            text = NormalizeResumeText(text);

            var strategies = new Func<string>[]
            {
                () => ExtractNameFromLabeledField(text),
                () => ExtractNameFromHeader(text),
                () => ExtractNameFromContactRegion(text),
                () => ExtractNameBeforeEmail(text),
                () => ExtractNameBeforePhone(text),
                () => ExtractNameFromEmailLocalPart(text),
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

        public string ExtractLinkedIn(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            var patterns = new[]
            {
                @"https?://(?:www\.)?linkedin\.com/in/[A-Za-z0-9\-_%]+/?",
                @"(?:www\.)?linkedin\.com/in/[A-Za-z0-9\-_%]+/?"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var url = match.Value.Trim().TrimEnd('.', ',', ';');
                    if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        url = "https://" + url;
                    return url;
                }
            }

            return "";
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

        private static string NormalizeResumeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            text = text.Replace('\u00A0', ' ');
            text = Regex.Replace(text, @"[ \t]+", " ");
            text = Regex.Replace(text, @"\r\n?|\n", "\n");
            return text.Trim();
        }

        private string ExtractNameFromLabeledField(string text)
        {
            var match = Regex.Match(
                text,
                @"(?<![A-Za-z])(?:full[ \t]+name|candidate[ \t]+name|name)[ \t]*[:\-][ \t]*([^\r\n@]{2,50})",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                return "";

            return TryParseNameFromLine(match.Groups[1].Value) ?? "";
        }

        private string ExtractNameFromContactRegion(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            var regionEnd = text.Length;
            var email = ExtractEmail(text);
            if (!string.IsNullOrEmpty(email))
            {
                var emailIndex = text.IndexOf(email, StringComparison.OrdinalIgnoreCase);
                if (emailIndex > 0)
                    regionEnd = Math.Min(regionEnd, emailIndex);
            }

            var phone = ExtractPhone(text);
            if (!string.IsNullOrEmpty(phone))
            {
                var phoneIndex = text.IndexOf(phone, StringComparison.Ordinal);
                if (phoneIndex > 0)
                    regionEnd = Math.Min(regionEnd, phoneIndex);
            }

            var linkedIn = ExtractLinkedIn(text);
            if (!string.IsNullOrEmpty(linkedIn))
            {
                var linkedInIndex = text.IndexOf(linkedIn, StringComparison.OrdinalIgnoreCase);
                if (linkedInIndex > 0)
                    regionEnd = Math.Min(regionEnd, linkedInIndex);
            }

            regionEnd = Math.Min(regionEnd, 300);
            if (regionEnd <= 0)
                return "";

            var region = text[..regionEnd].Trim();
            return FindBestNameMatch(region, takeFromEnd: false);
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

        private string ExtractNameFromEmailLocalPart(string text)
        {
            var email = ExtractEmail(text);
            if (string.IsNullOrEmpty(email))
                return "";

            var localPart = email.Split('@')[0];
            var parts = Regex.Split(localPart, @"[._\-+]+")
                .Where(p => p.Length >= 2 && Regex.IsMatch(p, @"^[A-Za-z]+$"))
                .Take(3)
                .ToArray();

            if (parts.Length < 2)
                return "";

            var name = ToTitleCase(string.Join(" ", parts));
            return IsValidName(name) ? name : "";
        }

        private string ExtractNameFromHeader(string text)
        {
            var lines = text
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Take(12)
                .ToList();

            foreach (var line in lines)
            {
                if (line.Length > 80)
                    continue;

                var segments = GetNameCandidateSegments(line);
                foreach (var segment in segments)
                {
                    var name = TryParseNameFromLine(segment);
                    if (!string.IsNullOrEmpty(name))
                        return name;

                    name = FindBestNameMatch(segment, takeFromEnd: false);
                    if (IsValidName(name))
                        return name;
                }
            }

            return "";
        }

        private static IEnumerable<string> GetNameCandidateSegments(string line)
        {
            yield return line;

            if (line.Contains('@', StringComparison.Ordinal))
            {
                var emailMatch = Regex.Match(line, @"[A-Za-z0-9._%+-]+@");
                if (emailMatch.Success && emailMatch.Index > 0)
                {
                    var beforeEmail = line[..emailMatch.Index].Trim();
                    if (!string.IsNullOrWhiteSpace(beforeEmail))
                        yield return beforeEmail;
                }
            }

            if (Regex.IsMatch(line, @"\d{3}.*\d{3}"))
            {
                var phoneMatch = Regex.Match(
                    line,
                    @"(\+91[- ]?)?[6-9]\d{9}|\+?1[-.\s]?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}|\b\d{3}[-.\s]\d{3}[-.\s]\d{4}\b");

                if (phoneMatch.Success && phoneMatch.Index > 0)
                {
                    var beforePhone = line[..phoneMatch.Index].Trim();
                    if (!string.IsNullOrWhiteSpace(beforePhone))
                        yield return beforePhone;
                }
            }
        }

        private string ExtractNameFromFilename(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "";

            var baseName = Path.GetFileNameWithoutExtension(fileName);
            baseName = Regex.Replace(baseName, @"(?i)(resume|cv|curriculum|vitae|profile)", "");
            baseName = Regex.Replace(baseName, @"([a-z])([A-Z])", "$1 $2");
            baseName = baseName.Replace('_', ' ').Replace('-', ' ').Trim();

            if (string.IsNullOrWhiteSpace(baseName))
                return "";

            var words = baseName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 1 && Regex.IsMatch(w, @"^[A-Za-z][A-Za-z\-']*$"))
                .Take(4)
                .ToArray();

            if (words.Length < 2)
                return "";

            var name = string.Join(" ", words.Select(ToTitleCaseWord));
            return IsValidName(name) ? name : "";
        }

        private string FindBestNameMatch(string text, bool takeFromEnd)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            var words = Regex.Matches(text, @"[A-Za-z][A-Za-z\-']*")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

            var candidates = new List<string>();
            for (var length = 2; length <= 4 && length <= words.Count; length++)
            {
                for (var start = 0; start <= words.Count - length; start++)
                {
                    var name = NormalizeNameLine(string.Join(" ", words.Skip(start).Take(length)));
                    if (IsValidName(name))
                        candidates.Add(name);
                }
            }

            candidates = candidates
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(CountSkipWords)
                .ThenBy(c => c.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)
                .ThenBy(c => takeFromEnd ? -text.IndexOf(c, StringComparison.OrdinalIgnoreCase)
                    : text.IndexOf(c, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (candidates.Count == 0)
                return "";

            return takeFromEnd ? candidates[^1] : candidates[0];
        }

        private string? TryParseNameFromLine(string line)
        {
            line = Regex.Replace(
                line,
                @"^(?:full\s*name|candidate\s*name|name)\s*[:\-]\s*",
                "",
                RegexOptions.IgnoreCase);
            line = Regex.Replace(line, @"[^\w\s\-'.]", " ").Trim();
            line = Regex.Replace(line, @"\s+", " ");

            if (string.IsNullOrWhiteSpace(line) || line.Length > 60)
                return null;

            if (line.Contains('@'))
                return null;

            if (Regex.IsMatch(line, @"\d{3}"))
                return null;

            var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2 || words.Length > 4)
                return null;

            if (!words.All(LooksLikeNameWord))
                return null;

            var name = NormalizeNameLine(string.Join(" ", words));
            return IsValidName(name) ? name : null;
        }

        private static bool LooksLikeNameWord(string word)
        {
            return word.Length >= 2 &&
                   word.All(c => char.IsLetter(c) || c == '-' || c == '\'');
        }

        private static string NormalizeNameLine(string line)
        {
            var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", words.Select(ToTitleCaseWord));
        }

        private static string ToTitleCase(string value)
        {
            return string.Join(" ",
                value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(ToTitleCaseWord));
        }

        private static string ToTitleCaseWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return word;

            if (word.Contains('-'))
            {
                return string.Join("-",
                    word.Split('-', StringSplitOptions.RemoveEmptyEntries)
                        .Select(ToTitleCaseWord));
            }

            if (word.Length == 1)
                return char.ToUpperInvariant(word[0]).ToString();

            return char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();
        }

        private static int CountSkipWords(string name)
        {
            return name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Count(w => SkipNameWords.Contains(w));
        }

        private static bool IsValidName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (name.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                return false;

            if (name.Any(c => char.IsDigit(c) || c == '@'))
                return false;

            var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2 || words.Length > 4)
                return false;

            if (!words.All(LooksLikeNameWord))
                return false;

            var skipWordCount = words.Count(w => SkipNameWords.Contains(w));
            if (skipWordCount >= words.Length)
                return false;

            if (skipWordCount >= 2)
                return false;

            return true;
        }
    }
}

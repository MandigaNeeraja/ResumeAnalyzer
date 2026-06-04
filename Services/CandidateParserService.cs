//using System.Text.RegularExpressions;

//namespace ResumeAnalyzer.Services
//{
//    public class CandidateParserService
//    {
//        public string ExtractEmail(string text)
//        {
//            var match = Regex.Match(
//                text,
//                @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}");

//            return match.Success ? match.Value : "";
//        }

//        public string ExtractPhone(string text)
//        {
//            var match = Regex.Match(
//                text,
//                @"(\+91[- ]?)?[6-9]\d{9}");

//            return match.Success ? match.Value : "";
//        }

//        public string ExtractName(string text)
//        {
//            var email = ExtractEmail(text);

//            if (string.IsNullOrWhiteSpace(email))
//                return "Unknown";

//            var emailIndex = text.IndexOf(email);

//            if (emailIndex <= 0)
//                return "Unknown";

//            var beforeEmail = text[..emailIndex];

//            // Look for two consecutive capitalized words
//            var matches = Regex.Matches(
//                beforeEmail,
//                @"\b([A-Z][a-z]+)\s+([A-Z][a-z]+)\b");

//            foreach (Match match in matches)
//            {
//                var name = match.Value;

//                if (name.Contains("Senior"))
//                    continue;

//                if (name.Contains("Software"))
//                    continue;

//                if (name.Contains("Developer"))
//                    continue;

//                if (name.Contains("Engineer"))
//                    continue;

//                return name;
//            }

//            return "Unknown";
//        }
//    }
//}
using System.Text.RegularExpressions;

namespace ResumeAnalyzer.Services
{
    public class CandidateParserService
    {
        public string ExtractEmail(string text)
        {
            var match = Regex.Match(
                text,
                @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}");

            return match.Success
                ? match.Value
                : "";
        }

        public string ExtractPhone(string text)
        {
            var match = Regex.Match(
                text,
                @"(\+91[- ]?)?[6-9]\d{9}");

            return match.Success
                ? match.Value
                : "";
        }

        public string ExtractName(string text)
        {
            var phone = ExtractPhone(text);

            if (string.IsNullOrEmpty(phone))
                return "Unknown";

            var phoneIndex = text.IndexOf(phone);

            if (phoneIndex <= 0)
                return "Unknown";

            var beforePhone =
                text.Substring(0, phoneIndex);

            var match = Regex.Match(
                beforePhone,
                @"([A-Z][a-z]+)\s([A-Z][a-z]+)");

            return match.Success
                ? match.Value
                : "Unknown";
        }
    }
}
namespace ResumeAnalyzer.Helpers
{
    public static class MatchHelper
    {
        public static string GetStatusFromScore(double score, int minReviewScore = 60)
        {
            if (score >= 80) return "Shortlisted";
            if (score >= minReviewScore) return "Review";
            return "Not Matched";
        }

        public static double CalculateScore(
            List<string> jobSkills,
            List<string> candidateSkills)
        {
            if (jobSkills.Count == 0)
                return 0;

            var matched = jobSkills
                .Where(js => candidateSkills.Any(cs =>
                    cs.Equals(js, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return Math.Round(
                (matched.Count / (double)jobSkills.Count) * 100,
                0);
        }

        public static List<string> GetMatchedSkills(
            List<string> jobSkills,
            List<string> candidateSkills)
        {
            return jobSkills
                .Where(js => candidateSkills.Any(cs =>
                    cs.Equals(js, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}

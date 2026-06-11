namespace ResumeAnalyzer.Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string HR = "HR";
        public const string Manager = "Manager";

        public const string AdminOnly = Admin;
        public const string HROnly = HR;
        public const string ManagerOnly = Manager;
        public const string AdminOrHR = $"{Admin},{HR}";
        public const string AdminOrManager = $"{Admin},{Manager}";
        public const string HROrManager = $"{HR},{Manager}";
        public const string AllRoles = $"{Admin},{HR},{Manager}";
    }
}

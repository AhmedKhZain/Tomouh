namespace Tomouh.Shared.Kernel.Enums;

public static class Permissions
{
    // Scholarship Module
    public static class Scholarship
    {
        public const string Read = "Scholarship.Read";
        public const string Add = "Scholarship.Add";
        public const string Update = "Scholarship.Update";
        public const string Delete = "Scholarship.Delete";
        public const string Restore = "Scholarship.Restore";
        public static List<string> All = new() { Read, Add, Update, Delete, Restore };
    }

    // Fund Organization Module
    public static class FundOrganization
    {
        public const string Read = "FundOrganization.Read";
        public const string Add = "FundOrganization.Add";
        public const string Update = "FundOrganization.Update";
        public const string Delete = "FundOrganization.Delete";
        public const string Restore = "FundOrganization.Restore";
        public static List<string> All = new() { Read, Add, Update, Delete, Restore };
    }
    public static class User
    {
        public const string Read = "User.Read";
        public const string Add = "User.Add";
        public const string Update = "User.Update";
        public const string Delete = "User.Delete";
        public const string Restore = "User.Restore";
        public static List<string> All = new() { Read, Add, Update, Delete, Restore };
    }
    public static class UserProfile
    {
        public const string Read = "UserProfile.Read";
        public const string Update = "UserProfile.Update";
        public const string Delete = "UserProfile.Delete";
        public const string Restore = "UserProfile.Restore";
        public const string Add = "UserProfile.Add";
        public static List<string> All = new() { Read, Update, Delete, Restore, Add };
    }
}

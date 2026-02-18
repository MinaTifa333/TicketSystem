namespace TicketSystem.TSModel.Permission;

public static class Permissions
{
    public static class Categories
    {
        public const string View = "Permissions.categories.View";
        public const string Add = "Permissions.categories.Add";
        public const string Edit = "Permissions.categories.Edit";
        public const string Delete = "Permissions.categories.Delete";
    }


    public static class Sections
    {
        public const string View = "Permissions.Sections.View";
        public const string Create = "Permissions.Sections.Create";
        public const string Edit = "Permissions.Sections.Edit";
        public const string Delete = "Permissions.Sections.Delete";
    }



    public static List<string> All =>
        typeof(Permissions).GetNestedTypes()
            .SelectMany(t => t.GetFields()
                .Where(f => f.IsLiteral)
                .Select(f => f.GetRawConstantValue()?.ToString() ?? string.Empty))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();
}

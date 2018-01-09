namespace MVCForumAutomation
{
    public class TestDefaults
    {
        public Role StandardMembers { get; } = new Role("Standard Members");
        public Category ExampleCategory { get; } = new Category("Example Category");
        public string AdminUsername { get; } = "admin";
        public string AdminPassword { get; } = "password";
    }
}
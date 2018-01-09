namespace MVCForumAutomation
{
    public class TestDefaults
    {
        public Role StandardMembers { get; } = new Role("Standard Members");

        public Category ExampleCategory
        {
            get { throw new System.NotImplementedException(); }
        }

        public string AdminUsername { get; } = "admin";
        public string AdminPassword { get; } = "password";
    }
}
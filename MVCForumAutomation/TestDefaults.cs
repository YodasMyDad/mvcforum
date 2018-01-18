namespace MVCForumAutomation
{
    public class TestDefaults
    {
        public Role StandardMembers
        {
            get { throw new System.NotImplementedException(); }
        }

        public Category ExampleCategory
        {
            get { throw new System.NotImplementedException(); }
        }

        public string AdminUsername { get; } = "admin";
        public string AdminPassword { get; } = "password";
    }
}
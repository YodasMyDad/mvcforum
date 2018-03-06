namespace MvcForum.Core.Events
{
    using Interfaces;

    public class LoginEventArgs : MvcForumEventArgs
    {
        public string ReturnUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public IMvcForumContext MvcForumContext { get; set; }
    }
}
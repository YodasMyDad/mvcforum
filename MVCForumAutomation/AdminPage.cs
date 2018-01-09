using OpenQA.Selenium;

namespace MVCForumAutomation
{
    public class AdminPage
    {
        private readonly IWebDriver _webDriver;

        public AdminPage(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public RolePermissionsPage GetPermissionsFor(Role role)
        {
            throw new System.NotImplementedException();
        }
    }
}
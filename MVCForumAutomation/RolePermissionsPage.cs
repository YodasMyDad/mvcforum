using OpenQA.Selenium;

namespace MVCForumAutomation
{
    public class RolePermissionsPage
    {
        private readonly IWebDriver _webDriver;

        public RolePermissionsPage(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public void AddToCategory(Category category, PermissionTypes permissionTypes)
        {
            throw new System.NotImplementedException();
        }
    }
}
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
            var sideNavBar = _webDriver.FindElement(By.ClassName("side-nav"));
            var permissionsMenu = sideNavBar.FindElement(By.XPath("//a[@data-target='#permissions']"));
            permissionsMenu.Click();

            var managePermissionsMenuItem = _webDriver.FindElement(By.ClassName("auto-managePermissions"));
            managePermissionsMenuItem.Click();

            var roleButton =
                _webDriver.FindElement(By.XPath($"//ul[@class='rolepermissionlist']//a[text()='{role.Name}']"));
            roleButton.Click();

            return new RolePermissionsPage(_webDriver);
        }
    }
}
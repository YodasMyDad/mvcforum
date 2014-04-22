using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IInstallerService
    {
        InstallerResult CreateDbTables(string connectionStringOveride, string sqlFilePath, string currentVersion);
        InstallerResult CreateInitialData();
    }
}

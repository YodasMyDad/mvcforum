using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface IInstallerService
    {
        InstallerResult CreateDbTables(string connectionStringOveride, string sqlFilePath, string currentVersion);
        InstallerResult CreateInitialData();
    }
}

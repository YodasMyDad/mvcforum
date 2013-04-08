using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCachingProvider;
using EFCachingProvider.Caching;
using EFCachingProvider.Web;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public class InstallerService : IInstallerService
    {
        private readonly IInstallerRepository _installerRepository;

        public InstallerService(IInstallerRepository installerRepository)
        {
            _installerRepository = installerRepository;
        }

        public InstallerResult CreateDbTables(string connectionStringOveride, string filepathOveride, string currentVersion)
        {
            return _installerRepository.CreateDbTables(connectionStringOveride, filepathOveride, currentVersion);
        }

        public InstallerResult CreateInitialData()
        {
            // CANT DO THIS HERE AS NEED TO USE UNIT OF WORK

            throw new NotImplementedException();
        }
    }
}

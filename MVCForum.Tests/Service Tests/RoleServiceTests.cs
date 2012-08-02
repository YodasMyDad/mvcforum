using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Exceptions;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Services;
using NSubstitute;
using NUnit.Framework;

namespace MVCForum.Tests.Service_Tests
{
    [TestFixture]
    public class RoleServiceTests
    {
        //private readonly ICacheHelper _testCacheHelper = new TestCache();

        [Test]
        [ExpectedException(typeof(InUseUnableToDeleteException))]
        public void Delete_Exception_If_Role_Has_Multiple_Users()
        {
            var roleRepository = Substitute.For<IRoleRepository>();
            var categoryPermissionForRoleRepository = Substitute.For<ICategoryPermissionForRoleRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var roleService = new RoleService(roleRepository, categoryPermissionForRoleRepository, permissionRepository);

            var role = new MembershipRole
                           {
                               Users = new List<MembershipUser>
                                           {
                                               new MembershipUser {UserName = "Hawkeye"},
                                               new MembershipUser {UserName = "Blackwidow"}
                                           },
                               RoleName = "Role Name"
                           };

            roleService.Delete(role);
        }


        [Test]
        public void Delete_Check_In_Use_By()
        {
            var roleRepository = Substitute.For<IRoleRepository>();
            var categoryPermissionForRoleRepository = Substitute.For<ICategoryPermissionForRoleRepository>();
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var roleService = new RoleService(roleRepository, categoryPermissionForRoleRepository, permissionRepository);

            var role = new MembershipRole
            {
                Users = new List<MembershipUser>
                                           {
                                               new MembershipUser {UserName = "Hawkeye"},
                                               new MembershipUser {UserName = "Blackwidow"}
                                           },
                RoleName = "Role Name"
            };

            try
            {
                roleService.Delete(role);
            }
            catch (InUseUnableToDeleteException ex)
            {
                Assert.IsTrue(ex.BlockingEntities.Any());
            }

        }
    }
}

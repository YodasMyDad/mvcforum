using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Services;
using NSubstitute;
using NUnit.Framework;

namespace MVCForum.Tests.Service_Tests
{
    [TestFixture]
    public class PermissionServiceTests
    {

        [Test]
        public void Delete_Check_CategoryPermissionForRoles_Are_Deleted()
        {
            var permissionRepository = Substitute.For<IPermissionRepository>();
            var categoryPermissionForRoleRepository = Substitute.For<ICategoryPermissionForRoleRepository>();
            var permissionService = new PermissionService(permissionRepository, categoryPermissionForRoleRepository);

            var permission = new Permission { Name = "Ghost Rider", Id = Guid.NewGuid() };
            var catePermOne = new CategoryPermissionForRole {Id = Guid.NewGuid()};
            var catePermTwo = new CategoryPermissionForRole {Id = Guid.NewGuid()};
            var catepermrole = new List<CategoryPermissionForRole>{ catePermOne, catePermTwo };

            categoryPermissionForRoleRepository.GetByPermission(permission.Id).Returns(catepermrole);


            permissionService.Delete(permission);

            categoryPermissionForRoleRepository.Received().Delete(Arg.Is<CategoryPermissionForRole>(x => x.Id == catePermOne.Id));
            categoryPermissionForRoleRepository.Received().Delete(Arg.Is<CategoryPermissionForRole>(x => x.Id == catePermTwo.Id));
        }
    }
}

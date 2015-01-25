using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class GlobalPermissionForRoleRepository : IGlobalPermissionForRoleRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public GlobalPermissionForRoleRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }
        public void Add(GlobalPermissionForRole permissionForRole)
        {
            _context.GlobalPermissionForRole.Add(permissionForRole);
        }

        public void Delete(GlobalPermissionForRole permissionForRole)
        {
            _context.GlobalPermissionForRole.Remove(permissionForRole);
        }

        public GlobalPermissionForRole Get(Guid permId, Guid roleId)
        {
            return _context.GlobalPermissionForRole.Include(x => x.MembershipRole).FirstOrDefault(x => x.Permission.Id == permId && x.MembershipRole.Id == roleId);
        }

        public GlobalPermissionForRole Get(Guid permId)
        {
            return _context.GlobalPermissionForRole.Include(x => x.MembershipRole).FirstOrDefault(x => x.Id == permId);
        }

        public IList<GlobalPermissionForRole> GetAll(MembershipRole role)
        {
            return _context.GlobalPermissionForRole.Include(x => x.MembershipRole).Where(x => x.MembershipRole.Id == role.Id).ToList();
        }

        public IList<GlobalPermissionForRole> GetAll()
        {
            return _context.GlobalPermissionForRole.Include(x => x.MembershipRole).ToList();
        }
    }
}

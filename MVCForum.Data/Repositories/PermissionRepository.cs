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
    public partial class PermissionRepository : IPermissionRepository
    {
        private readonly MVCForumContext _context;

        public PermissionRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public IEnumerable<Permission> GetAll()
        {
            return _context.Permission
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToList();
        }

        public Permission Add(Permission permission)
        {
            return _context.Permission.Add(permission);
        }

        public Permission Get(Guid id)
        {
            return _context.Permission.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(Permission item)
        {
            _context.Permission.Remove(item);
        }
    }
}

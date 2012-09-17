using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Utilities;


namespace MVCForum.Data.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly MVCForumContext _context;

        public PermissionRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public IEnumerable<Permission> GetAll()
        {
            return _context.Permission
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

        public void Update(Permission item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.Permission.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified;  
        }
    }
}

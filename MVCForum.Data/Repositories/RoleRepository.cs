﻿using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using System.Data.Entity;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class RoleRepository : IRoleRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"> </param>
        public RoleRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        /// <summary>
        /// Get all roles out of the database
        /// </summary>
        /// <returns></returns>
        public IList<MembershipRole> AllRoles()
        {
            return _context.MembershipRole
                .OrderByDescending(x => x.RoleName)
                .ToList();
        }

        /// <summary>
        /// Get a role by name
        /// </summary>
        /// <param name="rolename"></param>
        /// <param name="removeTracking"></param>
        /// <returns></returns>
        public MembershipRole GetRole(string rolename, bool removeTracking)
        {
            if (removeTracking)
            {
                return _context.MembershipRole
                    .Include(x => x.CategoryPermissionForRoles)
                    .Include(x => x.GlobalPermissionForRole)
                    .AsNoTracking()
                    .FirstOrDefault(y => y.RoleName.Contains(rolename));    
            }
            return _context.MembershipRole.FirstOrDefault(y => y.RoleName.Contains(rolename));
        }

        public MembershipRole Add(MembershipRole item)
        {
            var role = GetRole(item.RoleName, false);
            return role ?? _context.MembershipRole.Add(item);
        }

        public MembershipRole Get(Guid id)
        {
            return _context.MembershipRole.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(MembershipRole item)
        {
            _context.MembershipRole.Remove(item);
        }

        public void Update(MembershipRole item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.MembershipRole.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified; 
        }
    }
}

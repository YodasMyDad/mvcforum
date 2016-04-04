using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;
using MVCForum.Utilities;
using System.Data.SqlTypes;

namespace MVCForum.Services
{
    public partial class FirmService : IFirmService
    {
        private readonly MVCForumContext _context;

        public FirmService(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }


        /// <summary>
        /// Get firm by name
        /// </summary>
        /// <param name="firmname"></param>
        /// <returns></returns>
        public MembershipFirm GetFirm(string firmname)
        {
            return _context.MembershipFirm.FirstOrDefault(x => x.FirmName == firmname);
        }

        /// <summary>
        /// Get firm by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MembershipFirm GetFirm(Guid id)
        {
            return _context.MembershipFirm.FirstOrDefault(x => x.Id == id);
        }

        ///// <summary>
        ///// Get all users for a specified firm
        ///// </summary>
        ///// <param name="firmName"></param>
        ///// <returns></returns>
        //public IList<MembershipUser> GetUsersForFirm(string firmName)
        //{
        //    return _context.GetFirm(firmName).Users;

        //}

        /// <summary>
        /// Create a new firm
        /// </summary>
        /// <param name="firm"></param>
        public void CreateFirm(MembershipFirm firm)
        {
            firm.FirmName = StringUtils.SafePlainText(firm.FirmName);
            firm.CreateDate = DateTime.UtcNow;
            firm.LastModified = DateTime.UtcNow;

            // url slug generator
            firm.Slug = ServiceHelpers.GenerateSlug(firm.FirmName, GetFirmBySlugLike(ServiceHelpers.CreateUrl(firm.FirmName)), null);
            _context.MembershipFirm.Add(firm);
        }

        /// <summary>
        /// Delete a firm
        /// </summary>
        /// <param name="firm"></param>

        ///// <summary>
        ///// Save a firm
        ///// </summary>
        ///// <param name="firm"></param>
        //public void Save(MembershipFirm firm)
        //{
        //    firm.FirmName = StringUtils.SafePlainText(firm.FirmName);
        //    _context.Update(firm);
        //}


        #region Methods

        public IList<MembershipFirm> AllFirms()
        {
            return _context.MembershipFirm
                .OrderBy(x => x.FirmName)
                .ToList();
        }

        /// <summary>
        /// Get a firm by name
        /// </summary>
        /// <param name="firmname"></param>
        /// <returns></returns>

        public MembershipFirm Add(MembershipFirm item)
        {
            var firm = GetFirm(item.FirmName);
            return firm ?? _context.MembershipFirm.Add(item);
        }

        public MembershipFirm Get(Guid id)
        {
            return _context.MembershipFirm.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(MembershipFirm item)
        {
            _context.MembershipFirm.Remove(item);
        }

        public void Update(MembershipFirm item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.MembershipFirm.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified;
        }

        public MembershipFirm GetFirmBySlug(string slug)
        {
            return _context.MembershipFirm.FirstOrDefault(x => x.Slug == slug);
        }

        public IList<MembershipFirm> GetFirmBySlugLike(string slug)
        {
            return _context.MembershipFirm
                            .Where(x => x.Slug.Contains(slug))
                            .ToList();
        }

        #endregion



    }
}

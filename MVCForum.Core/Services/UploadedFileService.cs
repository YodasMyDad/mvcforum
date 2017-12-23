namespace MVCForum.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data.Entity;
    using Domain.DomainModel;
    using Domain.Interfaces.Services;
    using Data.Context;

    public partial class UploadedFileService : IUploadedFileService
    {
        private readonly MVCForumContext _context;
        public UploadedFileService(MVCForumContext context)
        {
            _context = context;
        }


        public UploadedFile Add(UploadedFile uploadedFile)
        {
            uploadedFile.DateCreated = DateTime.UtcNow;
            return _context.UploadedFile.Add(uploadedFile);
        }

        public void Delete(UploadedFile uploadedFile)
        {
            _context.UploadedFile.Remove(uploadedFile);
        }

        public IList<UploadedFile> GetAll()
        {
            return _context.UploadedFile.ToList();
        }

        public IList<UploadedFile> GetAllByPost(Guid postId)
        {
            return _context.UploadedFile.Where(x => x.Post.Id == postId).ToList();
        }

        public IList<UploadedFile> GetAllByUser(Guid membershipUserId)
        {
            return _context.UploadedFile.Where(x => x.MembershipUser.Id == membershipUserId).ToList();
        }

        public UploadedFile Get(Guid id)
        {
            return _context.UploadedFile
                .Include(x => x.Post.Topic.Category)
                .FirstOrDefault(x => x.Id == id);
        }
    }
}

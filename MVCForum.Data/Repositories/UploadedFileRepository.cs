﻿using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Data.Context;
using System.Data.Entity;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class UploadedFileRepository : IUploadedFileRepository
    {
        private readonly MVCForumContext _context;
        public UploadedFileRepository(MVCForumContext context)
        {
            _context = context;
        }

        public UploadedFile Add(UploadedFile uploadedFile)
        {
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
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == id);
        }
    }
}

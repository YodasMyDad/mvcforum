using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;

namespace MVCForum.Services
{
    public partial class PostEditService : IPostEditService
    {
        private readonly MVCForumContext _context;
        public PostEditService(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public void Delete(IList<PostEdit> entities)
        {
            foreach (var entity in entities)
            {
                Delete(entity);
            }
        }

        public void Delete(PostEdit entity)
        {
            _context.PostEdit.Remove(entity);
        }

        public PostEdit Add(PostEdit entity)
        {
            var isTheSame = false;
            var isEmpty = true;
            // only add if they are not the same
            if (!string.IsNullOrEmpty(entity.EditedPostTitle) && !string.IsNullOrEmpty(entity.OriginalPostTitle))
            {
                if (string.Compare(entity.EditedPostTitle, entity.OriginalPostTitle, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    isTheSame = true;
                }
                isEmpty = false;
            }            
            if (!string.IsNullOrEmpty(entity.EditedPostContent) && !string.IsNullOrEmpty(entity.OriginalPostContent))
            {
                if (string.Compare(entity.EditedPostContent, entity.OriginalPostContent, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // If we get here, and is empty is false and isthesame is false, it means the titles are 
                    // different - So DON't set as the same.
                    if (isEmpty == false && isTheSame == false)
                    {
                        // DO Nothing
                    }
                    else
                    {
                        isTheSame = true;
                    }
                }
                isEmpty = false;
            }

            // Check it's not empty and the data is not the same
            if (!isEmpty && !isTheSame)
            {
                return _context.PostEdit.Add(entity);
            }
            return entity;
        }

        public PostEdit Get(Guid id)
        {
            return _context.PostEdit.FirstOrDefault(x => x.Id == id);
        }

        public IList<PostEdit> GetByPost(Guid postId)
        {
            return _context.PostEdit.AsNoTracking()
                .Include(x => x.EditedBy)
                .Include(x => x.Post.User)
                .Where(x => x.Post.Id == postId)
                .OrderByDescending(x => x.DateEdited).ToList();
        }

        public IList<PostEdit> GetByMember(Guid memberId)
        {
            return _context.PostEdit.AsNoTracking()
                .Include(x => x.EditedBy)
                .Include(x => x.Post.User)
                .Where(x => x.EditedBy.Id == memberId)
                .OrderByDescending(x => x.DateEdited).ToList();
        }
    }
}

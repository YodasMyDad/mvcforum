namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;

    public partial class PostEditService : IPostEditService
    {
        private IMvcForumContext _context;

        public PostEditService(IMvcForumContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
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
            if (!string.IsNullOrWhiteSpace(entity.EditedPostTitle) && !string.IsNullOrWhiteSpace(entity.OriginalPostTitle))
            {
                if (string.Compare(entity.EditedPostTitle, entity.OriginalPostTitle,
                        StringComparison.OrdinalIgnoreCase) == 0)
                {
                    isTheSame = true;
                }
                isEmpty = false;
            }
            if (!string.IsNullOrWhiteSpace(entity.EditedPostContent) && !string.IsNullOrWhiteSpace(entity.OriginalPostContent))
            {
                if (string.Compare(entity.EditedPostContent, entity.OriginalPostContent,
                        StringComparison.OrdinalIgnoreCase) == 0)
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
                else
                {
                    isTheSame = false;
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
            return  _context.PostEdit
                .Include(x => x.EditedBy)
                .Include(x => x.Post.User)
                .FirstOrDefault(x => x.Id == id);
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
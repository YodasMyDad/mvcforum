using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IUploadedFileService
    {
        UploadedFile Add(UploadedFile uploadedFile);
        void Delete(UploadedFile uploadedFile);
        IList<UploadedFile> GetAll();
        IList<UploadedFile> GetAllByPost(Guid postId);
        IList<UploadedFile> GetAllByUser(Guid membershipUserId);
        UploadedFile Get(Guid id);  
    }
}

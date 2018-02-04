namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using Models.General;

    public partial interface IUploadedFileService : IContextService
    {
        UploadedFile Add(UploadedFile uploadedFile);
        void Delete(UploadedFile uploadedFile);
        IList<UploadedFile> GetAll();
        IList<UploadedFile> GetAllByPost(Guid postId);
        IList<UploadedFile> GetAllByUser(Guid membershipUserId);
        UploadedFile Get(Guid id);
    }
}
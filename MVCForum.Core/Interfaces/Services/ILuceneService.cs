using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Lucene.LuceneModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface ILuceneService
    {
        void UpdateIndex();
        bool CheckIndexExists();
        void AddUpdate(LuceneSearchModel luceneSearchModel);
        void DeleteEntireIndex();
        void Delete(Guid id);
        void OptimiseIndex();
        LuceneSearchModel MapToModel(Post post, string topicname);
        LuceneSearchModel MapToModel(Topic topic, string content);
        IEnumerable<LuceneSearchModel> Search(string searchTerm, string fieldName);
        IEnumerable<LuceneSearchModel> Search(string searchTerm);
        IEnumerable<LuceneSearchModel> GetAll();
   }
}

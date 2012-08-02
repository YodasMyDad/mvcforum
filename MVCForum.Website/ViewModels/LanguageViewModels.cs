using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.ViewModels
{
    public class LanguageListAllViewModel
    {
        public IEnumerable<Language> Alllanguages { get; set; }
    }
}
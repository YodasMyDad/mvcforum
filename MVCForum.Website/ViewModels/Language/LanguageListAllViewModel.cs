namespace MvcForum.Web.ViewModels.Language
{
    using System;
    using System.Collections.Generic;
    using Core.DomainModel.Entities;

    public class LanguageListAllViewModel
    {
        public IEnumerable<Language> Alllanguages { get; set; }
        public Guid CurrentLanguage { get; set; }
    }
}
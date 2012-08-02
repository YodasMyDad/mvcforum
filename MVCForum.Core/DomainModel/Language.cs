using System;
using System.Collections.Generic;

namespace MVCForum.Domain.DomainModel
{
    public class Language : Entity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LanguageCulture { get; set; }
        public string FlagImageFileName { get; set; }       
        public bool RightToLeft { get; set; }
        public virtual IList<LocaleStringResource> LocaleStringResources { get; set; }
    }
}

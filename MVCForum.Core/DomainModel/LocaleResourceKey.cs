using System;
using System.Collections.Generic;

namespace MVCForum.Domain.DomainModel
{
    public class LocaleResourceKey : Entity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public DateTime DateAdded { get; set; }
        public virtual IList<LocaleStringResource> LocaleStringResources { get; set; }
    }
}

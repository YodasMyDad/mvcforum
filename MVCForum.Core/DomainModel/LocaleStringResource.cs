using System;

namespace MVCForum.Domain.DomainModel
{
    public class LocaleStringResource : Entity
    {
        public Guid Id { get; set; }
        public virtual LocaleResourceKey LocaleResourceKey { get; set; }
        public string ResourceValue { get; set; }
        public virtual Language Language { get; set; }
    }
}

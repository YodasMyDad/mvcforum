namespace MvcForum.Core.Models.Entities
{
    using System;
    using Interfaces;
    using Utilities;

    public partial class LocaleStringResource : IBaseEntity
    {
        public LocaleStringResource()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public virtual LocaleResourceKey LocaleResourceKey { get; set; }
        public string ResourceValue { get; set; }
        public virtual Language Language { get; set; }
    }
}

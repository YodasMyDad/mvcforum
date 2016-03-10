using System;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public partial class Category : Entity
    {
        public Category()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsLocked { get; set; }
        public bool? ModerateTopics { get; set; }
        public bool? ModeratePosts { get; set; }
        public int SortOrder { get; set; }
        public DateTime DateCreated { get; set; }
        public string Slug { get; set; }
        public string PageTitle { get; set; }
        public string Path { get; set; }
        public string MetaDescription { get; set; }
        public string Colour { get; set; }
        public string Image { get; set; }
        public virtual Category ParentCategory { get; set; }
        public virtual IList<Topic> Topics { get; set; }
        public virtual IList<CategoryNotification> CategoryNotifications { get; set; }
        public virtual IList<CategoryPermissionForRole> CategoryPermissionForRoles { get; set; }
        public int Level { get; set; }

        public string NiceUrl => UrlTypes.GenerateUrl(UrlType.Category, Slug);
    }




}

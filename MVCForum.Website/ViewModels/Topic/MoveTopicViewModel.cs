namespace MvcForum.Web.ViewModels.Topic
{
    using System;
    using System.Collections.Generic;
    using Core.Models.Entities;

    public class MoveTopicViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CategoryId { get; set; }
        public List<Category> Categories { get; set; }
    }
}
namespace MvcForum.Web.ViewModels
{
    using System;

    public class SubscribeEmailViewModel
    {
        public Guid Id { get; set; }
        public string SubscriptionType { get; set; }
    }

    public class UnSubscribeEmailViewModel
    {
        public Guid Id { get; set; }
        public string SubscriptionType { get; set; }
    }
}
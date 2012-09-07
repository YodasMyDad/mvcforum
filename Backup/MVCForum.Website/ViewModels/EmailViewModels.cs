using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCForum.Website.ViewModels
{
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
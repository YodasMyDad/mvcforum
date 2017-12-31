using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcForum.Web.ViewModels.PrivateMessage
{
    public class GetMoreViewModel
    {
        public Guid UserId { get; set; }
        public int PageIndex { get; set; }
    }
}
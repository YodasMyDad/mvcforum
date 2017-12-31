using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcForum.Web.ViewModels.Vote
{
    using Core.DomainModel.Entities;

    public class ShowVotersViewModel
    {
        public List<Vote> Votes { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVCForum.Domain.DomainModel
{
    public class Email
    {
        public string EmailTo { get; set; }
        public string EmailFrom { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public string NameTo { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class PermissionMapping : EntityTypeConfiguration<Permission>
    {
        public PermissionMapping()
        {
            HasKey(x => x.Id);
        }
    }
}

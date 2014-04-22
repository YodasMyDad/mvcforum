using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.API
{
    public partial interface ICategoryAPI
    {
        Category Get(Guid id);
        Category GetBySlug(string slug);
    }
}

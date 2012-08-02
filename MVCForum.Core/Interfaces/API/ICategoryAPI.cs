using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.API
{
    public interface ICategoryAPI
    {
        Category Get(Guid id);
        Category GetBySlug(string slug);
    }
}

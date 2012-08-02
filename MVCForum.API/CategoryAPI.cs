using System;
using System.Collections.Generic;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.DomainModel;

namespace MVCForum.API
{
    public class CategoryAPI : ICategoryAPI
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryAPI(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public Category Get(Guid id)
        {
            return _categoryRepository.Get(id);
        }

        public Category GetBySlug(string slug)
        {
            return _categoryRepository.GetBySlug(slug);
        }
    }
}

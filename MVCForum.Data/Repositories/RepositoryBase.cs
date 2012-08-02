using System;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Repositories
{
    /// <summary>
    /// Abstract class supplying basic CRUD functions to super-class repository types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RepositoryBase<T> where T : Entity
    {
        /// <summary>
        /// Simple get
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Get(Guid id)
        {
            throw new NotImplementedException();
            //return GetContextSession().Get<T>(id);
        }

        /// <summary>
        /// Generic save or update
        /// </summary>
        /// <param name="entity"></param>
        public virtual void SaveOrUpdate(Entity entity)
        {
            throw new NotImplementedException();
            //GetContextSession().SaveOrUpdate(entity);
        }

        /// <summary>
        /// Generic single entity delete
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Delete(T entity)
        {
            throw new NotImplementedException();
            //GetContextSession().Delete(entity);
        }
    }
}

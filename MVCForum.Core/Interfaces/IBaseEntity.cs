namespace MvcForum.Core.Interfaces
{
    using System;

    /// <summary>
    /// A holding class to identify our entity classes generically
    /// </summary>
    public interface IBaseEntity
    {
        Guid Id { get; set; }
    }
}

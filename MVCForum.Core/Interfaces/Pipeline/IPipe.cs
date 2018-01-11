namespace MvcForum.Core.Interfaces.Pipeline
{
    using System.Threading.Tasks;

    public interface IPipe<T>
    {
        Task<T> Process(T input, IMvcForumContext context);
    }
}
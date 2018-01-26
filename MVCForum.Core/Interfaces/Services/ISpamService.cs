namespace MvcForum.Core.Interfaces.Services
{
    using Models.Entities;

    public partial interface ISpamService
    {
        bool IsSpam(Post post);
        bool IsSpam(Topic topic);
        bool IsSpam(string content);
    }
}
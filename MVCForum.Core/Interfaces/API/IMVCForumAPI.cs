namespace MVCForum.Domain.Interfaces.API
{
    public interface IMVCForumAPI
    {
        IMemberAPI Member { get; set; }
        IPostAPI Post { get; set; }
        ITopicAPI Topic { get; set; }
        ICategoryAPI Category { get; set; }
    }   
}

namespace MvcForum.Plugins.Pipelines.Post
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Hosting;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Core.Models.Enums;
    using Core.Models.General;

    public class PostDeletePipe : IPipe<IPipelineProcess<Post>>
    {
        private readonly IVoteService _voteService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly IUploadedFileService _uploadedFileService;
        private readonly IFavouriteService _favouriteService;
        private readonly IPostEditService _postEditService;
        private readonly ILoggingService _loggingService;
        private readonly IRoleService _roleService;
        private readonly ILocalizationService _localizationService;

        public PostDeletePipe(IVoteService voteService, IMembershipUserPointsService membershipUserPointsService, IUploadedFileService uploadedFileService, IFavouriteService favouriteService, IPostEditService postEditService, ILoggingService loggingService, IRoleService roleService, ILocalizationService localizationService)
        {
            _voteService = voteService;
            _membershipUserPointsService = membershipUserPointsService;
            _uploadedFileService = uploadedFileService;
            _favouriteService = favouriteService;
            _postEditService = postEditService;
            _loggingService = loggingService;
            _roleService = roleService;
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Post>> Process(IPipelineProcess<Post> input, IMvcForumContext context)
        {
            // Refresh the context in each of these services
            _voteService.RefreshContext(context);
            _membershipUserPointsService.RefreshContext(context);
            _uploadedFileService.RefreshContext(context);
            _favouriteService.RefreshContext(context);
            _postEditService.RefreshContext(context);
            _roleService.RefreshContext(context);
            _localizationService.RefreshContext(context);

            try
            {
                // Get the Current user from ExtendedData
                var username = input.ExtendedData[Constants.ExtendedDataKeys.Username] as string;
                var loggedOnUser = await context.MembershipUser.FirstOrDefaultAsync(x => x.UserName == username);
                var loggedOnUsersRole = loggedOnUser.GetRole(_roleService);
                var permissions = _roleService.GetPermissions(input.EntityToProcess.Topic.Category, loggedOnUsersRole);

                if (input.EntityToProcess.User.Id == loggedOnUser.Id ||
                    permissions[ForumConfiguration.Instance.PermissionDeletePosts].IsTicked)
                {
                    // Get the topic
                    var topic = input.EntityToProcess.Topic;

                    var votes = _voteService.GetVotesByPost(input.EntityToProcess.Id);

                    #region Deleting Points

                    // Remove the points the user got for this post
                    await _membershipUserPointsService.Delete(input.EntityToProcess.User, PointsFor.Post, input.EntityToProcess.Id);

                    // Also get all the votes and delete anything to do with those
                    foreach (var postVote in votes)
                    {
                        await _membershipUserPointsService.Delete(PointsFor.Vote, postVote.Id);
                    }

                    // Also the mark as solution
                    await _membershipUserPointsService.Delete(PointsFor.Solution, input.EntityToProcess.Id);

                    #endregion

                    await context.SaveChangesAsync();

                    #region Deleting Votes

                    var votesToDelete = new List<Vote>();
                    votesToDelete.AddRange(votes);
                    foreach (var vote in votesToDelete)
                    {
                        input.EntityToProcess.Votes.Remove(vote);
                        _voteService.Delete(vote);
                    }
                    input.EntityToProcess.Votes.Clear();

                    #endregion

                    #region Files

                    // Clear files attached to post
                    var filesToDelete = new List<UploadedFile>();
                    filesToDelete.AddRange(input.EntityToProcess.Files);
                    foreach (var uploadedFile in filesToDelete)
                    {
                        // store the file path as we'll need it to delete on the file system
                        var filePath = uploadedFile.FilePath;

                        input.EntityToProcess.Files.Remove(uploadedFile);
                        _uploadedFileService.Delete(uploadedFile);

                        // And finally delete from the file system
                        if (!string.IsNullOrWhiteSpace(filePath))
                        {
                            var mapped = HostingEnvironment.MapPath(filePath);
                            if (mapped != null)
                            {
                                File.Delete(mapped);
                            }
                        }
                    }
                    input.EntityToProcess.Files.Clear();

                    #endregion

                    #region Favourites

                    var postFavourites = new List<Favourite>();
                    postFavourites.AddRange(input.EntityToProcess.Favourites);
                    foreach (var postFavourite in postFavourites)
                    {
                        input.EntityToProcess.Favourites.Remove(postFavourite);
                        _favouriteService.Delete(postFavourite);
                    }
                    input.EntityToProcess.Favourites.Clear();

                    #endregion

                    #region Post Edits

                    var postEdits = new List<PostEdit>();
                    postEdits.AddRange(input.EntityToProcess.PostEdits);
                    foreach (var postEdit in postEdits)
                    {
                        input.EntityToProcess.PostEdits.Remove(postEdit);
                        _postEditService.Delete(postEdit);
                    }
                    input.EntityToProcess.PostEdits.Clear();

                    #endregion

                    await context.SaveChangesAsync();

                    // Before we delete the post, we need to check if this is the last post in the topic
                    // and if so update the topic

                    if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.IgnoreLastPost))
                    {
                        var ignoreLastPost = input.ExtendedData[Constants.ExtendedDataKeys.IgnoreLastPost] as bool?;
                        if (ignoreLastPost == false)
                        {
                            var lastPost = topic.Posts.OrderByDescending(x => x.DateCreated).FirstOrDefault();

                            if (lastPost != null && lastPost.Id == input.EntityToProcess.Id)
                            {
                                // Get the new last post and update the topic
                                topic.LastPost = topic.Posts.Where(x => x.Id != input.EntityToProcess.Id).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                            }

                            if (topic.Solved && input.EntityToProcess.IsSolution)
                            {
                                topic.Solved = false;
                            }

                            // Save the topic
                            await context.SaveChangesAsync();
                        }
                    }

                    // Remove from the topic
                    topic.Posts.Remove(input.EntityToProcess);

                    // now delete the post
                    context.Post.Remove(input.EntityToProcess);

                    // Save changes
                    await context.SaveChangesAsync();
                }
                else
                {
                    input.AddError(_localizationService.GetResourceString("Errors.NoPermission"));
                }
            }
            catch (System.Exception ex)
            {
                input.AddError(ex.Message);
                _loggingService.Error(ex);
            }

            return input;
        }
    }
}
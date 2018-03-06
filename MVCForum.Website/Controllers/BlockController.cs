namespace MvcForum.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using ViewModels;

    public partial class BlockController : BaseController
    {
        private readonly IBlockService _blockService;

        public BlockController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            IBlockService blockService, ICacheService cacheService, IMvcForumContext context) :
            base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
            _blockService = blockService;
        }

        [HttpPost]
        [Authorize]
        public virtual void BlockOrUnBlock(EntityIdViewModel viewModel)
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    // Get a db user
                    var loggedOnUser = MembershipService.GetUser(User.Identity.Name, true);

                    // Other member
                    MembershipUser otherMember = MembershipService.GetUser(viewModel.Id);

                    var block = loggedOnUser.BlockedUsers.FirstOrDefault(x => x.Blocked.Id == otherMember.Id);
                    if (block != null)
                    {
                        var getBlock = _blockService.Get(block.Id);
                        _blockService.Delete(getBlock);
                    }
                    else
                    {
                        loggedOnUser.BlockedUsers.Add(new Block
                        {
                            Blocked = otherMember,
                            Blocker = loggedOnUser,
                            Date = DateTime.UtcNow
                        });
                    }

                    Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                    throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
            }
        }
    }
}
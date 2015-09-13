using System;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    public class BlockController : BaseController
    {
        private readonly IBlockService _blockService;
        public BlockController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, 
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService, IBlockService blockService) : 
            base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _blockService = blockService;
        }

        [HttpPost]
        [Authorize]
        public virtual void BlockOrUnBlock(BlockMemberViewModel viewModel)
        {
            if (Request.IsAjaxRequest())
            {                
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        // Get a db user
                        var loggedOnUser = MembershipService.GetUser(LoggedOnReadOnlyUser.Id);

                        // Other member
                        var otherMember = MembershipService.GetUser(viewModel.MemberToBlockOrUnBlock);

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
                                Blocked = otherMember, Blocker = loggedOnUser, Date = DateTime.UtcNow
                            });
                        }

                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                }
            }
        }
    }
}
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    public partial class AdminLuceneController : Controller
    {
        private readonly ILuceneService _luceneService;

        public AdminLuceneController(ILuceneService luceneService)
        {
            _luceneService = luceneService;
        }

        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult Update()
        {
            // Set the timeout quite large just in case this takes a while
            Server.ScriptTimeout = 600;

            _luceneService.UpdateIndex();

            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "Index Updated",
                MessageType = GenericMessages.success
            };

            return View("Index");
        }

        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult Optimise()
        {
            // Set the timeout quite large just in case this takes a while
            Server.ScriptTimeout = 600;

            _luceneService.OptimiseIndex();

            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "Index Optimised",
                MessageType = GenericMessages.success
            };

            return View("Index");
        }

        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult Delete()
        {
            // Set the timeout quite large just in case this takes a while
            Server.ScriptTimeout = 600;

            _luceneService.DeleteEntireIndex();

            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "Index Deleted",
                MessageType = GenericMessages.success
            };

            return View("Index");
        }

    }
}

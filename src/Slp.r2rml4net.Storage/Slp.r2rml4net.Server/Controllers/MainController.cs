using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Slp.r2rml4net.Server.Controllers
{
    /// <summary>
    /// Main controller
    /// </summary>
    public class MainController : Controller
    {
        /// <summary>
        /// The index page
        /// </summary>
        public ActionResult Index()
        {
            return RedirectToAction("Query");
        }

        /// <summary>
        /// The query page
        /// </summary>
        public ActionResult Query()
        {
            return View();
        }

        /// <summary>
        /// The page informing that the application start failed.
        /// </summary>
        public ActionResult AppStartFailed()
        {
            return View(Slp.r2rml4net.Server.R2RML.StorageWrapper.StartException);
        }

        /// <summary>
        /// Called before the action method is invoked.
        /// </summary>
        /// <param name="filterContext">Information about the current request and action.</param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (Slp.r2rml4net.Server.R2RML.StorageWrapper.StartException != null && filterContext.ActionDescriptor.ActionName != "AppStartFailed")
            {
                filterContext.Result = RedirectToAction("AppStartFailed");
            }
        }
	}
}
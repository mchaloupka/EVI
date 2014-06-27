using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Slp.r2rml4net.Server.Controllers
{
    public class MainController : Controller
    {
        //
        // GET: /Main/
        public ActionResult Index()
        {
            return RedirectToAction("Query");
        }

        public ActionResult Query()
        {
            return View();
        }

        public ActionResult AppStartFailed()
        {
            return View(Slp.r2rml4net.Server.R2RML.StorageWrapper.StartException);
        }

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
using AdobeScheduler.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdobeScheduler.Security;
using AdobeScheduler.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace AdobeScheduler.Controllers
{


    public class DashboardController : Controller
    {
        //
        // GET: /Dashboard/
        
        [AdobeAuthorize]
        public ActionResult Index()
        {
            UserSession model = (UserSession)Session["UserSession"];            
            ViewObject viewObject = new ViewObject(model);
            return View(viewObject);
        }
    }
}

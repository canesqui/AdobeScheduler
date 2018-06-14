using AdobeScheduler.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdobeScheduler.Security;
using AdobeConnectSDK;
using AdobeScheduler.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace AdobeScheduler.Controllers
{


    public class DashboardController : Controller
    {
        //
        // GET: /Dashboard/

        [Authorize]
        [AdobeAuthorize]
        public ActionResult Index()
        {
            UserSession model = (UserSession)Session["UserSession"];
            ViewObject viewObject = new ViewObject(model);
            return View(viewObject);
        }

        [Authorize]
        [AdobeAuthorize]
        public ActionResult Report()
        {
            return View();
        }

        public JsonResult EventFeed()
        {
            string jsDate = Request.QueryString["date"];
            AdobeStream stream = new AdobeStream();
            var eventList = stream.GetAllAppointments(jsDate);
            return Json(eventList, JsonRequestBehavior.AllowGet); 
        }

        public ActionResult Room(string room)
        {
            UserSession model = (UserSession)Session["UserSession"];
            using (AdobeConnectDB _db = new AdobeConnectDB())
            {
                room = "/" + room + "/";
                var query = (from r in _db.Appointments where r.url == room select r).FirstOrDefault();
                if (query != null)
                {
                    if (DateTime.Now < query.start) {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    return View(query);
                }
                else
                {
                    ViewBag.Course = room;
                    return RedirectToAction("Index", "Dashboard");
                }
            } 
            
        } 

        


    }
}

using AdobeScheduler.Models;
using AdobeScheduler.Security;
using System.Web;
using System.Web.Mvc;
using AdobeConnectSDK;
using System.Web.Security;
using System;
using AdobeScheduler.Util;
using AdobeConnectSDK.Model;

namespace AdobeScheduler.Controllers
{
    public class AuthController : Controller
    {   
        
        [HandleError]
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(LoginUser user)
        {
            if (ModelState.IsValid)
            {
                AdobeConnectXmlAPI con = new AdobeConnectXmlAPI();                
                if (con.Login(user.Username, user.Password).Result)
                {                    
                    int id = int.Parse(con.GetUserInfo().Result.UserId);
                    Identity Id = new Identity( id , user.Username, "T", con.GetUserInfo().SessionInfo);
                    DateTime expire = DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes);
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(Id.ID, user.Username, DateTime.Now, expire, false, Id.GetUserData());
                    string hashTicket = FormsAuthentication.Encrypt(ticket);
                    HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hashTicket);
                    HttpContext.Response.Cookies.Add(cookie);
                    var userMeetings = Utilities.Adapter<Models.MeetingItem[], EnumerableResultStatus<AdobeConnectSDK.Model.MeetingItem>>(AdobeConnectSDK.Extensions.MeetingManagement.GetMyMeetings(con));
                    var sessionInfo = Utilities.Adapter<Models.UserInfo, AdobeConnectSDK.Model.UserInfo>(con.GetUserInfo().Result);

                    UserSession userSession = new UserSession(userMeetings, sessionInfo);
                    
                    Session["UserSession"] = userSession;
                }
                else {
                    return View("Login");
               }

                
            }

            return RedirectToAction("Index", "Dashboard");

        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "")
            {
                Expires = DateTime.Now.AddYears(-1)
            };
            Response.Cookies.Add(cookie1);
            return RedirectToAction("Index", "Dashboard");
        }

        public ActionResult UnAUthorized()
        {
            return View("Login"); 
        }

        
    }
}

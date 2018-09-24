using AdobeScheduler.Security;
using AdobeScheduler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Data.Entity;
using AutoMapper;
using log4net;
using Microsoft.AspNet.SignalR;

namespace AdobeScheduler
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            Mapper.Initialize(Util.AutoMapperConfiguration.Configure);
            GlobalHost.HubPipeline.AddModule(new Hubs.UnhandledExceptionHandlingModule());
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            //log the error!
            Log.Error(ex);
        }

        void MvcApplication_PostAuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket == null || authTicket.Expired)
                {
                    return;
                }

                Identity id = new Identity(authTicket.Name, authTicket.UserData);
                GenericPrincipal user = new GenericPrincipal(id, null);
                Context.User = user;
                Thread.CurrentPrincipal = user;
            }
        }

    }
}
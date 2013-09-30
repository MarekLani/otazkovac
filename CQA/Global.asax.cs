using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CQA.Models;
using WebMatrix.WebData;

namespace CQA
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            routes.MapRoute(
                "Default",
                "{controller}/{action}/{name}/{id}",
                new { controller = "HelloWorldController", action = "Welcome", name = UrlParameter.Optional, id = UrlParameter.Optional }
        );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            Database.SetInitializer<CQADBContext>( new CQADBContext.Initializer());
            CQADBContext context = new CQADBContext();
            context.Database.Initialize(true);
            if (!WebSecurity.Initialized)
                WebSecurity.InitializeDatabaseConnection("FitManagerDBContext", "UserProfile", "UserId", "UserName", autoCreateTables: true);
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
          //Create culture info object 
         CultureInfo ci = new CultureInfo("sk-SK");

         System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
         System.Threading.Thread.CurrentThread.CurrentCulture = 
        CultureInfo.CreateSpecificCulture(ci.Name);

        }
    }
}
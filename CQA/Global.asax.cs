using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Caching;
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

            //Repetitive task, not needed right now
            //AddTask("DoStuff", 10);
        }

        #region Scheduled tasks

        private static CacheItemRemovedCallback OnCacheRemove = null;

        private void AddTask(string name, int seconds)
        {
            OnCacheRemove = new CacheItemRemovedCallback(CacheItemRemoved);

            //Midnight task
            //HttpRuntime.Cache.Insert(name, null, null,
            //    DateTime.Today.AddDays(1),
            //    Cache.NoSlidingExpiration,
            //    CacheItemPriority.Normal, null);

            HttpRuntime.Cache.Insert(name, seconds, null,
                DateTime.Now.AddSeconds(seconds), Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, OnCacheRemove);
        }

        public void CacheItemRemoved(string k, object v, CacheItemRemovedReason r)
        {
            // do stuff here if it matches our taskname, like WebRequest
            // re-add our task so it recurs
            //SetupsStatistics.SaveSetupStats();
            AddTask(k, Convert.ToInt32(v));
        }
        #endregion

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
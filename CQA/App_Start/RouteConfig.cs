using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CQA
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "SKAbout",
                url: "o_nas/{id}",
                defaults: new
                {
                    controller = "Home",
                    action = "About",
                    id = UrlParameter.Optional
                });

            routes.MapRoute(
                name: "SKContact",
                url: "kontakt/{id}",
                defaults: new
                {
                    controller = "Home",
                    action = "Contact",
                    id = UrlParameter.Optional
                });


            

            routes.MapRoute(
               "ArticleDetail",
               "clanok/{title}",
               defaults: new
                {
                    controller = "articles",
                    action = "details",
                    title = UrlParameter.Optional
                });


            routes.MapRoute("Base",
                string.Empty,
                new
                {
                    controller = "Questions",
                    action = "Index",
                    id = string.Empty
                });

            routes.MapRoute(
               "Welcome",
               "{controller}/{action}/{name}/{numTimes}/",
               new { controller = "HelloWorldController", action = "Welcome", name = UrlParameter.Optional, numTimes = UrlParameter.Optional },
               new { numTimes = @"\d+", name = @"[a-z][A-Z]*" }
               );

           routes.MapRoute(
              "Base2",
              "{controller}/{action}/{id}",
              new { controller = "Home", action = "Index", id = UrlParameter.Optional}
              );




        }
    }
}
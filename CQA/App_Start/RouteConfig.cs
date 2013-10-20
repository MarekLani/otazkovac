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

            routes.MapRoute("Base",
                string.Empty,
                new
                {
                    controller = "Home",
                    action = "Index",
                    id = string.Empty
                });

            routes.MapRoute(
             "AnswerAndEvaluate",
             "AnswerAndEvaluate/{setupId}",
             new { Controller="QuestionsAndAnswers", action = "AnswerAndEvaluate", setupId = UrlParameter.Optional }
             );

            routes.MapRoute(
              "Base2",
              "{controller}/{action}/{id}",
              new { controller = "Setups", action = "Questions", id = UrlParameter.Optional }
             );

        }
    }
}
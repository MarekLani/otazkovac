using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using CQA.Models;
using WebMatrix.WebData;

namespace CQA.Controllers
{

    public class HomeController : Controller
    {
        private CQADBContext db = new CQADBContext();
        public ActionResult Index()
        {

            return View();
        }

        /// <summary>
        /// Prepare dynamic part of menu
        /// </summary>
        /// <returns></returns>
        public ActionResult _MainMenu()
        {
            var menu = new Menu();
            menu.Setups = db.Setups.Where(s => s.Active && s.Displayed).ToList();
            menu.SubjectsNotEmpty = db.Subjects.Any();
            menu.UnseenEvaluatedAnswersCount = db.Notifications.Where(n => n.Answer.Setup.Active && n.NotificationFor == NotificationFor.MyAnswer && n.UserId == WebSecurity.CurrentUserId).GroupBy(n => n.AnswerId).Count();
            menu.UnseenEvaluatedEvaluationsCount = db.Notifications.Where(n => n.Answer.Setup.Active && n.NotificationFor == NotificationFor.MyEvaluation && n.UserId == WebSecurity.CurrentUserId).GroupBy(n => n.AnswerId).Count();
            return PartialView("_Menu", menu);
        }



    }
}

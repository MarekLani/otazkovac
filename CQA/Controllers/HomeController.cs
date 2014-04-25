using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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

        public ActionResult GetEvals()
        {
            using (var sw = new StreamWriter("evals.scv",false))
            {
                foreach(var a in db.Answers.Where(a => a.Evaluations.Count > 15))
                {
                    foreach (var eval in a.Evaluations)
                    {
                        sw.WriteLine(a.QuestionId + ";" + eval.AnswerId + ";" + eval.Author.UserName + ";" + eval.UserId + ";" + eval.Value + "\n");
                    }
                }
            }
            return Json(true);
        }

        /// <summary>
        /// Prepare dynamic part of menu
        /// </summary>
        /// <returns></returns>
        public ActionResult _MainMenu()
        {
            var menu = new Menu();
            menu.Setups = db.Setups.Where(s => s.Active).ToList();
            menu.SubjectsNotEmpty = db.Subjects.Any();
            menu.UnseenEvaluatedAnswersCount = db.Notifications.Where(n => n.NotificationFor == NotificationFor.MyAnswer && n.UserId == WebSecurity.CurrentUserId).GroupBy(n => n.AnswerId).Count();
            menu.UnseenEvaluatedEvaluationsCount = db.Notifications.Where(n => n.NotificationFor == NotificationFor.MyEvaluation && n.UserId == WebSecurity.CurrentUserId).GroupBy(n => n.AnswerId).Count();
            return PartialView("_Menu", menu);
        }



    }
}

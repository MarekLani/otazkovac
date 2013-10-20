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
            menu.Setups = db.Setups.Where(s => s.Active).ToList();
            menu.SubjectsNotEmpty = db.Subjects.Any();
            menu.UnseenEvaluatedAnswersCount = db.Answers.Where(a => a.SeenEvaluation == false && a.UserId == WebSecurity.CurrentUserId).ToList().Count();
            return PartialView("_Menu", menu);
        }



    }
}

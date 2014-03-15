using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CQA.Models;
using WebMatrix.WebData;

namespace CQA.Controllers
{
    public class UsersController : Controller
    {
        //
        // GET: /Users/
        private CQADBContext db = new CQADBContext();
        
        public ActionResult Index()
        {
            var users = db.UserProfiles;
            return View();
        }

        public ActionResult EvaluatedAnswers()
        {
            List<EvaluatedAnswers> evaluatedAnswers = new List<Models.EvaluatedAnswers>();
            var setupsWithEvaluatedAnswers = db.Answers.Where(a => a.UserId == WebSecurity.CurrentUserId && a.Evaluations.Count > 0)
                .OrderByDescending(a => a.DateCreated).GroupBy(a => a.Question.Setup);
            foreach(var s in setupsWithEvaluatedAnswers)
            {
                EvaluatedAnswers ea = new EvaluatedAnswers();
                ea.Answers = new List<Answer>();
                ea.UnseenHighlightedAnswers = new List<Answer>();
                ea.Setup = s.Key;
                ea.UnseenCount = 0;

                List<Notification> Notifications = new List<Notification>();
                Notifications = db.Notifications.Where(n => n.UserId == WebSecurity.CurrentUserId && n.NotificationFor == NotificationFor.MyAnswer).ToList();

                foreach(Answer a in s)
                {
                    ea.Answers.Add(a);
                    foreach(Notification not in Notifications.Where(n => n.AnswerId == a.AnswerId))
                    {
                        ea.UnseenCount++;
                        ea.UnseenHighlightedAnswers.Add(a);
                        db.Notifications.Remove(not);
                    }
                }
                evaluatedAnswers.Add(ea);
            }
            db.SaveChanges();



            return View(evaluatedAnswers);
        }

        public ActionResult MyEvaluations()
        {
            List<EvaluatedAnswers> evaluatedAnswers = new List<Models.EvaluatedAnswers>();
            var setupsWithEvaluatedAnswers = db.Answers.Where(a => a.Evaluations.Where(e => e.UserId == WebSecurity.CurrentUserId).Any())
                .OrderByDescending(a => a.DateCreated).GroupBy(a => a.Question.Setup);
            foreach (var s in setupsWithEvaluatedAnswers)
            {
                EvaluatedAnswers ea = new EvaluatedAnswers();
                ea.Answers = new List<Answer>();
                ea.UnseenHighlightedAnswers = new List<Answer>();
                ea.Setup = s.Key;
                ea.UnseenCount = 0;
                List<Notification> Notifications = new List<Notification>();
                Notifications = db.Notifications.Where(n => n.UserId == WebSecurity.CurrentUserId && n.NotificationFor == NotificationFor.MyEvaluation).ToList();

                foreach (Answer a in s)
                {
                    ea.Answers.Add(a);
                    foreach (Notification not in Notifications.Where(n => n.AnswerId == a.AnswerId))
                    {
                        ea.UnseenCount++;
                        ea.UnseenHighlightedAnswers.Add(a);
                        db.Notifications.Remove(not);
                    }
                }
                evaluatedAnswers.Add(ea);
            }
            db.SaveChanges();
            return View(evaluatedAnswers);
        }

        // GET: /Account/RegisterUser
        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddAdmin()
        {
            var roles = (SimpleRoleProvider)Roles.Provider;
            ViewData["Admins"] = roles.GetUsersInRole("Admin").ToList();
            return View();
        }

        [HttpPost]
        public ActionResult AddAdmin(string user)
        {
            var roles = (SimpleRoleProvider)Roles.Provider;
            if (!roles.GetRolesForUser(user).Contains("Admin"))
            {
                roles.AddUsersToRoles(new[] { user }, new[] { "Admin" });
                TempData["AdminAdded"] = user + "bol pridaný medzi administrátorov";
            }
            else
            {
                TempData["AdminNotAdded"] = user + " je už administrátorom";
            }
            ViewData["Admins"] = roles.GetUsersInRole("Admin").ToList();
            return View();
        }

        [HttpPost]
        public ActionResult GetUsers(string Name)
        {
            var ret = db.UserProfiles.Where(u => u.UserName.StartsWith(Name)).Select(a => new { UserId = a.UserId, UserName = a.UserName });
            return Json(ret);

        }

    }
}

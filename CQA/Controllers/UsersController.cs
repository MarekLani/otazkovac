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
            var setupsWithEvaluatedAnswers = db.Answers.Where(a => a.UserId == WebSecurity.CurrentUserId && a.SeenEvaluation != null )
                .OrderByDescending(a => a.DateCreated).GroupBy(a => a.Question.Setup);
            foreach(var s in setupsWithEvaluatedAnswers) //.Where(a => a SeenEvaluation == false))
            {
                EvaluatedAnswers ea = new EvaluatedAnswers();
                ea.Answers = new List<Answer>();
                ea.Setup = s.Key;
                ea.UnseenCount = 0;
                foreach(Answer a in s)
                {
                    ea.Answers.Add(a);
                    if (!(bool)a.SeenEvaluation)
                    {
                        ea.UnseenCount++;
                        a.SeenEvaluation = true;
                        db.Entry(a).State = EntityState.Modified;
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
            if (!roles.GetRolesForUser("xlanim").Contains("Admin"))
            {
                roles.AddUsersToRoles(new[] { "xlanim" }, new[] { "Admin" });
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

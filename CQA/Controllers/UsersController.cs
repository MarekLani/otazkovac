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

        public ActionResult EvaluatedAnswers(int id)
        {
            var evaluatedAnswers = db.Answers.Where(a => a.UserId == id && a.Evaluations.Count() > 2).OrderByDescending(a => a.DateCreated);
            foreach(var a in evaluatedAnswers.Where(a => a.SeenEvaluation == false))
            {
                a.SeenEvaluation = true;
                db.Entry(a).State = EntityState.Modified;
                db.SaveChanges();
            }
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
                TempData["AdminAdded"] = user + "je už administrátorom";
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

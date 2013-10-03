using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CQA.Models;

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

    }
}

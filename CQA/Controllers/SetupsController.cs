using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CQA.Models;
using WebMatrix.WebData;

namespace CQA.Controllers
{

    [Authorize(Roles = "Admin")] 
    public class SetupsController : Controller
    {
        private CQADBContext db = new CQADBContext();

        //
        // GET: /Setups/

        public ActionResult Index()
        {
            var setup = db.Setups.Include(s => s.Subject);
            return View(setup.ToList());
        }

        //
        // GET: /Setups/Details/5

        public ActionResult Details(int id = 0)
        {
            Setup setup = db.Setups.Find(id);
            if (setup == null)
            {
                return HttpNotFound();
            }
            return View(setup);
        }

        //
        // GET: /Setups/Create

        public ActionResult Create()
        {
            ViewBag.SubjectId = new SelectList(db.Subjects, "SubjectId", "Name");
            Setup s = new Setup();
            s.AnsweringProbability = 10;
            s.Active = true;
            return View(s);
        }

        //
        // POST: /Setups/Create

        [HttpPost]
        public ActionResult Create(Setup setup)
        {
            if (ModelState.IsValid)
            {
                setup.StartDate = DateTime.Now;
                db.Setups.Add(setup);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SubjectId = new SelectList(db.Subjects, "SubjectId", "Name", setup.SubjectId);
            return View(setup);
        }

        //
        // GET: /Setups/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Setup setup = db.Setups.Find(id);
            if (setup == null)
            {
                return HttpNotFound();
            }
            ViewBag.SubjectId = new SelectList(db.Subjects, "SubjectId", "Name", setup.SubjectId);
            return View(setup);
        }

        //
        // POST: /Setups/Edit/5

        [HttpPost]
        public ActionResult Edit(Setup setup)
        {
            Setup original = db.Setups.Find(setup.SetupId);
            if (ModelState.IsValid)
            {
                int originValue = original.AnsweringProbability;

                setup.DateCreated = original.DateCreated;
                db.Entry(original).CurrentValues.SetValues(setup);
                db.SaveChanges();

                if (setup.AnsweringProbability != originValue)
                    SetupsProbabilityChange.CreateSetupsProbabilityChange(setup.SetupId,setup.AnsweringProbability);

                return RedirectToAction("Index");
            }
            ViewBag.SubjectId = new SelectList(db.Subjects, "SubjectId", "Name", setup.SubjectId);
            return View(setup);
        }

        //
        // GET: /Setups/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Setup setup = db.Setups.Find(id);
            if (setup == null)
            {
                return HttpNotFound();
            }
            return View(setup);
        }

        //
        // POST: /Setups/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Setup setup = db.Setups.Find(id);
            db.Setups.Remove(setup);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult ConvertOldNotifications()
        {
            foreach (Answer ans in db.Answers.Where(a => a.SeenEvaluation == false).ToList())
            {
                Notification not = new Notification();
                
                not.AnswerId = ans.AnswerId;
                not.NotificationFor = NotificationFor.MyAnswer;
                not.User = ans.Author;
                not.NotificationType = NotificationType.NewEvaluation;
                db.Notifications.Add(not);
                db.SaveChanges();
            }

            return RedirectToAction("Index","Home");
        }

        public ActionResult GetBasicStatistics()
        {
            using (StreamWriter sw = new StreamWriter(Server.MapPath("~/Reports/") + "stats.csv", false))
            {
                sw.WriteLine("pocet hodnoteni ;" + db.Ratings.Count());
                sw.WriteLine("pocet odpovedi ;" + db.Answers.Count());
                sw.WriteLine("dostatocne ohodnotenych ;" + db.Answers.Where(a => a.Evaluations.Count() == 16).Count());

                List<UserProfile> users = db.UserProfiles.OrderBy(u => u.Evaluations.Count()).ToList();
                foreach (UserProfile u in users)
                {
                    sw.WriteLine(u.RealName + ";" + u.Evaluations.Count() + ";" + u.UsersActions.Where(a => a.Action == UserActionType.SkippedAnswering || a.Action == UserActionType.SkippedEvaluation).Count() );
                }
                
            }
            ViewBag.Link = Request.Url.GetLeftPart(UriPartial.Authority)+"/Reports/stats.csv";
            return View();
        }

        [HttpPost]
        public ActionResult AddAnswers(HttpPostedFileBase file, int setupId)
        {
            var reader = new StreamReader(file.InputStream, Encoding.UTF8);
            List<string> columnNames = new List<string>();

            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            Setup s = db.Setups.Find(setupId);

            var firstLine = reader.ReadLine();

            while (!reader.EndOfStream)
            {
                var Line = reader.ReadLine();
                var values = Line.Split(';');

                int id = Convert.ToInt32(values[0]);
                var q = db.Questions.Where(que => que.SubjectId == s.SubjectId && que.QuestionFileId == id).ToList().First();
               

                Answer a = new Answer();
                a.Text = values[1];
                a.Question = q;
                a.SetupId = setupId;
                db.Answers.Add(a);
                db.SaveChanges();

            }

            return RedirectToAction("Details", new { id = setupId });
        }

        [HttpPost]
        public ActionResult SetStartDate(int setupId, string date)
        {
            var l = date.Split('/');
            Setup setup = db.Setups.Find(setupId);
            setup.StartDate = new DateTime(Convert.ToInt32(l[2]), Convert.ToInt32(l[0]),Convert.ToInt32(l[1]));
            db.Entry(setup).State = EntityState.Modified;
            db.SaveChanges();

            return new HttpStatusCodeResult((int)HttpStatusCode.OK);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
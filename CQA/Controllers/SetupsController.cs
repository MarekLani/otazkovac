using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
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

        [HttpGet]
        public ActionResult Questions(int id)
        {
            ViewData["SetupId"] = id;
            var setup = db.Setups.Find(id);
            ViewData["SetupName"] = setup.Name;
            ViewData["SubjectName"] = setup.Subject.Name;
            var questions = db.Setups.Find(id).Questions;
            return View(questions);
        }

        [HttpPost]
        public ActionResult AddQuestions(HttpPostedFileBase file, int setupId)
        {
            var reader = new StreamReader(file.InputStream, Encoding.UTF8);
            List<string> columnNames = new List<string>();


            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            var firstLine = reader.ReadLine();

            while (!reader.EndOfStream)
            {
                var Line = reader.ReadLine();
                var values = Line.Split(';');

                Question q = db.Questions.Find(Convert.ToInt32(values[0]));
                if ( q != null)
                {
                    FillupQuestion(ref q, values);
                    db.Entry(q).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    Question newQ = new Question();
                    FillupQuestion(ref newQ, values);
                    newQ.SetupId = setupId;
                    db.Questions.Add(newQ);
                    db.SaveChanges();
                }

            }

            return RedirectToAction("Questions", new { id=setupId });
        }

        private void FillupQuestion(ref Question q, string[] values)
        {
            try
            {
                q.QuestionId = Convert.ToInt32(values[0]);
                q.QuestionText = values[2];
                //If hint is present
                if (values[4] != "")
                    q.Hint = values[4];
                else
                    q.Hint = null;
                q.IsActive = (1 == Convert.ToInt32(values[5]));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
            }
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

                List<UserProfile> users = db.UserProfiles.OrderBy(u => u.Evaluations.Count()).ToList();
                foreach (UserProfile u in users)
                {
                    sw.WriteLine(u.UserId + ";" + u.Evaluations.Count());
                }
                
            }
            ViewBag.Link = Request.Url.GetLeftPart(UriPartial.Authority)+"/Reports/stats.csv";
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
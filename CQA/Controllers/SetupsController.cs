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

namespace CQA.Controllers
{
    public class SetupsController : Controller
    {
        private CQADBContext db = new CQADBContext();

        //
        // GET: /Setups/

        public ActionResult Index()
        {
            var setup = db.Setup.Include(s => s.Subject);
            return View(setup.ToList());
        }

        //
        // GET: /Setups/Details/5

        public ActionResult Details(int id = 0)
        {
            Setup setup = db.Setup.Find(id);
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
            return View();
        }

        //
        // POST: /Setups/Create

        [HttpPost]
        public ActionResult Create(Setup setup)
        {
            if (ModelState.IsValid)
            {
                db.Setup.Add(setup);
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
            Setup setup = db.Setup.Find(id);
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
            if (ModelState.IsValid)
            {
                db.Entry(setup).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SubjectId = new SelectList(db.Subjects, "SubjectId", "Name", setup.SubjectId);
            return View(setup);
        }

        //
        // GET: /Setups/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Setup setup = db.Setup.Find(id);
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
            Setup setup = db.Setup.Find(id);
            db.Setup.Remove(setup);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public void AddQuestions(HttpPostedFileBase file, int setupId)
        {
            var reader = new StreamReader(file.InputStream);
            List<string> columnNames = new List<string>();
            //List<QARating> data = new List<QARating>();

            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            var firstLine = reader.ReadLine();

            //var colNames = firstLine.Split(',');

            //foreach (var value in colNames)
            //{
            //    columnNames.Add(value);
            //}

            while (!reader.EndOfStream)
            {

                var Line = reader.ReadLine();
                var values = firstLine.Split(',');

                var q = db.Questions.Find(values[0]);
                if(q != null){
                    q.QuestionText = values[1];
                    if(values.Count() == 3)
                        q.Hint = values[2];
                }
                else{
                    Question newQ = new Question();
                    newQ.QuestionId = Convert.ToInt32(values[0]);
                    newQ.QuestionText = values[1];
                    //If hint is present
                    if (values.Count() == 3)
                        newQ.Hint = values[2];
                    newQ.SetupId = setupId;
                    db.Questions.Add(newQ);
                }

            }

            db.SaveChanges();
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
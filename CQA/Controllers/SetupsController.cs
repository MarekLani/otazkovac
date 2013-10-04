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
            return View();
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
            var questions = db.Setups.Find(id).Questions;
            return View(questions);
        }

        [HttpPost]
        public ActionResult AddQuestions(HttpPostedFileBase file, int setupId)
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
                var values = Line.Split(';');

                Question newQ = new Question();
                newQ.QuestionId = Convert.ToInt32(values[0]);
                newQ.QuestionText = values[1];
                //If hint is present
                if (values.Count() == 3)
                    newQ.Hint = values[2];
                newQ.SetupId = setupId;

                if(db.Questions.Find(Convert.ToInt32(values[0])) != null)
                    db.Entry(newQ).State = EntityState.Modified;
                else
                    db.Questions.Add(newQ);

            }

            db.SaveChanges();
            return RedirectToAction("Details", new { id=setupId });
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
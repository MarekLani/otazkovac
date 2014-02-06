using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CQA.Models;

namespace CQA.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class SubjectsController : Controller
    {
        private CQADBContext db = new CQADBContext();

        //
        // GET: /Subjects/

        public ActionResult Index()
        {
            return View(db.Subjects.ToList());
        }

        //
        // GET: /Subjects/Details/5

        public ActionResult Details(int id = 0)
        {
            Subject subject = db.Subjects.Find(id);
            if (subject == null)
            {
                return HttpNotFound();
            }
            return View(subject);
        }

        //
        // GET: /Subjects/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Subjects/Create

        [HttpPost]
        public ActionResult Create(Subject subject)
        {
            if (ModelState.IsValid)
            {
                db.Subjects.Add(subject);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(subject);
        }

        //
        // GET: /Subjects/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Subject subject = db.Subjects.Find(id);
            if (subject == null)
            {
                return HttpNotFound();
            }
            return View(subject);
        }

        //
        // POST: /Subjects/Edit/5

        [HttpPost]
        public ActionResult Edit(Subject subject)
        {
            if (ModelState.IsValid)
            {
                db.Entry(subject).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(subject);
        }

        //
        // GET: /Subjects/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Subject subject = db.Subjects.Find(id);
            if (subject == null)
            {
                return HttpNotFound();
            }
            return View(subject);
        }

        //
        // POST: /Subjects/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Subject subject = db.Subjects.Find(id);
            db.Subjects.Remove(subject);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddConcept(Concept concept)
        {
            if (ModelState.IsValid)
            {
                concept.ActiveWeeks = "";
                db.Concepts.Add(concept);
                db.SaveChanges();
            }

            return new HttpStatusCodeResult((int)HttpStatusCode.Created);
        }

        [HttpPost]
        public ActionResult RemoveConcept(int conceptId)
        {
            db.Concepts.Remove(db.Concepts.Find(conceptId));
            db.SaveChanges();
            return new HttpStatusCodeResult((int)HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult ActivateConcept()
        {
            Request.InputStream.Seek(0, SeekOrigin.Begin);
            string jsonData = new StreamReader(Request.InputStream).ReadToEnd();
            Concept concept = new JavaScriptSerializer().Deserialize<Concept>(jsonData);
            Concept original = db.Concepts.Find(concept.ConceptId);

            List<int> activeWeeks = new List<int>(original.ActiveWeeksList);
            if (!activeWeeks.Contains(Convert.ToInt32(concept.ActiveWeeks)))
                activeWeeks.Add(Convert.ToInt32(concept.ActiveWeeks));

            original.ActiveWeeksList = activeWeeks;
            db.Entry(original).State = EntityState.Modified;
            db.SaveChanges();

            return new HttpStatusCodeResult((int)HttpStatusCode.Created);
        }

        struct AutocompletedConcepts
        {
            public int id;
            public string label;
            public string value;
        }

        public ActionResult SearchConcepts(int subjectId, string term, int questionId)
        {
            List<AutocompletedConcepts> result = new List<AutocompletedConcepts>();
            Question que = db.Questions.Find(questionId);
            foreach(var concept in db.Concepts.Where(c => c.Value.StartsWith(term) && c.SubjectId == subjectId))
            {
                if (!concept.Questions.Where(c => c.QuestionId == questionId).ToList().Any())
                {
                    AutocompletedConcepts c = new AutocompletedConcepts();
                    c.id = concept.ConceptId;
                    c.label = concept.Value;
                    c.value = concept.Value;
                    result.Add(c);
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        struct QuestionConcept
        {
            public int ConceptId;
            public int QuestionId;
        }

        public ActionResult AssignConceptToQuestion()
        {
            Request.InputStream.Seek(0, SeekOrigin.Begin);
            string jsonData = new StreamReader(Request.InputStream).ReadToEnd();

            QuestionConcept qc = new JavaScriptSerializer().Deserialize<QuestionConcept>(jsonData);

            Concept c = db.Concepts.Find(qc.ConceptId);
            c.Questions.Add(db.Questions.Find(qc.QuestionId));
            db.Entry(c).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new { value = c.Value, id = c.ConceptId }) ;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
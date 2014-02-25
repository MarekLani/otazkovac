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

            StringBuilder sb = new StringBuilder("");

            sb.Append(String.Format("<div id=\"{0}\" class=\"concept\">",concept.ConceptId));
            sb.Append(String.Format("<span class=\"concept\">{0}</span>",concept.Value));

            for(int i = 0; i < 13; i++)
            {
                sb = sb.Append(String.Format("<input class=\"activeCheckbox\" type=\"checkbox\" data-conceptid=\"{0}\" data-week=\"{1}\">",concept.ConceptId,i));
            }
            
            sb.Append("<form method=\"post\" data-ajax-url=\"/Subjects/RemoveConcept\" data-ajax-method=\"POST\" data-ajax=\"true\" autocomplete=\"off\" novalidate=\"novalidate\">");
            sb.Append(String.Format("<input type=\"hidden\" value=\"{0}\" name=\"conceptId\">",concept.ConceptId));
            sb.Append("<input class=\"btn removeConcept\" type=\"submit\" value=\"Odobrať koncept\">");
            sb.Append("</form>");
            sb.Append("<br>");
            sb.Append("</div");

            return Json(new { conceptWeeks = sb.ToString(),  });
        }

        [HttpPost]
        public ActionResult RemoveConcept()
        {
            Request.InputStream.Seek(0, SeekOrigin.Begin);
            string jsonData = new StreamReader(Request.InputStream).ReadToEnd();
            Concept concept = new JavaScriptSerializer().Deserialize<Concept>(jsonData);
            db.Concepts.Remove(db.Concepts.Find(concept.ConceptId));
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

            return Json(new { week = Convert.ToInt32(concept.ActiveWeeks), weekQuestionsCount = _QuestionInWeekCount(Convert.ToInt32(concept.ActiveWeeks),original) });
        }

        [HttpPost]
        public ActionResult DeactivateConcept()
        {
            Request.InputStream.Seek(0, SeekOrigin.Begin);
            string jsonData = new StreamReader(Request.InputStream).ReadToEnd();
            Concept concept = new JavaScriptSerializer().Deserialize<Concept>(jsonData);
            Concept original = db.Concepts.Find(concept.ConceptId);

            List<int> activeWeeks = new List<int>(original.ActiveWeeksList);
            if (activeWeeks.Contains(Convert.ToInt32(concept.ActiveWeeks)))
                activeWeeks.Remove(Convert.ToInt32(concept.ActiveWeeks));

            original.ActiveWeeksList = activeWeeks;
            db.Entry(original).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new { week = Convert.ToInt32(concept.ActiveWeeks), weekQuestionsCount = _QuestionInWeekCount(Convert.ToInt32(concept.ActiveWeeks), original) });
        }

        [HttpPost]
        public ActionResult DeleteConcept()
        {
            Request.InputStream.Seek(0, SeekOrigin.Begin);
            string jsonData = new StreamReader(Request.InputStream).ReadToEnd();
            Concept concept = new JavaScriptSerializer().Deserialize<Concept>(jsonData);
            Concept c = db.Concepts.Find(concept.ConceptId);

            db.Concepts.Remove(c);
            db.SaveChanges();

            return new HttpStatusCodeResult((int)HttpStatusCode.OK);
        }


        private int _QuestionInWeekCount(int week, Concept concept)
        {
            List<Question> questions = new List<CQA.Models.Question>();
            foreach (var con in concept.Subject.Concepts)
            {

                if (con.ActiveWeeksList.Contains(week))
                {
                    foreach (var q in con.Questions)
                    {
                        if (!questions.Contains(q))
                        {
                            questions.Add(q);
                        }
                    }
                }
            }
            return questions.Count;
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

        [HttpPost]
        public string AssignConceptToQuestion()
        {
            Request.InputStream.Seek(0, SeekOrigin.Begin);
            string jsonData = new StreamReader(Request.InputStream).ReadToEnd();

            QuestionConcept qc = new JavaScriptSerializer().Deserialize<QuestionConcept>(jsonData);

            Concept c = db.Concepts.Find(qc.ConceptId);
            c.Questions.Add(db.Questions.Find(qc.QuestionId));
            db.Entry(c).State = EntityState.Modified;
            db.SaveChanges();

            string result = String.Format(@"<span>"+
                    "<span class=\"concept\">{0} </span>"+
                    "<input type=\"hidden\" class=\"questionId\" value=\"{1}\" />" +
                    "<input type=\"hidden\" class=\"conceptId\" value=\"{2}\" />" +
                    "<b class=\"removeConcept\">x</b>" +
                "</span>", c.Value,qc.QuestionId,c.ConceptId );

            return result ;
        }

        [HttpPost]
        public ActionResult RemoveConceptFromQuestion()
        {
            Request.InputStream.Seek(0, SeekOrigin.Begin);
            string jsonData = new StreamReader(Request.InputStream).ReadToEnd();

            QuestionConcept qc = new JavaScriptSerializer().Deserialize<QuestionConcept>(jsonData);

            Concept c = db.Concepts.Find(qc.ConceptId);
            c.Questions.Remove(db.Questions.Find(qc.QuestionId));
            db.Entry(c).State = EntityState.Modified;
            db.SaveChanges();

            return new HttpStatusCodeResult((int)HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult AddQuestions(HttpPostedFileBase file, int subjectId)
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

                int id = Convert.ToInt32(values[0]);
                var l = db.Questions.Where(que => que.SubjectId == subjectId && que.QuestionFileId == id).ToList();
                Question q = null;
                if (l.Any())
                    q = l.First();
                if (q != null)
                {
                    FillupQuestion(ref q, values);
                    db.Entry(q).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    Question newQ = new Question();
                    FillupQuestion(ref newQ, values);
                    newQ.SubjectId = subjectId;
                    db.Questions.Add(newQ);
                    db.SaveChanges();
                }

            }

            return RedirectToAction("Questions", new { id = subjectId });
        }

        [HttpPost]
        public ActionResult AddQuestionsAnswers(HttpPostedFileBase file, int subjectId)
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

                int id = Convert.ToInt32(values[0]);
                var l = db.Questions.Where(que => que.SubjectId == subjectId && que.QuestionFileId == id).ToList();
                Question q = null;
                if (l.Any())
                    q = l.First();
                if (q == null)
                {
                    Question newQ = new Question();
                    newQ.QuestionFileId = Convert.ToInt32(values[0]);
                    newQ.QuestionText = values[1];
                    newQ.ImageUri = values[2];
                    newQ.SubjectId = subjectId;
                    newQ.Concepts = new List<Concept>();
                    newQ.IsActive = true;
                    db.Questions.Add(newQ);
                    db.SaveChanges();

                    string[] concepts = values[3].Split(',');
                    var conceptsList = concepts.ToList();
                    conceptsList.RemoveAt(concepts.Count() - 1);
                    foreach (var concept in conceptsList)
                    {

                        if (!db.Concepts.Where(c => c.SubjectId == subjectId && c.Value == concept).ToList().Any())
                        {
                            Concept c = new Concept();
                            c.Value = concept;
                            c.SubjectId = subjectId;
                            db.Concepts.Add(c);
                            db.SaveChanges();

                            newQ.Concepts.Add(c);
                            db.Entry(newQ).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else newQ.Concepts.Add(db.Concepts.Where(c => c.SubjectId == subjectId && c.Value == concept).First());


                    }
                    q = newQ;
                }

                Answer a = new Answer();
                a.Text =values[5];
                a.Question = q;
                db.Answers.Add(a);
                db.SaveChanges();

            }

            return RedirectToAction("Questions", new { id = subjectId });
        }

        [HttpGet]
        public ActionResult Questions(int id)
        {
            ViewData["SubjectId"] = id;
            var subject = db.Subjects.Find(id);
            ViewData["SubjectName"] = subject.Name;
            var questions = db.Subjects.Find(id).Questions;
            return View(questions);
        }

        private void FillupQuestion(ref Question q, string[] values)
        {
            try
            {
                q.QuestionFileId = Convert.ToInt32(values[0]);
                q.QuestionText = values[2];
                //If hint is present
                if (values[4] != "")
                    q.Hint = values[4];
                else
                    q.Hint = null;
                q.IsActive = (1 == Convert.ToInt32(values[5]));
                q.ImageUri = values[6];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
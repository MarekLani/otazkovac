using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CQA.Models;
using WebMatrix.WebData;
using System.DirectoryServices.Protocols;
using System.Security.Permissions;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

namespace CQA.Controllers
{
    [Authorize]
    public class QuestionsController : Controller
    {
        private CQADBContext db = new CQADBContext();
        // static variables used throughout the example

        //
        // GET: /Questions/

        public ActionResult Index()
        {

            return View(db.Questions.ToList());
        }

        //
        // GET: /Questions/Details/5

        public ActionResult Details(int id = 0)
        {

            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        //
        // GET: /Questions/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Questions/Create

        [HttpPost]
        public ActionResult Create(Question question)
        {
            if (ModelState.IsValid)
            {
                question.Author = db.UserProfiles.Find(WebSecurity.CurrentUserId);
                db.Questions.Add(question);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(question);
        }

        //
        // GET: /Questions/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        //
        // POST: /Questions/Edit/5

        [HttpPost]
        public ActionResult Edit(Question question)
        {
            if (ModelState.IsValid)
            {
                db.Entry(question).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(question);
        }

        //
        // GET: /Questions/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        //
        // POST: /Questions/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Question question = db.Questions.Find(id);
            db.Questions.Remove(question);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CreateAnswer(int questionId, string text)
        {
            if (ModelState.IsValid)
            {
                if (db.Answers.Where(a => a.QuestionId == questionId && a.UserId == WebSecurity.CurrentUserId).Any())
                {
                    ModelState.AddModelError("", "Na otázku ste už odpovedali");
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(false);
                }

                var answer = new Answer();
                answer.UserId = WebSecurity.CurrentUserId;// = db.UserProfiles.Find(WebSecurity.CurrentUserId);
                answer.QuestionId = questionId;//db.Questions.Find(questionId);
                answer.Text = text;
                db.Answers.Add(answer);
                db.SaveChanges();

                object result = new { answerText = text, answerAuthor = db.UserProfiles.Find(WebSecurity.CurrentUserId).RealName, answerId = answer.AnswerId };
                return Json(result);
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(false);
            }
        }

        [HttpPost]
        public ActionResult CreateRating(int answerId, int rating)
        {
            if (ModelState.IsValid)
            {
                if (db.Ratings.Where(a => a.AnswerId == answerId && a.UserId == WebSecurity.CurrentUserId).Any())
                {
                    ModelState.AddModelError("", "Odpoveď ste už hodnotili");
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(false);
                }

                var r = new Rating();
                r.UserId= WebSecurity.CurrentUserId;
                r.AnswerId = answerId;
                r.Value = (double)rating/100;
                db.Ratings.Add(r);
                db.SaveChanges();

            }

            return Json(true);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
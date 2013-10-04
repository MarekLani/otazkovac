﻿using System;
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

    public enum UserActionType
    {
        Evaluation = 1,
        Answering,
        AnsweringWithHint,
        EvaluationWithHint,
        SkippedAnswering,
        SkippedEvaluation

    }

    [Authorize]
    public class QuestionsController : Controller
    {


        private CQADBContext db = new CQADBContext();

        //
        // GET: /Questions/

        public ActionResult Index()
        {
           
           //Else return unfortunately there is nothing to return 
            return Json(false);
          
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

        [HttpGet]
        public ActionResult GetHint(int questionId)
        {
            if(db.Questions.Find(questionId).Hint != null){
                return Json(new { Hint = db.Questions.Find(questionId).Hint});
            }
            else{
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(false);
            }
        }

        [HttpPost]
        public ActionResult CreateEvaluation(int answerId, int value)
        {
            if (ModelState.IsValid)
            {
                if (db.Ratings.Where(a => a.AnswerId == answerId && a.UserId == WebSecurity.CurrentUserId).Any())
                {
                    ModelState.AddModelError("", "Odpoveď ste už hodnotili");
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(false);
                }

                var e = new Evaluation();
                e.UserId= WebSecurity.CurrentUserId;
                e.AnswerId = answerId;
                e.Value = (double)value/100;
                db.Ratings.Add(e);
                db.SaveChanges();

            }

            return Json(true);
        }

        [HttpGet]
        public ActionResult AnswerAndEvaluate(int setupId)
        {
            Random rand = new Random();
            if (rand.Next(4) != 4)
            {
                //user is going to validate answer
                var answers = db.Answers.OrderByDescending(a => a.Evaluations.Count())
                                    .Where(a => !a.Evaluations.Where(r => r.UserId == WebSecurity.CurrentUserId).Any() && a.Evaluations.Count() < 17 && a.Question.SetupId == setupId);
                var res = answers.ToList();
                if (res.Any())
                {
                    int n = res.First().Evaluations.Count();
                    var bestRatedAnswers = res.Where(a => a.Evaluations.Count() == n);
                    return View("Evaluate", bestRatedAnswers.ElementAt(rand.Next(bestRatedAnswers.Count() - 1)));

                }
            }

            var questions = db.Questions.OrderBy(q => q.Answers.Count())
                                .Where(q => !q.Answers.Where(r => r.UserId == WebSecurity.CurrentUserId).Any() && q.SetupId == setupId );
            var res2 = questions.ToList();
            if (res2.Any())
            {
                int n = res2.First().Answers.Count();
                var worstAnsweredQuestions = res2.Where(a => a.Answers.Count() == n);
                return View("Answer", worstAnsweredQuestions.ElementAt(rand.Next(worstAnsweredQuestions.Count() - 1)));

            }
            return Json(false);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
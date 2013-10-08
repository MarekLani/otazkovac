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

    public enum UserActionType
    {
        Evaluation = 1,
        Answering,
        ShowHint,
        SkippedAnswering,
        SkippedEvaluation

    }

    [Authorize]
    public class QuestionsAndAnswersController : Controller
    {

        private CQADBContext db = new CQADBContext();

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

                UserMadeAction((int)UserActionType.Answering, 0,questionId);

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
            Question q = db.Questions.Find(questionId);
            if(q != null && q.Hint != null){

                if (!db.UsersActions.Where(usa => usa.UserId == WebSecurity.CurrentUserId && usa.QuestionId == questionId).Any())
                {
                    UserMadeAction((int)UserActionType.ShowHint, 0,questionId);
                }

                return Json(new { Hint = db.Questions.Find(questionId).Hint }, JsonRequestBehavior.AllowGet);
            }
            else{
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(false, JsonRequestBehavior.AllowGet);
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

                //Check if we already have 3 evaluations and if the evaluated answer can be shown to answer
                var answer = db.Answers.Find(answerId);
                if (answer.Evaluations.Count == 3)
                {
                    answer.SeenEvaluation = false;
                    db.Entry(answer).State = EntityState.Modified;
                    db.SaveChanges();
                }

                //Mark action
                UserMadeAction((int)UserActionType.Evaluation, answerId,0);
            }

            return Json(true);
        }

        private void UserMadeAction(int action, int answerId, int questionId)
        {
            UsersAction ua = new UsersAction();
            ua.UserId = WebSecurity.CurrentUserId;

            if (answerId != 0)
            {
                ua.AnswerId = answerId;
            }
            else
            {
                ua.QuestionId = questionId;
            }
            
            ua.Action = action;
            db.UsersActions.Add(ua);
            db.SaveChanges();
        }

        [HttpGet]
        public ActionResult AnswerAndEvaluate(int setupId)
        {

            if (db.UsersSetups.Where( us => us.UserId == WebSecurity.CurrentUserId && us.SetupId == setupId ).ToList() == null)
            {
                UsersSetup us = new UsersSetup();
                us.UserId = WebSecurity.CurrentUserId;
                us.Score = 0;
                us.SetupId = setupId;
                db.UsersSetups.Add(us);
            }

            Random rand = new Random();
            if (rand.Next(4) != 4)
            {
                //user is going to validate answer
                var answers = db.Answers.OrderByDescending(a => a.Evaluations.Count())
                                    .Where(a =>/* !a.Evaluations.Where(r => r.UserId == WebSecurity.CurrentUserId).Any()) &&*/ a.Evaluations.Count() < 17 && a.Question.SetupId == setupId);
                var res = answers.ToList();
                if (res.Any())
                {
                    int n = res.First().Evaluations.Count();
                    var bestRatedAnswers = res.Where(a => a.Evaluations.Count() == n);
                    return View("Evaluate", bestRatedAnswers.ElementAt(rand.Next(bestRatedAnswers.Count())));

                }
            }

            var questions = db.Questions.OrderBy(q => q.Answers.Count())
                                .Where(q => !q.Answers.Where(r => r.UserId == WebSecurity.CurrentUserId).Any() && q.SetupId == setupId );
            var res2 = questions.ToList();
            if (res2.Any())
            {
                int n = res2.First().Answers.Count();
                var worstAnsweredQuestions = res2.Where(a => a.Answers.Count() == n);
                return View("Answer", worstAnsweredQuestions.ElementAt(rand.Next(worstAnsweredQuestions.Count())));

            }
            return Json(false);
        }

        public ActionResult MyEvaluatedAnswers()
        {
            var Answers = db.Answers.Where(a => a.SeenEvaluation != null && a.UserId == WebSecurity.CurrentUserId);
            return View(Answers);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
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
                answer.UserId = WebSecurity.CurrentUserId;
                answer.QuestionId = questionId;
                answer.Text = text;
                db.Answers.Add(answer);
                db.SaveChanges();

                UserMadeAction((int)UserActionType.Answering, 0,questionId);

                UserSeenQuestion(questionId);

                object result = new { answerText = text };
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
                if (answer.Evaluations.Count == MyConsts.MinEvaluationLimit)
                {
                    answer.SeenEvaluation = false;
                    db.Entry(answer).State = EntityState.Modified;
                    db.SaveChanges();
                }

                //Mark action
                UserMadeAction((int)UserActionType.Evaluation, answerId,0);

                //Mark question as seen
                UserSeenQuestion(answer.QuestionId);
                
                db.SaveChanges();
            }

            return Json(true);
        }

        /// <summary>
        /// Finds and update/creates new record of viewed question by user
        /// </summary>
        /// <param name="questionId"></param>
        private void UserSeenQuestion(int questionId)
        {
            QuestionView qv = db.QuestionViews.Find(WebSecurity.CurrentUserId,questionId);
            if (qv == null)
            {
                qv = new QuestionView();
                qv.QuestionId = questionId;
                qv.UserId = WebSecurity.CurrentUserId;
                qv.ViewDate = DateTime.Now;
                db.QuestionViews.Add(qv);
            }
            else
            {
                qv.ViewDate = DateTime.Now;
                db.Entry(qv).State = EntityState.Modified;
            }

            db.SaveChanges();
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
            var defaultQuestionView = new QuestionView();
            defaultQuestionView.ViewDate = DateTime.MinValue;

            if (db.UsersSetups.Where( us => us.UserId == WebSecurity.CurrentUserId && us.SetupId == setupId ).ToList() == null)
            {
                UsersSetup us = new UsersSetup();
                us.UserId = WebSecurity.CurrentUserId;
                us.Score = 0;
                us.SetupId = setupId;
                db.UsersSetups.Add(us);
            }

            Random rand = new Random();

            //Base on probability set for setup it is selected if user is going to evaluate or answer
            int p = db.Setups.Find(setupId).AnsweringProbability;
            if (rand.Next(p) != p-1)
            {
                //user is going to validate answer

                //Algorythm for selection of answer to be evaluated looks like this:
                //1. Find all answers, which have not been already evaluated by current user,
                //   which were not written by current user, which are in current setup and for active question
                //2. With greedy maximization principle, try to find questions which have fewer than 3 evaluations (MinEvaluationLimit)
                //  and which question was not seen by current user in last day, if such a answer exists ruturn it
                //3. If no such a question exists try to find question which have fewer then 16 (FullEvaluationLimit) evaluations with
                // the same proccedure as before. 
                //4.If there is no answer sleected byt his rules, ignore 1 day rule and try to find
                // answer as before, but without the one day rule.
                //5. If there are also no results, try to find question to be answered

                //step 1
                var answers = db.Answers
                                    .Where(a => !a.Evaluations.Where(e => e.UserId == WebSecurity.CurrentUserId).Any()
                                        && a.Question.SetupId == setupId
                                        && a.UserId != WebSecurity.CurrentUserId
                                        && a.Question.IsActive).ToList();

                if (answers.Any())
                {
                    //step 2
                    var bottomGreedy = answers
                                .Where(a => a.Question.QuestionViews.Where(qv => qv.UserId == WebSecurity.CurrentUserId)
                                    .DefaultIfEmpty(defaultQuestionView).Single().ViewDate.AddDays(1) < DateTime.Now
                                    && a.Evaluations.Count < MyConsts.MinEvaluationLimit).OrderByDescending(a => a.Evaluations.Count()).ToList();
                    if (bottomGreedy.Any())
                    {
                        int n = bottomGreedy.First().Evaluations.Count();
                        var bestEvaluatedAnswers = bottomGreedy.Where(a => a.Evaluations.Count() == n);
                        return View("Evaluate", bestEvaluatedAnswers.ElementAt(rand.Next(bestEvaluatedAnswers.Count())));
                    }
                    else
                    {
                        //step 3 and 4
                        var upperGreedy = answers
                            .Where(a => a.Evaluations.Count < MyConsts.FullEvaluationLimit)
                            .OrderByDescending(a => a.Evaluations.Count()).ToList();
                        if (upperGreedy.Any())
                        {
                            var UnseenAnswers = upperGreedy.Where(a => a.Question.QuestionViews.Where(qv => qv.QuestionId == a.QuestionId && qv.UserId == WebSecurity.CurrentUserId)
                                .DefaultIfEmpty(defaultQuestionView).Single().ViewDate.AddDays(1) < DateTime.Now).ToList();
                            if (UnseenAnswers.Any())
                                upperGreedy = UnseenAnswers;
                            int n = upperGreedy.First().Evaluations.Count();
                            var bestEvaluatedAnswers = upperGreedy.Where(a => a.Evaluations.Count() == n);
                            return View("Evaluate", bestEvaluatedAnswers.ElementAt(rand.Next(bestEvaluatedAnswers.Count())));
                        }
                    }
                }
            }

            //Choosing a question to be answered
            //Algorythm looks like this:
            //1. Choose active questions from current setup, which was not already answered by current user
            //2. From this questions choose only those, which answers were not evaluated by current user in last two days
            //3. If no questions are selected in step two, ignore 2 days rule and select from all questions

            //Step 1 take active questions which user has not answered yet
            var tempQuestions = db.Questions.Where( q => !q.Answers.Where(r => r.UserId == WebSecurity.CurrentUserId).Any() &&
                q.IsActive &&
                q.SetupId == setupId).OrderByDescending(q => q.Answers.Count()).ToList();

            if (tempQuestions.Any())
            {
                //Step2 Check if there are questions to be answered, which have not been seen by current user in last two days
                //TODO check if the order of questions stays as it was
                var questions = tempQuestions.Where(q => q.QuestionViews.Where(qv => qv.UserId == WebSecurity.CurrentUserId)
                    .DefaultIfEmpty(defaultQuestionView).Single().ViewDate.AddDays(2) < DateTime.Now).OrderByDescending(q => q.Answers.Count()).ToList();
                
                //Step3 if there are no questions from step2 we have to ignore 2 days rule
                if (!questions.Any())
                    questions = tempQuestions;
                
                //Check if there is any question to be answered
                if(questions.Any()){
                    int n = questions.First().Answers.Count();
                    var worstAnsweredQuestions = questions.Where(a => a.Answers.Count() == n);
                    return View("Answer", worstAnsweredQuestions.ElementAt(rand.Next(worstAnsweredQuestions.Count())));
                }
            }

            return View("NothingToDoHere");
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
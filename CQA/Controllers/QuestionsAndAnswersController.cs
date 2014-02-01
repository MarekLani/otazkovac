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
using System.Web.Script.Serialization;

namespace CQA.Controllers
{

    public class QuestionsAndAnswersController : Controller
    {

        private CQADBContext db = new CQADBContext();

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateAnswer(int questionId, string text)
        {
            if (ModelState.IsValid)
            {
                RemoveHandledObjectFromSession(false);

                if (db.Answers.Where(a => a.QuestionId == questionId && a.UserId == WebSecurity.CurrentUserId).Any())
                {
                    ModelState.AddModelError("", "Na otázku ste už odpovedali");
                    return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
                }

                var answer = new Answer();
                answer.UserId = WebSecurity.CurrentUserId;
                answer.QuestionId = questionId;
                answer.Text = HttpUtility.HtmlDecode(text);
                db.Answers.Add(answer);
                db.SaveChanges();

                UserMadeAction(UserActionType.Answering, 0,questionId);
                UserSeenQuestion(questionId);

                object result = new { answerText = text };
                return Json(result);
            }
            else
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
        }  

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateEvaluation(int answerId, int value)
        {
            if (ModelState.IsValid)
            {
                RemoveHandledObjectFromSession(true);

                if (db.Ratings.Where(a => a.AnswerId == answerId && a.UserId == WebSecurity.CurrentUserId).Any())
                {
                    ModelState.AddModelError("", "Odpoveď ste už hodnotili");
                    return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
                }


                //Notifications
                Answer answer = db.Answers.Find(answerId);

                //Create Notification
                Notification not = new Notification();
                not.Answer = answer;
                not.User = answer.Author;
                not.NotificationFor = NotificationFor.MyAnswer;
                not.NotificationType = NotificationType.NewEvaluation;
                db.Notifications.Add(not);
                db.SaveChanges();

                //ForEach already assigned evaluation we need to create notifications too
                foreach (Evaluation eval in answer.Evaluations)
                {
                    Notification not2 = new Notification();
                    not2.Answer = eval.Answer;
                    not2.User = eval.Author;
                    not2.NotificationFor = NotificationFor.MyEvaluation;
                    not2.NotificationType = NotificationType.NewEvaluation;
                    db.Notifications.Add(not2);
                    db.SaveChanges();
                }

                var e = new Evaluation();
                e.UserId= WebSecurity.CurrentUserId;
                e.AnswerId = answerId;
                e.Value = (double)value/100;
                db.Ratings.Add(e);
                db.SaveChanges();

                //Mark action
                UserMadeAction(UserActionType.Evaluation, answerId,0);
                //Mark question as seen
                UserSeenQuestion(answer.QuestionId);
                
                db.SaveChanges();

                List<Comment> comments = db.Answers.Find(answerId).Comments.ToList();
                List<ViewComment> viewComments = new List<ViewComment>();
                foreach (Comment c in comments)
                {
                    ViewComment vc;
                    if (c.Anonymous)
                        vc = new ViewComment(c.Text, "Anonym", answerId);
                    else
                        vc = new ViewComment(c.Text, db.UserProfiles.Find(c.UserId).RealName, answerId);
                    viewComments.Add(vc);
                }
                var jsonSerialiser = new JavaScriptSerializer();
                var commentsInJson = jsonSerialiser.Serialize(viewComments);

                return Json(new { avgEval = answer.GetAvgEvaluation(), evalsCount = answer.Evaluations.Count(), comments = commentsInJson });
            }

            return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateComment(Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.UserId = WebSecurity.CurrentUserId;
                db.Comments.Add(comment);
                db.SaveChanges();

                Answer ans = db.Answers.Single(a => a.AnswerId == comment.AnswerId);
                //Create Notification
                if (ans.UserId != WebSecurity.CurrentUserId)
                {
                    Notification not = new Notification();
                    not.Answer = comment.Answer;
                    not.User = comment.Answer.Author;
                    not.NotificationFor = NotificationFor.MyAnswer;
                    not.NotificationType = NotificationType.NewComment;
                    db.Notifications.Add(not);
                    db.SaveChanges();
                }

                //ForEach already assigned evaluation we need to create notifications too
                foreach (Evaluation eval in ans.Evaluations)
                {
                    //We do not want to notify author of comment
                    if (eval.UserId != comment.UserId)
                    {
                        Notification not2 = new Notification();
                        not2.Answer = eval.Answer;
                        not2.User = eval.Author;
                        not2.NotificationFor = NotificationFor.MyEvaluation;
                        not2.NotificationType = NotificationType.NewComment;
                        db.Notifications.Add(not2);
                        db.SaveChanges();
                    }
                }

                //Mark action
                UserMadeAction(UserActionType.Commented, comment.AnswerId, 0);
                ViewComment vc;
                if(comment.Anonymous)
                    vc = new ViewComment( comment.Text, "Anonym", comment.AnswerId);
                else
                    vc = new ViewComment(comment.Text, db.UserProfiles.Find(comment.UserId).RealName, comment.AnswerId);
                var jsonSerialiser = new JavaScriptSerializer();
                return Json(jsonSerialiser.Serialize(vc));

            }

            return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
        }

        //GetHint - not being used
        //[HttpGet]
        //public ActionResult GetHint(int objectId, bool evaluating)
        //{
        //    if (!evaluating)
        //    {
        //        Question q = db.Questions.Find(objectId);
        //        if (q != null && q.Hint != null)
        //        {

        //            if (!db.UsersActions.Where(usa => usa.UserId == WebSecurity.CurrentUserId && usa.QuestionId == objectId).Any())
        //            {
        //                UserMadeAction(UserActionType.ViewedHintWhenAnswering, 0, objectId);
        //            }

        //            return Json(new { Hint = db.Questions.Find(objectId).Hint }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    else
        //    {
        //        Answer a = db.Answers.Find(objectId);
        //        if (a != null && a.Question.Hint != null)
        //        {
        //            UserMadeAction(UserActionType.ViewedHintWhenEvaluating, objectId, 0);
        //        }

        //        return Json(new { Hint = db.Questions.Find(objectId).Hint }, JsonRequestBehavior.AllowGet);
        //    }

        //    Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //    return Json(false, JsonRequestBehavior.AllowGet);

        //}

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

        /// <summary>
        /// Save action of user 
        /// set answerId = 0 if saving action connected with question
        /// otherwise set questionId = 0
        /// </summary>
        /// <param name="action"></param>
        /// <param name="answerId"></param>
        /// <param name="questionId"></param>
        private void UserMadeAction(UserActionType action, int answerId, int questionId)
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
        [Authorize]
        public ActionResult AnswerAndEvaluate(int setupId)
        {
            int skippedAnswerId = 0;
            int skippedQuestionId = 0;

            if (Session["SkippedQuestionId"] != null)
            {
                skippedQuestionId = (int)Session["SkippedQuestionId"];
                Session.Remove("SkippedQuestionId");
            }

            if (Session["SkippedAnswerId"] != null)
            {
                skippedAnswerId = (int)Session["SkippedAnswerId"];
                Session.Remove("SkippedAnswerId");
            }

            //check if setup is active
            Setup setup = db.Setups.Find(setupId);
            if (setup == null || !setup.Active)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            //Do not allow user to change answered question or evaluated answer by refresh
            if (Session != null && Session["SetupId"] != null && (int)Session["SetupId"] == setupId)
            {
                if ((bool)Session["IsUserEvaluating"])
                {
                    Answer a = db.Answers.Find((int)Session["AnswerId"]);
                    return View("Evaluate", a);
                }
                else
                {
                    Question q = db.Questions.Find((int)Session["QuestionId"]);
                    return View("Answer", q);
                }
            }

            if (db.UsersSetups.Where( us => us.UserId == WebSecurity.CurrentUserId && us.SetupId == setupId ).ToList() == null)
            {
                UsersSetup us = new UsersSetup();
                us.UserId = WebSecurity.CurrentUserId;
                us.Score = 0;
                us.SetupId = setupId;
                db.UsersSetups.Add(us);
            }

            Question que = null;
            Answer ans = null;

            ChooseAnswerOrQuestion(setupId, skippedAnswerId, skippedQuestionId, ref ans, ref que, WebSecurity.CurrentUserId);

            if (ans != null)
            {
                AddHandledObjectToSession(ans.AnswerId, true, setupId);
                return View("Evaluate", ans);
            }
            if (que != null)
            {
                AddHandledObjectToSession(que.QuestionId,false, setupId);
                return View("Answer", que);
            }

            if (skippedAnswerId == 0 && skippedQuestionId == 0)
                return View("NothingToDoHere");
            else
                return RedirectToAction("AnswerAndEvaluate", new { setupId = setupId });
        }

        [HttpGet]
        public ActionResult ExternAnswerAndEvaluate(string ais, string name, int setupId)
        {
            //check if setup is active
            Setup setup = db.Setups.Find(setupId);
            if (setup == null || !setup.Active)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            Question que = null;
            Answer ans = null;

            UserProfile user;

            if ((user = db.UserProfiles.Single(u => u.UserName == ais)) == null)
            {
                WebSecurity.CreateUserAndAccount(ais, "pass", new { RealName = name });
                user = db.UserProfiles.Single(u => u.UserName == ais);
            }

            ChooseAnswerOrQuestion(setupId, 0, 0, ref ans, ref que, user.UserId);

            if(ans != null)
                return Json(new { 
                    action = "Eval",
                    questionText = ans.Question.QuestionText,
                    answerText = ans.Text,
                    answerId = ans.AnswerId
                }, JsonRequestBehavior.AllowGet);
            if (que != null)
                return Json(new
                {
                    action = "Answer",
                    questionText = que.QuestionText,
                    questionId = que.QuestionId

                }, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                action = "No content"
            }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Returns true when answer or question was selected otherwise returns false
        /// </summary>
        /// <param name="setupId"></param>
        /// <param name="skippedAnswerId"></param>
        /// <param name="skippedQuestionId"></param>
        /// <param name="ans"></param>
        /// <param name="que"></param>
        /// <returns></returns>
        private bool ChooseAnswerOrQuestion(int setupId, int skippedAnswerId, int skippedQuestionId, ref Answer ans, ref Question que, int userId)
        {
            
            //Hack for selecting, when user has not seen the question, so there is no record to be selected
            //See selects lower using DefaultIfEmpty
            var defaultQuestionView = new QuestionView();
            defaultQuestionView.ViewDate = DateTime.MinValue;
            
            Random rand = new Random();

            //Base on probability set for setup it is selected if user is going to evaluate or answer
            int p = db.Setups.Find(setupId).AnsweringProbability;
            if (rand.Next(p) != p - 1)
            {
                //user is going to validate answer

                //Algorithm for selection of answer to be evaluated looks like this:
                //1. Find all answers, which have not been already evaluated by current user,
                //   which were not written by current user, which are in current setup and for active question
                //2. If answer was skipped remove it from list (if it is not the only one possible answer to be displayed) 
                //3. With greedy maximization principle, try to find questions which have fewer than 3 evaluations (MinEvaluationLimit)
                //  and which question was not seen by current user in last day, if such a answer exists ruturn it
                //4. If no such a question exists try to find question which have fewer then 16 (FullEvaluationLimit) evaluations with
                // the same proccedure as before. 
                //5.If there is no answer sleected in step 4, ignore 1 day rule and try to find
                // answer as before, but without the one day rule.
                //6. If there are also no results, try to find question to be answered

                //step 1
                var answers = db.Answers
                                    .Where(a => !a.Evaluations.Where(e => e.UserId == userId).Any()
                                        && a.Question.SetupId == setupId
                                        && a.UserId != userId
                                        && a.Question.IsActive).ToList();

                //Remove skipped answer (if was)
                if (skippedAnswerId != 0 && answers.Count() > 1)
                {
                    Answer delAns = answers.Find(a => a.AnswerId == skippedAnswerId);
                    if (delAns != null)
                        answers.Remove(delAns);
                }

                if (answers.Any())
                {
                    //step 2
                    var bottomGreedy = answers
                                .Where(a => a.Question.QuestionViews.Where(qv => qv.UserId == userId)
                                    .DefaultIfEmpty(defaultQuestionView).Single().ViewDate.AddDays(1) < DateTime.Now
                                    && a.Evaluations.Count < MyConsts.MinEvaluationLimit).OrderByDescending(a => a.Evaluations.Count()).ToList();
                    if (bottomGreedy.Any())
                    {
                        int n = bottomGreedy.First().Evaluations.Count();
                        var bestEvaluatedAnswers = bottomGreedy.Where(a => a.Evaluations.Count() == n);
                        ans = bestEvaluatedAnswers.ElementAt(rand.Next(bestEvaluatedAnswers.Count()));
                        return true;
                    }
                    else
                    {
                        //step 3 and 4
                        var upperGreedy = answers
                            .Where(a => a.Evaluations.Count < MyConsts.FullEvaluationLimit)
                            .OrderByDescending(a => a.Evaluations.Count()).ToList();

                        if (upperGreedy.Any())
                        {
                            var UnseenAnswers = upperGreedy.Where(a => a.Question.QuestionViews.Where(qv => qv.QuestionId == a.QuestionId && qv.UserId == userId)
                                .DefaultIfEmpty(defaultQuestionView).Single().ViewDate.AddDays(1) < DateTime.Now).ToList();
                            if (UnseenAnswers.Any())
                                upperGreedy = UnseenAnswers;
                            int n = upperGreedy.First().Evaluations.Count();
                            var bestEvaluatedAnswers = upperGreedy.Where(a => a.Evaluations.Count() == n);
                            ans = bestEvaluatedAnswers.ElementAt(rand.Next(bestEvaluatedAnswers.Count()));
                            return true;
                        }
                    }
                }
            }

            //Choosing a question to be answered
            //Algorithm looks like this:
            //1. Choose active questions from current setup, which was not already answered by current user
            //2. If question was skipped remove it from list (if it is not the only one possible question to be displayed) 
            //3. From this questions choose only those, which answers were not evaluated by current user in last two days
            //4. If no questions are selected in step two, ignore 2 days rule 
            //5. Choose random question with smallest number of answers 

            //Step 1 take active questions which user has not answered yet
            var tempQuestions = db.Questions.Where(q => !q.Answers.Where(r => r.UserId == userId).Any() &&
                q.IsActive &&
                q.SetupId == setupId).OrderBy(q => q.Answers.Count()).ToList();

            if (skippedQuestionId != 0 && tempQuestions.Count() > 1)
            {
                Question delQue = tempQuestions.Find(q => q.QuestionId == skippedQuestionId);
                if (delQue != null)
                    tempQuestions.Remove(delQue);
            }

            if (tempQuestions.Any())
            {
                //Step2 Check if there are questions to be answered, which have not been seen by current user in last two days
                //TODO check if the order of questions stays as it was
                var questions = tempQuestions.Where(q => q.QuestionViews.Where(qv => qv.UserId == userId)
                    .DefaultIfEmpty(defaultQuestionView).Single().ViewDate.AddDays(2) < DateTime.Now).ToList();

                //Step3 if there are no questions from step2 we have to ignore 2 days rule
                if (!questions.Any())
                    questions = tempQuestions;

                //Check if there is any question to be answered
                if (questions.Any())
                {
                    int n = questions.First().Answers.Count();
                    var worstAnsweredQuestions = questions.Where(a => a.Answers.Count() == n);
                    que = worstAnsweredQuestions.ElementAt(rand.Next(worstAnsweredQuestions.Count()));
                    return true;
                }
            }
            return false;
        }

        public ActionResult SkipAnswer(int questionId)
        {
            int setupId = db.Questions.Find(questionId).SetupId;
            UserMadeAction(UserActionType.SkippedAnswering, 0, questionId);
            RemoveHandledObjectFromSession(false);
            Session["SkippedQuestionId"] = questionId;  
            return RedirectToAction("AnswerAndEvaluate", new { setupId = setupId });
        }

        public ActionResult SkipEvaluation(int answerId, int setupId)
        {
            UserMadeAction(UserActionType.SkippedEvaluation, answerId, 0);
            RemoveHandledObjectFromSession(true);
            Session["SkippedAnswerId"] = answerId; 
            return RedirectToAction("AnswerAndEvaluate", new { setupId = setupId});
        }

        /// <summary>
        /// Adds Handled object to session to unable user to change handled object by refresh
        /// </summary>
        /// <param name="handledObjectId"></param>
        /// <param name="evaluating"></param>
        private void AddHandledObjectToSession(int handledObjectId,bool evaluating, int setupId)
        {
            Session["SetupId"] = setupId;
            if (evaluating)
            {
                Session["IsUserEvaluating"] = true;
                Session["AnswerId"] = handledObjectId;
            }
            else
            {
                Session["IsUserEvaluating"] = false;
                Session["QuestionId"] = handledObjectId;
            }
        }

        /// <summary>
        /// Removes handled object from session when user evaluated or answered
        /// </summary>
        /// <param name="evaluating"></param>
        private void RemoveHandledObjectFromSession(bool evaluating)
        {
            Session.Remove("SetupId");
            Session.Remove("IsUserEvaluating");
            if (evaluating)
            {
                Session.Remove("AnswerId");
            }
            else
            {
                Session.Remove("QuestionId");
            }

        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
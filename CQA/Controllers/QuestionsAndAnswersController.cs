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
using System.IO;

namespace CQA.Controllers
{

    public class QuestionsAndAnswersController : Controller
    {

        private CQADBContext db = new CQADBContext();

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateAnswer(int questionId, string text, int setupId)
        {
            if (ModelState.IsValid)
            {
                RemoveHandledObjectFromSession(false);

                if (db.Answers.Where(a => a.QuestionId == questionId && a.UserId == WebSecurity.CurrentUserId).Any())
                {
                    ModelState.AddModelError("", "Na otázku ste už odpovedali");
                    return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
                }

                var answer = new Answer(questionId, HttpUtility.HtmlDecode(text), WebSecurity.CurrentUserId, setupId);
                db.Answers.Add(answer);
                db.SaveChanges();

                UserMadeAction(UserActionType.Answering, 0, questionId, WebSecurity.CurrentUserId);
                UserSeenQuestion(questionId, WebSecurity.CurrentUserId);

                object result = new { answerText = text };
                return Json(result);
            }
            else
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ExternCreateAnswer()
        {
            Answer ans = new JavaScriptSerializer().Deserialize<Answer>(Request.Form["json"]);

            if (!db.UserProfiles.Where(u => u.UserId == ans.UserId).Any() ||
                db.Answers.Where(a => a.QuestionId == ans.QuestionId && a.UserId == ans.UserId).Any())
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            ans.Text = HttpUtility.HtmlEncode(ans.Text).Replace("\"", "'");

            db.Answers.Add(ans);
            db.SaveChanges();

            UserMadeAction(UserActionType.Answering, 0, ans.QuestionId, (int)ans.UserId);
            UserSeenQuestion(ans.QuestionId, (int)ans.UserId);

            Question que = db.Questions.Single(q => q.QuestionId == ans.QuestionId);

            object result = new { answerText = HttpUtility.HtmlDecode(ans.Text), answerId = ans.AnswerId, questionId = ans.QuestionId, questionFileId = que.QuestionFileId };
            return Json(result);
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
                CreateNotifications(answer, NotificationType.NewEvaluation, WebSecurity.CurrentUserId);

                var e = new Evaluation(WebSecurity.CurrentUserId, answerId, (double)value / 100);
                db.Ratings.Add(e);
                db.SaveChanges();

                //Mark action
                UserMadeAction(UserActionType.Evaluation, answerId, 0, WebSecurity.CurrentUserId);
                //Mark question as seen
                UserSeenQuestion(answer.QuestionId, WebSecurity.CurrentUserId);
                
                return Json(new { avgEval = answer.GetAvgEvaluation(), evalsCount = answer.Evaluations.Count(), comments = answer.GetAnswerComments() });
            }

            return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
        }

        [HttpPost]
        public ActionResult ExternCreateEvaluation()
        {

            Evaluation eval = new JavaScriptSerializer().Deserialize<Evaluation>(Request.Form["json"]);
            eval.Value = eval.Value / (double)100;

            if (!db.UserProfiles.Where(u => u.UserId == eval.UserId).Any() || 
                db.Ratings.Where(a => a.AnswerId == eval.AnswerId && a.UserId == eval.UserId).Any())
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            //Notifications
            Answer answer = db.Answers.Find(eval.AnswerId);
            CreateNotifications(answer, NotificationType.NewEvaluation, eval.UserId);

            db.Ratings.Add(eval);
            db.SaveChanges();

            //Mark action
            UserMadeAction(UserActionType.Evaluation, eval.AnswerId, 0, eval.UserId);
            //Mark question as seen
            UserSeenQuestion(eval.Answer.QuestionId, eval.UserId);

            return Json(new { avgEval = answer.GetAvgEvaluation(), evalsCount = answer.Evaluations.Count(), comments = answer.GetAnswerComments(), answerId = eval.AnswerId });
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult CreateComment(Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.UserId = WebSecurity.CurrentUserId;
                comment.Text = HttpUtility.HtmlEncode(comment.Text).Replace("\"","'");
                db.Comments.Add(comment);
                db.SaveChanges();

                Answer ans = db.Answers.Single(a => a.AnswerId == comment.AnswerId);
                //Create Notification
                CreateNotifications(ans, NotificationType.NewComment, comment.UserId);

                //Mark action
                UserMadeAction(UserActionType.Commented, comment.AnswerId, 0, WebSecurity.CurrentUserId);
                ViewComment vc;
                if(comment.Anonymous)
                    vc = new ViewComment( comment.Text, "Anonym", comment.AnswerId);
                else
                    vc = new ViewComment(comment.Text, db.UserProfiles.Find(comment.UserId).RealName, comment.AnswerId);

                vc.Text = HttpUtility.HtmlDecode(vc.Text); 

                return Json(vc);

            }

            return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ExternCreateComment()
        {

            Comment comment = new JavaScriptSerializer().Deserialize<Comment>(Request.Form["json"]);
            comment.Text = HttpUtility.HtmlEncode(comment.Text).Replace("\"", "'");

            db.Comments.Add(comment);
            db.SaveChanges();

            Answer ans = db.Answers.Single(a => a.AnswerId == comment.AnswerId);
            //Create Notification
            CreateNotifications(ans, NotificationType.NewComment, comment.UserId);

            //Mark action
            UserMadeAction(UserActionType.Commented, comment.AnswerId, 0, comment.UserId);
            ViewComment vc;
            if (comment.Anonymous)
                vc = new ViewComment(comment.Text, "Anonym", comment.AnswerId);
            else
                vc = new ViewComment(comment.Text, db.UserProfiles.Find(comment.UserId).RealName, comment.AnswerId);
            vc.Text = HttpUtility.HtmlDecode(vc.Text).Replace("\"", "'");
            return Json(vc);
        }


        private void CreateNotifications(Answer answer, NotificationType type, int authorId)
        {
            //Create Notification of author of answer (We do not want to notify author if he created comment/eval)
            if (answer.AnswerId != authorId && answer.UserId != null)
            {
                Notification not = new Notification();
                not.Answer = answer;
                not.User = answer.Author;
                not.NotificationFor = NotificationFor.MyAnswer;
                not.NotificationType = type;
                db.Notifications.Add(not);
                db.SaveChanges();
            }

            //ForEach already assigned evaluation we need to create notifications too
            foreach (Evaluation eval in answer.Evaluations)
            {
                if(eval.UserId != authorId){
                    Notification not2 = new Notification();
                    not2.Answer = eval.Answer;
                    not2.User = eval.Author;
                    not2.NotificationFor = NotificationFor.MyEvaluation;
                    not2.NotificationType = type;
                    db.Notifications.Add(not2);
                    db.SaveChanges();
                }
            }
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

        [HttpGet]
        [Authorize]
        public ActionResult AnswerAndEvaluate(int setupId)
        {
            int skippedAnswerId = 0;
            int skippedQuestionId = 0;
            ViewBag.setupId = setupId;

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
            if (setup == null || !setup.Active || !setup.Displayed)
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

            if (db.UsersSetups.SingleOrDefault(us => us.UserId == WebSecurity.CurrentUserId && us.SetupId == setupId) == null)
            {
                UsersSetup us = new UsersSetup();
                us.UserId = WebSecurity.CurrentUserId;
                us.Score = 0;
                us.SetupId = setupId;
                db.UsersSetups.Add(us);
                db.SaveChanges();
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

            //Create User Profile
            UserProfile user;
            if ((user = db.UserProfiles.FirstOrDefault(u => u.UserName == ais)) == null)
            {
                WebSecurity.CreateUserAndAccount(ais, "pass", new { RealName = name });
                user = db.UserProfiles.Single(u => u.UserName == ais);
            }

            if (db.UsersSetups.SingleOrDefault(us => us.UserId == user.UserId && us.SetupId == setupId) == null)
            {
                UsersSetup us = new UsersSetup();
                us.UserId = user.UserId;
                us.Score = 0;
                us.SetupId = setupId;
                db.UsersSetups.Add(us);
                db.SaveChanges();
            }

            ChooseAnswerOrQuestion(setupId, 0, 0, ref ans, ref que, user.UserId);

            if(ans != null){
                string imgUri = "";
                if(ans.Question.ImageUri != null)
                    imgUri = ans.Question.ImageUri;
                return Json(new { 
                    action = "Eval",
                    questionText = ans.Question.QuestionText.Replace("\"","'"),
                    answerText = ans.Text.Replace("\"", "'"),
                    answerId = ans.AnswerId,
                    setupId = ans.SetupId,
                    image = imgUri,
                    userId = user.UserId
                }, JsonRequestBehavior.AllowGet);
            }
            if (que != null)
            {
                if (que.ImageUri == null)
                    que.ImageUri = "";
                return Json(new
                {
                    action = "Answer",
                    questionText = que.QuestionText.Replace("\"", "'"),
                    questionId = que.QuestionId,
                    image = que.ImageUri,
                    userId = user.UserId

                }, JsonRequestBehavior.AllowGet);
            }
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

            Setup s = db.Setups.Find(setupId);

            int week = (int)(DateTime.Now-s.StartDate).TotalDays / 7;
            if(week > 13)
                week = 13;

            var concepts = db.Concepts.Where(c => c.SubjectId == s.SubjectId).ToList();
            List<Concept> activeConcepts = new List<Concept>();
            foreach(var c in concepts)
            {
                if(c.ActiveWeeksList.Contains(week))
                    activeConcepts.Add(c);
            }

            var ques = db.Questions.Where(q => q.SubjectId == s.SubjectId).ToList();
            List<Question> activeQuestions = new List<Question>();
            foreach (var q in ques)
            {
                if (q.Concepts.Intersect(activeConcepts).Any())
                    activeQuestions.Add(q);
            }

             var anws = db.Answers;
            List<Answer> activeAns = new List<Answer>();
            foreach (var a in anws)
            {
                if (activeQuestions.Contains(a.Question))
                    activeAns.Add(a);
            }

            //Base on probability set for setup it is selected if user is going to evaluate or answer
            int p = s.AnsweringProbability;
            if (rand.Next(p) != p - 1)
            {
                //user is going to validate answer

                //Algorithm for selection of answer to be evaluated looks like this:
                //0. filter answer according to activness of concepts assigned to the related questions
                //1. Find all answers, which have not been already evaluated by current user, which question concepts are within the active concepts
                //   which were not written by current user, which are in current setup and for active question
                //2. If answer was skipped remove it from list (if it is not the only one possible answer to be displayed) 
                //3. With greedy maximization principle, try to find questions which have fewer than 3 evaluations (MinEvaluationLimit)
                //  and which question was not seen by current user in last day, if such a answer exists ruturn it
                //4. If no such a question exists try to find question which have fewer then 16 (FullEvaluationLimit) evaluations with
                // the same proccedure as before. 
                //5.If there is no answer sleected in step 4, ignore 1 day rule and try to find
                // answer as before, but without the one day rule.
                //6. If there are also no results, try to find question to be answered

                //step 0 select only from active concepts


                //step 1

                var answers = db.Answers
                                    .Where(a => !a.Evaluations.Where(e => e.UserId == userId).Any()
                                        // && activeQuestions.Where(q => q.QuestionId == a.Question.QuestionId).Any()
                                        && a.Question.SubjectId == s.SubjectId
                                        && (a.UserId == null || a.UserId != userId)).ToList();
                                       // && a.Question.IsActive).ToList();*/
                answers = answers.Intersect(activeAns).ToList();


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
            var tempQuestions = activeQuestions.Where(q => !q.Answers.Where(r => r.UserId == userId).Any() &&
               // q.IsActive &&
                q.SubjectId == s.SubjectId).OrderBy(q => q.Answers.Count()).ToList();

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

        /// <summary>
        /// Finds and update/creates new record of viewed question by user
        /// </summary>
        /// <param name="questionId"></param>
        private void UserSeenQuestion(int questionId, int userId)
        {
            QuestionView qv = db.QuestionViews.Find(userId, questionId);
            if (qv == null)
            {
                qv = new QuestionView();
                qv.QuestionId = questionId;
                qv.UserId = userId;
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
        private void UserMadeAction(UserActionType action, int answerId, int questionId, int userId = 0)
        {
            if (userId == 0)
                userId = WebSecurity.CurrentUserId;

            UsersAction ua = new UsersAction();
            ua.UserId = userId;

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

        public ActionResult SkipAnswer(int questionId, int setupId)
        {
            UserMadeAction(UserActionType.SkippedAnswering, 0, questionId);
            RemoveHandledObjectFromSession(false);
            Session["SkippedQuestionId"] = questionId;  
            return RedirectToAction("AnswerAndEvaluate", new { setupId = setupId });
        }

        [HttpGet]
        public void ExternSkipAnswer(int userId, int questionId)
        {
            UserMadeAction(UserActionType.SkippedAnswering, 0, questionId, userId);
        }

        public ActionResult SkipEvaluation(int answerId, int setupId)
        {
            UserMadeAction(UserActionType.SkippedEvaluation, answerId, 0);
            RemoveHandledObjectFromSession(true);
            Session["SkippedAnswerId"] = answerId; 
            return RedirectToAction("AnswerAndEvaluate", new { setupId = setupId});
        }

        [HttpGet]
        public void ExternSkipEvaluation(int userId, int answerId)
        {
            UserMadeAction(UserActionType.SkippedEvaluation, answerId, 0, userId);
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
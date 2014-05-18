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
using CQA.Models;
using WebMatrix.WebData;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Extensions;
using CQA.Jobs;
//using Excel = Microsoft.Office.Interop.Excel; 

namespace CQA.Controllers
{

    [Authorize(Roles = "Admin")] 
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
            Setup s = new Setup();
            s.AnsweringProbability = 10;
            s.Active = true;
            return View(s);
        }

        //
        // POST: /Setups/Create

        [HttpPost]
        public ActionResult Create(Setup setup)
        {
            if (ModelState.IsValid)
            {
                setup.StartDate = DateTime.Now;
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
            Setup original = db.Setups.Find(setup.SetupId);
            if (ModelState.IsValid)
            {
                int originValue = original.AnsweringProbability;

                setup.DateCreated = original.DateCreated;
                db.Entry(original).CurrentValues.SetValues(setup);
                db.SaveChanges();

                if (setup.AnsweringProbability != originValue)
                    SetupsProbabilityChange.CreateSetupsProbabilityChange(setup.SetupId,setup.AnsweringProbability);

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


        public ActionResult ConvertOldNotifications()
        {
            foreach (Answer ans in db.Answers.Where(a => a.SeenEvaluation == false).ToList())
            {
                Notification not = new Notification();
                
                not.AnswerId = ans.AnswerId;
                not.NotificationFor = NotificationFor.MyAnswer;
                not.User = ans.Author;
                not.NotificationType = NotificationType.NewEvaluation;
                db.Notifications.Add(not);
                db.SaveChanges();
            }

            return RedirectToAction("Index","Home");
        }

        public ActionResult GetBasicStatistics()
        {
            using (StreamWriter sw = new StreamWriter(Server.MapPath("~/Reports/") + "stats.csv", false))
            {
                sw.WriteLine("pocet hodnoteni ;" + db.Ratings.Count());
                sw.WriteLine("pocet odpovedi ;" + db.Answers.Count());
                sw.WriteLine("dostatocne ohodnotenych ;" + db.Answers.Where(a => a.Evaluations.Count() == 16).Count());

                List<UserProfile> users = db.UserProfiles.OrderBy(u => u.Evaluations.Count()).ToList();
                foreach (UserProfile u in users)
                {
                    sw.WriteLine(u.RealName + ";" + u.Evaluations.Count() + ";" + u.UsersActions.Where(a => a.Action == UserActionType.SkippedAnswering || a.Action == UserActionType.SkippedEvaluation).Count() );
                }
                
            }
            ViewBag.Link = Request.Url.GetLeftPart(UriPartial.Authority)+"/Reports/stats.csv";
            return View();
        }

        [HttpPost]
        public ActionResult AddAnswers(HttpPostedFileBase file, int setupId)
        {
            var reader = new StreamReader(file.InputStream, Encoding.UTF8);
            List<string> columnNames = new List<string>();

            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            Setup s = db.Setups.Find(setupId);

            var firstLine = reader.ReadLine();

            while (!reader.EndOfStream)
            {
                var Line = reader.ReadLine();
                var values = Line.Split(';');

                int id = Convert.ToInt32(values[0]);
                var q = db.Questions.Where(que => que.SubjectId == s.SubjectId && que.QuestionFileId == id).ToList().First();
               

                Answer a = new Answer();
                a.Text = values[1];
                a.Question = q;
                a.SetupId = setupId;
                db.Answers.Add(a);
                db.SaveChanges();

            }

            return RedirectToAction("Details", new { id = setupId });
        }

        [HttpPost]
        public ActionResult SetStartDate(int setupId, string date)
        {
            var l = date.Split('/');
            Setup setup = db.Setups.Find(setupId);
            setup.StartDate = new DateTime(Convert.ToInt32(l[2]), Convert.ToInt32(l[0]),Convert.ToInt32(l[1]));
            db.Entry(setup).State = EntityState.Modified;
            db.SaveChanges();

            return new HttpStatusCodeResult((int)HttpStatusCode.OK);
        }

        [HttpGet]
        public ActionResult GetReport(int setupId)
        {

           Setup s = db.Setups.SingleOrDefault(set => set.SetupId == setupId);
           Int32 unixTimestamp = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

           MemoryStream stream = Report.GetReport(setupId);

           //Write to response stream
           Response.Clear();
           Response.AddHeader("content-disposition", String.Format("attachment;filename={0}", "report_" + s.Subject.Name + "_" + unixTimestamp.ToString()+".xlsx"));
           Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

           stream.WriteTo(Response.OutputStream);
           Response.End();

            //Excel.Application xlApp;
            //I  xlWorkSheet;
            //object misValue = System.Reflection.Missing.Value;

            //xlApp = new Excel.Application();
            //xlWorkBook = xlApp.Workbooks.Add(misValue);

            //xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            //xlWorkSheet.Name = "QALO aktivita (po týždňoch)";

            //Setup s = db.Setups.SingleOrDefault(set => set.SetupId == setupId);
            //DateTime start = s.StartDate;

            //int k = 2;
            ////active users
            //while (start.AddDays(7) < DateTime.Now)
            //{
            //    start = start.AddDays(7);
            //    xlWorkSheet.Cells[1, k] = "k " + start.ToString();
            //    k++;
            //}

            //k = 2;
            //start = s.StartDate;
            ////active users
            //while (start.AddDays(7) < DateTime.Now)
            //{
            //    start = start.AddDays(7);
            //    xlWorkSheet.Cells[2, 1] = "Počet zapojených študentov";
            //    xlWorkSheet.Cells[2, k] = db.UsersSetups.Where(us => us.SetupId == setupId && us.DateCreated <= start).ToList().Count.ToString();
            //    k++;
            //}

            //start = s.StartDate;
            //k = 2;
            ////evaluations : users
            //while (start.AddDays(7) < DateTime.Now)
            //{
            //    start = start.AddDays(7);
            //    int sCount = db.UsersSetups.Where(us => us.SetupId == setupId && us.DateCreated <= start).ToList().Count;
            //    int eCount = db.Ratings.Where(e => e.Answer.SetupId == setupId && e.DateCreated <= start).ToList().Count;

            //    xlWorkSheet.Cells[3, 1] = "Priemer hodnotení na študenta";
            //    if (sCount != 0)
            //        xlWorkSheet.Cells[3, k] = ((double)((double)eCount / sCount)).ToString("#.###");
            //    else
            //        xlWorkSheet.Cells[3, k] = "0";
            //    k++;
            //}

            //start = s.StartDate;
            //k = 2;
            ////evaluations : answers
            //while (start.AddDays(7) < DateTime.Now)
            //{
            //    xlWorkSheet.Cells[4, 1] = "Priemer hodnotení na odpoveď";
            //    start = start.AddDays(7);
            //    int aCount = db.Answers.Where(a => a.SetupId == setupId).ToList().Count();
            //    int eCount = db.Ratings.Where(e => e.Answer.SetupId == setupId && e.DateCreated <= start).ToList().Count;

            //    if (aCount != 0)
            //        xlWorkSheet.Cells[4, k] = ((double)((double)eCount / aCount)).ToString("#.###");
            //    else
            //        xlWorkSheet.Cells[4, k] = "0";
            //    k++;
            //}

            //start = s.StartDate;
            //k = 2;
            ////answers : users
            //while (start.AddDays(7) < DateTime.Now)
            //{
            //    xlWorkSheet.Cells[5, 1] = "Priemer odpovedí na študenta";
            //    start = start.AddDays(7);
            //    int aCount = db.UsersSetups.Where(a => a.SetupId == setupId && a.DateCreated <= start && a.UserId != null).ToList().Count;
            //    int sCount = db.UsersSetups.Where(us => us.SetupId == setupId && us.DateCreated <= start).ToList().Count;

            //    if (sCount != 0)
            //        xlWorkSheet.Cells[5, k] = ((double)((double)aCount / sCount)).ToString("#.###");
            //    else
            //        xlWorkSheet.Cells[5, k] = "0";
            //    k++;
            //}

            //start = s.StartDate;
            //k = 2;
            ////maxHodnoteni
            //while (start.AddDays(7) < DateTime.Now)
            //{
            //    xlWorkSheet.Cells[6, 1] = "Max. počet hodnotení odpovede";
            //    start = start.AddDays(7);
            //    if (db.Answers.Where(a => a.SetupId == setupId && a.DateCreated <= start).ToList().Count > 0)
            //    {
            //        Answer ans = db.Answers.Where(a => a.SetupId == setupId && a.DateCreated <= start).OrderByDescending(a => a.Evaluations.Count()).ToList().First();
            //        xlWorkSheet.Cells[6, k] = ans.Evaluations.Count;
            //    }
            //    else
            //        xlWorkSheet.Cells[6, k] = 0;
            //    k++;
            //}

            //start = s.StartDate;
            //k = 2;
            ////pocet hodnoteni
            //while (start.AddDays(7) < DateTime.Now)
            //{
            //    start = start.AddDays(7);
            //    xlWorkSheet.Cells[7, 1] = "Celkový počet hodnotení";
            //    xlWorkSheet.Cells[7, k] = db.Ratings.Where(e => e.Answer.SetupId == setupId && e.DateCreated <= start).ToList().Count();
            //    k++;
            //}

            //start = s.StartDate;
            //k = 2;
            ////pocet odpovedi
            //while (start.AddDays(7) < DateTime.Now)
            //{
            //    start = start.AddDays(7);
            //    xlWorkSheet.Cells[8, 1] = "Celkový počet štud. odpovedí";
            //    xlWorkSheet.Cells[8, k] = db.Answers.Where(a => a.SetupId == setupId && a.DateCreated <= start && a.UserId != null).ToList().Count();
            //    k++;
            //}

            //start = s.StartDate;
            //k = 2;
            ////pocet hodnotenych odpovedi
            //while (start.AddDays(7) < DateTime.Now)
            //{
            //    start = start.AddDays(7);
            //    xlWorkSheet.Cells[9, 1] = "Počet hodnotených odpovedí";
            //    var evals = db.Ratings.Where(e => e.Answer.SetupId == setupId && e.DateCreated <= start).ToList();

            //    List<Answer> ans = new List<Answer>();

            //    foreach (var e in evals)
            //    {
            //        if (!ans.Contains(e.Answer))
            //            ans.Add(e.Answer);
            //    }

            //    xlWorkSheet.Cells[9, k] = ans.Count();
            //    k++;
            //}

            //start = s.StartDate;
            //k = 2;
            ////pocet odpovedi
            //while (start.AddDays(7) < DateTime.Now)
            //{
            //    start = start.AddDays(7);
            //    xlWorkSheet.Cells[10, 1] = "Počet odpovedí s aspoň 15 hodnoteniami";
            //    xlWorkSheet.Cells[10, k] = db.Answers.Where(a => a.SetupId == setupId && a.DateCreated <= start && a.Evaluations.Count >= 15).ToList().Count();
            //    k++;
            //}
            //var aRange = xlWorkSheet.get_Range("A1", "XX100");
            //aRange.EntireColumn.AutoFit();
            //aRange.Rows.AutoFit();
            ////xlWorkSheet.get_Range("B2", "XX100").NumberFormat = "@";

            //xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.Add(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            //xlWorkSheet.Name = "QALO štatistiky (po týždňoch)";

            //int i = 2;
            //int j = 3;

            //start = s.StartDate;
            //k = 3;
            //while (start.AddDays(7) < DateTime.Now)
            //{
            //    xlWorkSheet.Range[xlWorkSheet.Cells[1, k], xlWorkSheet.Cells[1, k + 1]].Merge();
            //    start = start.AddDays(7);
            //    xlWorkSheet.Cells[1, k] = "k " + start.ToString();
            //    xlWorkSheet.Cells[2, k] = "# Hodnotení";
            //    xlWorkSheet.Cells[2, k + 1] = "# Odpovedí";
            //    k += 2;
            //}

            //xlWorkSheet.Cells[2, 1] = "Login";
            //xlWorkSheet.Cells[2, 2] = "Meno";

            //j = 3;
            //i++;
            //foreach (UsersSetup us in s.UsersSetups.OrderBy(us => us.User.RealName))
            //{
            //    xlWorkSheet.Cells[i, 1] = us.User.UserName;
            //    xlWorkSheet.Cells[i, 2] = us.User.RealName;
            //    start = s.StartDate;

            //    while (start.AddDays(7) < DateTime.Now)
            //    {
            //        start = start.AddDays(7);
            //        xlWorkSheet.Cells[i, j] = us.User.Evaluations.Where(e => e.DateCreated <= start && e.Answer.SetupId == setupId).ToList().Count.ToString();
            //        xlWorkSheet.Cells[i, j + 1] = us.User.Answers.Where(a => a.DateCreated <= start && a.SetupId == setupId).ToList().Count.ToString();
            //        j += 2;
            //    }
            //    j = 3;
            //    i++;
            //}
            //aRange = xlWorkSheet.get_Range("A1", "XX100");
            //aRange.EntireColumn.AutoFit();
            //aRange.Rows.AutoFit();
            //Int32 unixTimestamp = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            //string fileUrl = Server.MapPath("~/Reports/") + "report_" + s.Subject.Name + "_" + unixTimestamp.ToString() + ".xls";

            //xlWorkBook.SaveAs(fileUrl, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            //xlWorkBook.Close(true, misValue, misValue);
            //xlApp.Quit();

            //releaseObject(xlWorkSheet);
            //releaseObject(xlWorkBook);
            //releaseObject(xlApp);

            
            return View();

        }

        [HttpGet]
        public ActionResult ShowStudents(int setupId){
        
            ViewBag.SetupId = setupId;
            ViewBag.SetupName = db.Setups.Find(setupId).Subject.Shortcut+ " " + db.Setups.Find(setupId).Name;
            List<UserProfile> students = db.UserProfiles.Where(u => u.UsersSetups.Where(us => us.SetupId == setupId && us.UserId == u.UserId).Any()).ToList();
            students = students.OrderByDescending(s => s.Evaluations.Count() + s.Answers.Count()).ToList();
            return View(students);
        }

        [HttpGet]
        public ActionResult ShowStudentsDetail(int userId, int setupId)
        {
            ViewBag.UserName = db.UserProfiles.Find(userId).RealName;
            ViewBag.SubjectName = db.Setups.Find(setupId).Subject.Shortcut+ " " + db.Setups.Find(setupId).Name;
            var evaluated = db.Answers.Where( a => a.Evaluations.Where(e => e.UserId == userId).Any()).ToList();
            var answered = db.Answers.Where(a => a.UserId == userId).ToList();
            ViewBag.EvalsCount = evaluated.Count();
            ViewBag.AnswersCount = answered.Count();
            ViewBag.UserId = userId;
            var answers = db.Answers.Where( a => a.Evaluations.Where(e => e.UserId == userId).Any() || a.UserId == userId).ToList();
            return View(answers);
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
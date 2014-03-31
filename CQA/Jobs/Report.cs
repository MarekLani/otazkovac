using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CQA.Models;
using DocumentFormat.OpenXml.Extensions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CQA.Jobs
{
    public class Report 
    {
        public static MemoryStream GetReport(int setupId)
        {

            //TODO solve format

            MemoryStream stream = SpreadsheetReader.Create();
           SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, true);

           SpreadsheetWriter.RemoveWorksheet(doc, "Sheet1");
           SpreadsheetWriter.RemoveWorksheet(doc, "Sheet2");
           SpreadsheetWriter.RemoveWorksheet(doc, "Sheet3");

           WorksheetPart worksheetPart = SpreadsheetWriter.InsertWorksheet(doc, "QALO aktivita (po týždňoch)");
           WorksheetWriter writer = new WorksheetWriter(doc, worksheetPart);

           
           //writer.PasteText("B2", "Hello World");
           //writer.MergeCells(GetCellReference(3, 1), GetCellReference(2, 1));
           CQADBContext db = new CQADBContext();
           Setup s = db.Setups.SingleOrDefault(set => set.SetupId == setupId);
           DateTime start = s.StartDate;

           int k = 2;
           writer.FindColumn(1).BestFit = true;
           //active users
           while (start.AddDays(7) < DateTime.Now)
           {
               start = start.AddDays(7);
               writer.PasteText( GetCellReference(k,1), "k " + start.ToString());
               writer.FindColumn((uint)k).BestFit = true;
               k++;
           }



           k = 2;
           start = s.StartDate;
           //active users
           writer.PasteText(GetCellReference(1, 2), "Počet zapojených študentov");
           while (start.AddDays(7) < DateTime.Now)
           {
               start = start.AddDays(7);
               writer.PasteText(GetCellReference(k, 2), db.UsersSetups.Where(us => us.SetupId == setupId && us.DateCreated <= start).ToList().Count.ToString());
               k++;
           }

           start = s.StartDate;
           k = 2;
           //evaluations : users
           writer.PasteText(GetCellReference(1, 3),"Priemer hodnotení na študenta");
           while (start.AddDays(7) < DateTime.Now)
           {
               start = start.AddDays(7);
               int sCount = db.UsersSetups.Where(us => us.SetupId == setupId && us.DateCreated <= start).ToList().Count;
               int eCount = db.Ratings.Where(e => e.Answer.SetupId == setupId && e.DateCreated <= start).ToList().Count;

               
               if (sCount != 0)
                   writer.PasteText(GetCellReference(k, 3),((double)((double)eCount / sCount)).ToString("#.###"));
               else
                   writer.PasteText(GetCellReference(k, 3),"0");
               k++;
           }

           start = s.StartDate;
           k = 2;
           //evaluations : answers
           writer.PasteText(GetCellReference(1, 4),"Priemer hodnotení na odpoveď");
           while (start.AddDays(7) < DateTime.Now)
           {
              
               start = start.AddDays(7);
               int aCount = db.Answers.Where(a => a.SetupId == setupId).ToList().Count();
               int eCount = db.Ratings.Where(e => e.Answer.SetupId == setupId && e.DateCreated <= start).ToList().Count;

               if (aCount != 0)
                   writer.PasteText(GetCellReference(k,4),((double)((double)eCount / aCount)).ToString("#.###"));
               else
                   writer.PasteText(GetCellReference(k,4), "0");
               k++;
           }

           start = s.StartDate;
           k = 2;
           //answers : users
           writer.PasteText(GetCellReference(1, 5), "Priemer odpovedí na študenta");
           while (start.AddDays(7) < DateTime.Now)
           {
               start = start.AddDays(7);
               int aCount = db.UsersSetups.Where(a => a.SetupId == setupId && a.DateCreated <= start && a.UserId != null).ToList().Count;
               int sCount = db.UsersSetups.Where(us => us.SetupId == setupId && us.DateCreated <= start).ToList().Count;

               if (sCount != 0)
               {
                   writer.PasteText(GetCellReference(k, 5), ((double)((double)aCount / sCount)).ToString("#.###"));
                   
               }
               else
                   writer.PasteText(GetCellReference(k, 5), "0");
               k++;
           }

           start = s.StartDate;
           k = 2;
           //maxHodnoteni
           writer.PasteText(GetCellReference(1, 6),"Max. počet hodnotení odpovede");
           while (start.AddDays(7) < DateTime.Now)
           {
               start = start.AddDays(7);
               if (db.Answers.Where(a => a.SetupId == setupId && a.DateCreated <= start).ToList().Count > 0)
               {
                   Answer ans = db.Answers.Where(a => a.SetupId == setupId && a.DateCreated <= start).OrderByDescending(a => a.Evaluations.Count()).ToList().First();
                   writer.PasteText(GetCellReference(k,6), ans.Evaluations.Count.ToString());
               }
               else
                   writer.PasteText(GetCellReference(k,6), "0");
               k++;
           }

           start = s.StartDate;
           k = 2;
           writer.PasteText(GetCellReference(1, 7),"Celkový počet hodnotení");
           //pocet hodnoteni
           while (start.AddDays(7) < DateTime.Now)
           {
               start = start.AddDays(7);
               writer.PasteText(GetCellReference(k,7), db.Ratings.Where(e => e.Answer.SetupId == setupId && e.DateCreated <= start).ToList().Count().ToString());
               k++;
           }

           start = s.StartDate;
           k = 2;
           //pocet odpovedi
           writer.PasteText(GetCellReference(1,8),"Celkový počet štud. odpovedí");
           while (start.AddDays(7) < DateTime.Now)
           {
               start = start.AddDays(7);
               writer.PasteText(GetCellReference(k,8),db.Answers.Where(a => a.SetupId == setupId && a.DateCreated <= start && a.UserId != null).ToList().Count().ToString());
               k++;
           }

           start = s.StartDate;
           k = 2;
           //pocet hodnotenych odpovedi
           writer.PasteText(GetCellReference(1,9), "Počet hodnotených odpovedí");
           while (start.AddDays(7) < DateTime.Now)
           {
               start = start.AddDays(7);
               var evals = db.Ratings.Where(e => e.Answer.SetupId == setupId && e.DateCreated <= start).ToList();

               List<Answer> ans = new List<Answer>();

               foreach (var e in evals)
               {
                   if (!ans.Contains(e.Answer))
                       ans.Add(e.Answer);
               }

               writer.PasteText(GetCellReference(k,9), ans.Count().ToString());
               k++;
           }

           start = s.StartDate;
           k = 2;
           writer.PasteText(GetCellReference(1,10),"Počet odpovedí s aspoň 15 hodnoteniami");
           //pocet odpovedi
           while (start.AddDays(7) < DateTime.Now)
           {
               start = start.AddDays(7);
               writer.PasteText(GetCellReference(k, 10), db.Answers.Where(a => a.SetupId == setupId && a.DateCreated <= start && a.Evaluations.Count >= 15).ToList().Count().ToString()) ;
               k++;
           }

           //var aRange = xlWorkSheet.get_Range("A1", "XX100");
           //aRange.EntireColumn.AutoFit();
           //aRange.Rows.AutoFit();
           ////xlWorkSheet.get_Range("B2", "XX100").NumberFormat = "@";

           //xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.Add(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
           //xlWorkSheet.Name = "QALO štatistiky (po týždňoch)";
           worksheetPart = SpreadsheetWriter.InsertWorksheet(doc, "QALO štatistiky (po týždňoch)");
           writer = new WorksheetWriter(doc, worksheetPart);
 
           int i = 2;
           int j = 3;

           start = s.StartDate;
           k = 3;
           while (start.AddDays(7) < DateTime.Now)
           {
               writer.MergeCells(GetCellReference(k, 1),GetCellReference(k+7, 1));
               start = start.AddDays(7);
               writer.PasteText(GetCellReference(k,1), "k " + start.ToString());
               writer.PasteText(GetCellReference(k,2),"# Hodnotení");
               writer.PasteText(GetCellReference(k + 1, 2), "% Hodnotení");
               writer.PasteText(GetCellReference(k + 2, 2), "# Preskočení hodnotenia");
               writer.PasteText(GetCellReference(k + 3, 2), "% Preskočení hodnotenia");
               writer.PasteText(GetCellReference(k + 4, 2), "# Odpovedí");
               writer.PasteText(GetCellReference(k + 5, 2), "% Odpovedí");
               writer.PasteText(GetCellReference(k + 6, 2), "# Preskočení odpovedania");
               writer.PasteText(GetCellReference(k + 7, 2), "% Preskočení odpovedania");
               k += 8;
           }

           writer.PasteText(GetCellReference(1,2),"Login");
           writer.PasteText(GetCellReference(2, 2), "Meno");

           j = 3;
           i++;
           foreach (UsersSetup us in s.UsersSetups.OrderBy(us => us.User.RealName))
           {
               writer.PasteText(GetCellReference(1,i), us.User.UserName);
               writer.PasteText(GetCellReference(2,i),us.User.RealName);
               start = s.StartDate;

               while (start.AddDays(7) < DateTime.Now)
               {
                   start = start.AddDays(7);
                   //TODO solve setupID add to action table
                   int evalsCount = us.User.Evaluations.Where(e => e.DateCreated <= start && e.Answer.SetupId == setupId).ToList().Count;
                   int evalsSkipCount = us.User.UsersActions.Where(ua => ua.DateCreated <= start && ua.Action == UserActionType.SkippedEvaluation).ToList().Count;
                   int evalsActionsTotal = evalsCount + evalsSkipCount;
                   writer.PasteText(GetCellReference(j,i), evalsCount.ToString());
                   //percent
                   if(evalsCount == 0)
                        writer.PasteText(GetCellReference(j+1, i), "0");
                   else
                       writer.PasteText(GetCellReference(j+1, i), ((double)evalsCount/(double)evalsActionsTotal).ToString("#.###"));

                   writer.PasteText(GetCellReference(j+2, i), evalsSkipCount.ToString());
                   //percent
                   if (evalsSkipCount == 0)
                       writer.PasteText(GetCellReference(j + 3, i), "0");
                   else
                       writer.PasteText(GetCellReference(j + 3, i), ((double)evalsSkipCount / (double)evalsActionsTotal).ToString("#.###"));

                   int ansCount = us.User.Answers.Where(a => a.DateCreated <= start && a.SetupId == setupId).ToList().Count;
                   int ansSkipCount = us.User.UsersActions.Where(ua => ua.DateCreated <= start && ua.Action == UserActionType.SkippedAnswering).ToList().Count;
                   int ansActionsTotal = ansCount + ansSkipCount;
                   writer.PasteText(GetCellReference(j+4,i), ansCount.ToString());
                   //percent
                   if (ansCount == 0)
                       writer.PasteText(GetCellReference(j + 5, i), "0");
                   else
                       writer.PasteText(GetCellReference(j + 5, i), ((double)ansCount / (double)ansActionsTotal).ToString("#.###"));

                   writer.PasteText(GetCellReference(j + 6, i), ansSkipCount.ToString());
                   //percent
                   if (ansSkipCount == 0)
                       writer.PasteText(GetCellReference(j + 7, i), "0");
                   else
                       writer.PasteText(GetCellReference(j + 7, i), ((double)ansSkipCount / (double)ansActionsTotal).ToString("#.###"));

                   j += 8;
               }
               j = 3;
               i++;
           }
           //aRange = xlWorkSheet.get_Range("A1", "XX100");
           //aRange.EntireColumn.AutoFit();
           //aRange.Rows.AutoFit(); 

           //Save to the memory stream
           SpreadsheetWriter.Save(doc);

           return stream;
        }


        private static string GetCellReference(int x, int y)
        {
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            StringBuilder sb = new StringBuilder();

            if ((x-1) / 26 == 0 )
                return alpha[x - 1].ToString() + y.ToString();
            else{
                return alpha[(x / 26) - 1].ToString() + alpha[(x % 26)].ToString() + y.ToString();
            }


        }
    }
}

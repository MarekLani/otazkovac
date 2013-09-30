using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using CQA.Models;

namespace CQA.Controllers
{

    public class HomeController : Controller
    {
         private CQADBContext db = new CQADBContext();
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            //ParseGyms.ParseAllGyms();
            //ParseCities parser = new ParseCities();
           // parser.Parse();
            //ParseGyms.CreateExcel();
            string t = "aké";

            //SqlCommand com = new SqlCommand("SELECT * FROM Articles WHERE Contains(Description,' \"@term\" ')");
           // com.Parameters.AddWithValue("@term",t ); //Replace 1 with messageid you want to get
           // string s = com.ExecuteScalar().ToString();

            SqlConnection SqlConnection;
            SqlCommand SqlCommand;
            SqlDataReader SqlDataReader;

            SqlConnection = new SqlConnection();
            SqlConnection.ConnectionString = ConfigurationManager.ConnectionStrings["CQADBContext"].ConnectionString;
            string cmd = String.Format(@"SELECT 100 * coalesce(ct1.RANK, 0) "+
                "as WeightedRank, * " +
                "FROM Articles " +
                "LEFT JOIN " +
                "CONTAINSTABLE(Articles, Description, ' \"ake*\" OR \"dsasd*\" ') "
                +"ct1 ON ct1.[Key] = Articles.ArticleId "
                +"WHERE RANK > 0");
            SqlCommand = new SqlCommand(cmd, SqlConnection);
           // SqlCommand.CommandType = CommandType.Text;

            //SqlCommand.Connection.Open();
            //SqlDataReader = SqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
            //SqlDataReader.Read();
            //string s = SqlDataReader.GetValue(0).ToString();

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}

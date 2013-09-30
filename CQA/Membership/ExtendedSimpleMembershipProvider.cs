using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using CQA.Models;
using WebMatrix.WebData;

namespace CQA.Membership
{
    //Extending membership this is how it is done but we do not need it

    //public class CustomMembershipProvider : SimpleMembershipProvider
    //{
    //    private FitManagerDBContext db = new FitManagerDBContext();
    //    public override bool ValidateUser(string login, string password)
    //    {
    //        // check to see if the login passed is an email address
    //        if (IsValidEmail(login))
    //        {
    //            string actualUsername = db.UserProfiles.FirstOrDefault(u => u.Email == login).UserName;
    //            return base.ValidateUser(actualUsername, password);
    //        }
    //        else
    //        {
    //            return base.ValidateUser(login, password);
    //        }

    //    }

    //    bool IsValidEmail(string strIn)
    //    {
    //        // Return true if strIn is in valid e-mail format.
    //        return Regex.IsMatch(strIn, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
    //    }

    //}
}
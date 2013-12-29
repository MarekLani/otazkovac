using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using CQA.Models;
using WebMatrix.WebData;

namespace CQA.Membership
{

    public class CustomMembershipProvider : SimpleMembershipProvider
    {
        private static CQADBContext db = new CQADBContext();
        public static UserProfile GetUser(int id)
        {
            return db.UserProfiles.Single(u => u.UserId == id);

        }


    }
}
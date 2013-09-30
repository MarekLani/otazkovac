using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CQA
{
    using System.Data.Entity;
    using System.Web.Security;
    using CQA.Membership;
    using CQA.Models;
    using CQA.Models;
    using WebMatrix.WebData;

    namespace SeedSimple
    {
        public class InitSecurityDb : DropCreateDatabaseIfModelChanges<CQADBContext>
        {
            protected override void Seed(CQADBContext context)
            {

                WebSecurity.InitializeDatabaseConnection("DefaultConnection",
                   "UserProfile", "UserId", "UserName", autoCreateTables: true);
                var roles = (SimpleRoleProvider)Roles.Provider;

                if (!roles.RoleExists("Admin"))
                {
                    roles.CreateRole("Admin");
                }
                //if (WebSecurity.UserExists("test") == null)
                //{
                //    WebSecurity.CreateAccount("test", "test");
                //}
                //if (!roles.GetRolesForUser("test").Contains("Admin"))
                //{
                //    roles.AddUsersToRoles(new[] { "test" }, new[] { "admin" });
                //}

            }
        }
    }
}
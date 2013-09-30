using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using CQA.Models;
using WebMatrix.WebData;
using System.Data.Entity.Infrastructure;

namespace CQA
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");
            OAuthWebSecurity.RegisterClient(new CQA.App_Start.FacebookScopedClient("345626672235621", "9785de07193c0585fd849a7dbd2f4e12"), "Facebook", null);
            /*OAuthWebSecurity.RegisterFacebookClient(
                appId: "345626672235621",
                appSecret: "9785de07193c0585fd849a7dbd2f4e12");*/

            //OAuthWebSecurity.RegisterGoogleClient();

            
        }
    }
}

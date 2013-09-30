﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using DotNetOpenAuth.AspNet;
using Newtonsoft.Json;

namespace CQA.App_Start
{
    public class FacebookScopedClient : IAuthenticationClient
    {
        private string appId ;
        private string appSecret;

        private const string baseUrl = "https://www.facebook.com/dialog/oauth?client_id=";
        public const string graphApiToken = "https://graph.facebook.com/oauth/access_token?";
        public const string graphApiMe = "https://graph.facebook.com/me?";


        private static string GetHTML(string URL)
        {
            string connectionString = URL;

            try
            {
                System.Net.HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(connectionString);
                myRequest.Credentials = CredentialCache.DefaultCredentials;
                //// Get the response
                WebResponse webResponse = myRequest.GetResponse();
                Stream respStream = webResponse.GetResponseStream();
                ////
                StreamReader ioStream = new StreamReader(respStream);
                string pageContent = ioStream.ReadToEnd();
                //// Close streams
                ioStream.Close();
                respStream.Close();
                return pageContent;
            }
            catch (Exception)
            {
            }
            return null;
        }

        private IDictionary<string, string> GetUserData(string accessCode, string redirectURI)
        {

            string token = GetHTML(graphApiToken + "client_id=" + appId + "&redirect_uri=" + HttpUtility.UrlEncode(redirectURI) + "&client_secret=" + appSecret + "&code=" + accessCode);
            if (token == null || token == "")
            {
                return null;
            }
            string data = GetHTML(graphApiMe + "fields=id,name,email,username,gender,link&access_token=" + token.Substring("access_token=", "&"));

            // this dictionary must contains
            Dictionary<string, string> userData = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            return userData;
        }

        public FacebookScopedClient(string appId, string appSecret)
        {
            this.appId = appId;
            this.appSecret = appSecret;
        }

        public string ProviderName
        {
            get { return "Facebook"; }
        }

        public void RequestAuthentication(System.Web.HttpContextBase context, Uri returnUrl)
        {
            string url = baseUrl + appId + "&redirect_uri=" + HttpUtility.UrlEncode(returnUrl.ToString()) + "&scope=email";
            context.Response.Redirect(url);
        }

        public AuthenticationResult VerifyAuthentication(System.Web.HttpContextBase context)
        {
            string code = context.Request.QueryString["code"];

            string rawUrl = context.Request.Url.OriginalString;
            //From this we need to remove code portion
            rawUrl = Regex.Replace(rawUrl, "&code=[^&]*", "");

            IDictionary<string, string> userData = GetUserData(code, rawUrl);

            if (userData == null)
                return new AuthenticationResult(false, ProviderName, null, null, null);

            string id = userData["id"];
            string username = userData["name"];
            userData.Remove("id");
            userData.Remove("name");

            AuthenticationResult result = new AuthenticationResult(true, ProviderName, id, username, userData);
            return result;
        }
    }

    public static class String
    {
        public static string Substring(this string str, string StartString, string EndString)
        {
            if (str.Contains(StartString))
            {
                int iStart = str.IndexOf(StartString) + StartString.Length;
                int iEnd = str.IndexOf(EndString, iStart);
                return str.Substring(iStart, (iEnd - iStart));
            }
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using CQA.Filters;
using CQA.Models;
using System.Dynamic;
using Postal;
using CQA.Membership;
using System.DirectoryServices;

namespace CQA.Controllers
{
    
    //[InitializeSimpleMembership]
    public class AccountController : Controller
    {
        private CQADBContext db = new CQADBContext();
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {

            if (ModelState.IsValid)
            {
                string surName = " ";
                string firstName = "";
                int uisId = 0;
                //comment it out for testing
                //Attempt to register the user
                try
                {
                    string sDomain = "LDAP://ldap.stuba.sk:389/uid=xlanim,ou=People,dc=stuba,dc=sk";
                    string sServiceUser = "uid="+model.UserName+",ou=People,dc=stuba,dc=sk";
                    string sServicePassword = model.Password;
                    DirectoryEntry de = new DirectoryEntry(sDomain, sServiceUser, sServicePassword, AuthenticationTypes.None);
                    DirectorySearcher ds = new DirectorySearcher(de);
                    ds.Filter = "(&" +
                                 "(uid=" + model.UserName +")" +
                                  ")";
                    
 
                    SearchResult sr = ds.FindOne();


                    foreach (string PropertyName in sr.Properties.PropertyNames)
                    {
                        try
                        {
                            foreach (Object key in sr.GetDirectoryEntry().Properties[PropertyName])
                            {
                                switch (PropertyName.ToLower())
                                {
                                    case "sn":
                                        surName += key.ToString();
                                        break;
                                    case "givenname":
                                        firstName += key.ToString();
                                        break;
                                    case "uisid":
                                        uisId = Convert.ToInt32(key.ToString());
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        catch { }
                    }
                   
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Nesprávne meno alebo heslo");
                    return View(model);
                }
                // Using Hack with password, because we really do not need it

                //If user was not logged before
                if (!db.UserProfiles.Where(u => u.UserName == model.UserName).Any())
                    WebSecurity.CreateUserAndAccount(model.UserName, "pass", new { RealName = firstName + surName });

                WebSecurity.Login(model.UserName, "pass", persistCookie: model.RememberMe);
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();
            Session.Abandon();
           // Session.Remove("facebooktoken");
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/RegisterUser
        [AllowAnonymous]
        public ActionResult RegisterUser()
        {
            return View();
        }

        //
        // POST: /Account/RegisterUser
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterUser(RegisterUserModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                //try
                //{
                //    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                //    WebSecurity.Login(model.UserName, model.Password);
                //    return RedirectToAction("Index", "Home");
                //}
                //catch (MembershipCreateUserException e)
                //{
                //    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                //}

                // Attempt to register the user
                try
                {
                    string urlName = Helpers.StringHelper.CreateSeoFriendlyString(model.RealName) + "-" + Guid.NewGuid().ToString();
                    string confirmationToken =
                        WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new { RealName = model.RealName, UrlName = urlName },true);
                    /*dynamic email = new Email("RegEmail");
                    email.To = model.UserName;
                    email.UserName = model.RealName;
                    email.ConfirmationToken = confirmationToken;
                    email.Send();*/

                    return RedirectToAction("Index", "Questions");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    if (e.Message.Contains("Violation of UNIQUE KEY constraint 'U_email'"))
                        ModelState.AddModelError("", "Zadaná emailová adresa je už registrovaná");
                }

            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

      



        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}

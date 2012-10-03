/*
 * Author       : Alex Ninneman
 * Created      : January 10, 2011
 * Description  : This controller will be used for authenticating the user, allowing them to sign in, register an account, recover an account and logout.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Web.Security;
using CurtAdmin.Models;

namespace CurtAdmin.Controllers
{
    public class AuthenticateController : Controller
    {
        /// <summary>
        /// Display the login screen.
        /// </summary>
        /// <returns>Login screen.</returns>
        public ActionResult Index()
        {
            Session.Clear();
            return View();
        }

        /// <summary>
        /// Takes in the information from the login screen and tries to log the user in.
        /// </summary>
        /// <param name="username">User's username</param>
        /// <param name="password">User's password</param>
        /// <returns>Successful: Redirects to user home or admin home ::: Error: Returns to login screen with error message.</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(string username, string password, string rememberMe = "", string redirectUrl = "") {
            // Make sure the user entered a password.
            if (password.Trim().Length == 0) {
                ViewBag.Message = "You must enter a password.";
                return View();
            }

            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            user login_user = new user();
            login_user = (from u in doc_db.users
                               where u.username.Equals(username)
                               select u).FirstOrDefault();
            if (login_user == null) {
                ViewBag.Message = "Username was not found in our database";
                return View();
            }

            if (password.Trim() != login_user.password) {
                ViewBag.Message = "Username/password was incorrect.";
                return View();
            } else {
                // User login successful: assign Session data and redirect.
                Session["auth_level"]   = login_user.isAdmin + "";
                Session["userID"]       = login_user.userID;
                Session["username"]     = login_user.username;
                Session["superUser"]    = login_user.superUser;
                Session["name"]         = login_user.fname + " " + login_user.lname;

                if (rememberMe == "1") {
                    HttpCookie authLevel = new HttpCookie("auth_level");
                    authLevel.Value = login_user.isAdmin + "";
                    authLevel.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Add(authLevel);

                    HttpCookie userID = new HttpCookie("userID");
                    userID.Value = login_user.userID + "";
                    userID.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Add(userID);

                    HttpCookie cookie_username = new HttpCookie("username");
                    cookie_username.Value = login_user.username;
                    cookie_username.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Add(cookie_username);

                    HttpCookie superUser = new HttpCookie("superUser");
                    superUser.Value = login_user.superUser + "";
                    superUser.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Add(superUser);

                    HttpCookie name = new HttpCookie("name");
                    name.Value = login_user.fname + " " + login_user.lname;
                    name.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Add(name);
                }

                if (login_user.isAdmin == 1 && redirectUrl == "") { // Redirect to admin section
                    HttpContext.Response.Redirect("~/Admin");
                } else if(login_user.isAdmin == 0 && redirectUrl == "") { // Redirect to user home
                    HttpContext.Response.Redirect("~/Home");
                } else if (login_user.isAdmin == 1 || login_user.isAdmin == 0 && redirectUrl != "") {
                    HttpContext.Response.Redirect(redirectUrl);
                }
            }
            ViewBag.Message = "There was error while logging you in, my bad!";
            return View();
        }

        /// <summary>
        /// Sign up for a new account.
        /// </summary>
        /// <returns>Sign up page.</returns>
        public ActionResult Signup() {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            List<State> states = (from s in doc_db.States
                                  orderby s.abbr
                                  select s).ToList<State>();
            ViewBag.states = states;
            ViewBag.stateID = 0;
            ViewBag.submitted = 0;

            return View();
        }

        /// <summary>
        /// Handles the submission of the sign up form.
        /// </summary>
        /// <param name="fname"></param>
        /// <returns>If the sign up was successful, we e-mail the admin letting them know a new account has been created. Otherwise display errors.</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Signup(string fname) {

            // Assign form fields
            fname = Request.Form["fname"].Trim();
            string lname = Request.Form["lname"].Trim();
            string new_username = Request.Form["new_username"].Trim();
            string email = Request.Form["email"].Trim();
            string address = Request.Form["address"].Trim();
            string phone = Request.Form["phone"].Trim().Replace("-", "");
            string city = Request.Form["city"].Trim();
            int stateID = Convert.ToInt32(Request.Form["stateID"].Trim());
            int isDealer = (Request.Form["dealer"] != null)?1:0;
            string comments = Request.Form["comments"];

            // Initiate error list
            List<string> error_messages = new List<string>();

            /******* Validate form fields ******/
            if (fname.Length == 0) { error_messages.Add("First name is required."); }
            if (lname.Length == 0) { error_messages.Add("Last name is required."); }
            if (new_username.Length < 6) { error_messages.Add("Username must be at least 6 characters."); }
            if (email.Length == 0) { error_messages.Add("E-Mail is required."); }
            if (phone.Length == 0) { error_messages.Add("Phone number is required."); }
            if (address.Length == 0) { error_messages.Add("Address is required."); }
            if (city.Length == 0) { error_messages.Add("City is required."); }
            if (stateID == 0) { error_messages.Add("State is required."); }
            if (comments.Length == 0) { error_messages.Add("Comments are required."); }

            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            // Make sure we don't have a user for this e-mail address
            List<user> u = (from users in doc_db.users
                              where users.email.Equals(email)
                              select users).ToList<user>();
            if (u.Count != 0) { error_messages.Add("A user with this e-mail already exists in the database."); }

            // Make sure we don't have a user with this username
            int username_count = (from uc in doc_db.users
                                  where uc.username.Equals(new_username)
                                  select uc).Count();
            if (username_count > 0) { error_messages.Add("Username is taken."); }


            if(error_messages.Count == 0){ // Store user information and send e-mail to rep
                PasswordGenerator pg = new PasswordGenerator();
                string password = pg.Generate();

                user newUser = new user {
                    username = new_username,
                    password = password,
                    email = email,
                    fname = fname,
                    lname = lname,
                    phone = phone,
                    comments = comments,
                    stateID = stateID,
                    city = city,
                    address = address,
                    dateAdded = DateTime.Now,
                    isDealer = isDealer
                };

                doc_db.users.InsertOnSubmit(newUser);
                try{
                    doc_db.SubmitChanges();
                    Users.AlertRep(newUser);
                    ViewBag.submitted = 1;
                }catch(Exception e){
                    error_messages.Add(e.Message);
                    ViewBag.error_messages = error_messages;

                    // Get the states
                    List<State> states = (from s in doc_db.States
                                          orderby s.abbr
                                          select s).ToList<State>();
                    ViewBag.states = states;
                }

            }else{ // Present error messages to user
                ViewBag.error_messages = error_messages;
                ViewBag.fname = fname;
                ViewBag.lname = lname;
                ViewBag.new_username = new_username;
                ViewBag.email = email;
                ViewBag.address = address;
                ViewBag.phone = phone;
                ViewBag.city = city;
                ViewBag.stateID = stateID;
                ViewBag.comments = comments;
                ViewBag.isDealer = isDealer;

                // Get the states
                List<State> states = (from s in doc_db.States
                                      orderby s.abbr
                                      select s).ToList<State>();
                ViewBag.states = states;

            }

            return View();
        }

        /// <summary>
        /// This method is called if the user has forgotten there username or password. It will ask them to enter either their username or e-mail address and we will regenerate a password and e-mail it to them.
        /// </summary>
        /// <returns>Form to allow the user to recover their password.</returns>
        public ActionResult Forgot() {

            return View();
        }

        /// <summary>
        /// Handles the forgot password form.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="email">E-Mail</param>
        /// <returns>If successful, send an e-mail with the user authentication info. Otherwise display error message.</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Forgot(string username, string email) {
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            if (username.Trim().Length != 0) {

                // Instantiate the user object and assign user
                user u = new user();
                u = (from users in doc_db.users
                          where users.username.Equals(username.Trim())
                          select users).FirstOrDefault();
                if (u != null) { // Make sure we found a user
                    if (AuthenticateUser.sendNewPass(u)) { // Attempt to send updated e-mail
                        HttpContext.Response.Redirect("~/Authenticate/userFound");
                    } else {
                        ViewBag.Message = "We were unable to locate " + username.Trim() + " in our system";
                    }
                    
                } else {
                    ViewBag.Message = "We were unable to locate " + username.Trim() + " in our system.";
                }
            } else if (email.Trim().Length != 0) {

                // Instantiate our user object and populate from database
                user u = new user();
                u = (from users in doc_db.users
                     where users.email.Equals(email.Trim())
                     select users).FirstOrDefault();

                if (u != null) { // Make sure we found a user
                    if (AuthenticateUser.sendNewPass(u)) { // Attempt to send update e-mail
                        HttpContext.Response.Redirect("~/Authenticate/userFound");
                    } else {
                        ViewBag.Message = "We were unable to locate " + email.Trim() + " in our system.";
                    }
                } else {
                    ViewBag.Message = "We were unable to locate " + email.Trim() + " in our system.";
                }
            } else { // Both username and email were blank
                ViewBag.Message = "You did not enter a username or e-mail address";
            }                

            return View("Forgot");
        }

        /// <summary>
        /// Log the user out.
        /// </summary>
        public void Logout() {
            string[] cookies = Request.Cookies.AllKeys;
            foreach (string cookie in cookies) {
                Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1);
            }
            Session.Clear();
            HttpContext.Response.Redirect("~/Authenticate");
        }

        /// <summary>
        /// This is the page that user sees after they have filled out the forgot password form.
        /// </summary>
        /// <returns>View displaying message.</returns>
        public ActionResult userFound() {
            return View();
        }
    }
}

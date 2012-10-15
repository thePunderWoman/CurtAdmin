using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CurtAdmin.Models;

namespace CurtAdmin.Controllers
{
    public class UsersController : BaseController
    {

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
 	        base.OnActionExecuted(filterContext);
            ViewBag.activeModule = "Users";
        }


        /// <summary>
        /// List all the users in the database. Always easy access to basic CRUD tools.
        /// </summary>
        /// <returns>View</returns>
        public ActionResult Index() {

            // Get the modules for this user
            List<module> modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.modules = modules;

            // Get a list of all the users in the database
            List<user> users = Users.GetAllUsers();
            ViewBag.users = users;

            return View();
        }

        /// <summary>
        /// List all the users in the database. Always easy access to basic CRUD tools.
        /// </summary>
        /// <returns>View</returns>
        public ActionResult Customers() {

            // Get a list of all the users in the database
            List<CustomerUser> users = CustomerUser.GetAll();
            ViewBag.users = users;

            return View();
        }

        /// <summary>
        /// Loads the needed elements for the default Add User page.
        /// </summary>
        /// <returns>View</returns>
        public ActionResult Add() {

            // Get the modules for the logged in user
            List<module> modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.modules = modules;

            // Get all the admin modules
            List<module> allmodules = Users.GetAllModules();
            ViewBag.allmodules = allmodules;

            return View();
        }


        /// <summary>
        /// Handles the submit for the Add page. Adds a new user record to the database along with any associated modules. If save is successful and user is set to active, an e-mail is distributed to the entered users's email.
        /// </summary>
        /// <param name="username">Takes in the username field from the form.</param>
        /// <returns>View/Redirect</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Add(string username) {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            username = Request.Form["username"].Trim();
            string password1 = Request.Form["password1"].Trim();
            string password2 = Request.Form["password2"].Trim();
            string email = Request.Form["email"].Trim();
            string fname = Request.Form["fname"].Trim();
            string lname = Request.Form["lname"].Trim();
            string website = Request.Form["website"].Trim();
            string phone = Request.Form["phone"].Trim();
            string fax = Request.Form["fax"].Trim();
            string comments = Request.Form["comments"].Trim();
            string biography = Request.Form["biography"].Trim();
            string photo = Request.Form["photo"].Trim();

            // Determine if the user is marked as active
            int isActive = 0;
            if (Request.Form["isActive"] != null) {
                isActive = Convert.ToInt32(Request.Form["isActive"].Trim());
            }

            // Are we going to assign super user privilages?
            int superUser = 0;
            if (Request.Form["superUser"] != null) {
                superUser = Convert.ToInt32(Request.Form["superUser"].Trim());
            }
            
            // Compile the selected modules
            List<string> selected_modules = new List<string>();
            if (Request.Form["module"] != null) {
                selected_modules = Request.Form["module"].Split(',').ToList<string>();
            }
            
            // Initialize our error list
            List<string> error_messages = new List<string>();

            // Make sure passwords match
            if (password1 != password2) { error_messages.Add("Passwords do not match."); }

            // Make sure password is longer than 8 characters
            if (password1.Length < 8) { error_messages.Add("Password must be at least 8 characters."); }

            // Make sure username is longer than 8 characters
            if (username.Length < 8) { error_messages.Add(" Username must be at least 8 characters."); }

            // Make sure they entered an e-mail
            if (email.Trim().Length < 5) { error_messages.Add("You must enter a valid e-mail address"); }

            // We need to check the database to make sure we don't have a user with this username or e-mail
            int username_count = (from u in doc_db.users
                             where u.username.Equals(username)
                             select u).Count();
            int email_count = (from u in doc_db.users
                               where u.email.Equals(email)
                               select u).Count();
            if (username_count > 0) { error_messages.Add("The username already exists."); }
            if (email_count > 0) { error_messages.Add("The e-mail is already in use by another user."); }

            #region
            if (error_messages.Count == 0) { // No errors found

                // Create new user
                user newUser = new user {
                    username = username,
                    password = password1,
                    email = email,
                    fname = fname,
                    lname = lname,
                    website = website,
                    phone = phone.Replace("-", ""),
                    fax = fax.Replace("-", ""),
                    isActive = isActive,
                    comments = comments,
                    dateAdded = DateTime.Now,
                    superUser = superUser,
                    biography = biography,
                    photo = photo
                };
                doc_db.users.InsertOnSubmit(newUser);

                // Submit new user to database
                try {

                    doc_db.SubmitChanges(); // Commit new user

                    // Now we have to insert the modules for this user

                    int userId = newUser.userID; // Get ther userID for the new user

                    // Step through the select modules and insert to database
                    foreach (string moduleId in selected_modules) {
                        user_module newUser_module = new user_module {
                            userID = userId,
                            moduleID = Convert.ToInt32(moduleId)
                        };
                        doc_db.user_modules.InsertOnSubmit(newUser_module);
                    }

                    // Attempt to commit user modules
                    try {
                        doc_db.SubmitChanges();

                        if (isActive == 1) { // Send user e-mail message letting them know their account is active
                            Users.AlertNewUser(userId);
                        }
                        return RedirectToAction("Index");
                    } catch (Exception e) {
                        error_messages.Add(e.Message);
                        
                    }
                    ViewBag.selected_modules = selected_modules;
                    ViewBag.error_messages = error_messages;
                } catch (Exception e) { // Something went wrong while saving
                    
                    error_messages.Add(e.Message);
                    ViewBag.selected_modules = selected_modules;
                    ViewBag.error_messages = error_messages;

                }

            } else { // There were errors so we will store all entered data into the ViewBag so the user doesn't have to retype.

                ViewBag.username = username;
                ViewBag.email = email;
                ViewBag.fname = fname;
                ViewBag.lname = lname;
                ViewBag.website = website;
                ViewBag.phone = phone.Replace("-", "");
                ViewBag.fax = fax.Replace("-", "");
                ViewBag.isActive = isActive;
                ViewBag.superUser = superUser;
                ViewBag.comments = comments;
                ViewBag.biography = biography;
                ViewBag.photo = photo;
                ViewBag.selected_modules = selected_modules;
                ViewBag.error_messages = error_messages;
            }
            #endregion

            // Get the modules for the logged in user
            List<module> modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.modules = modules;

            // Get all the admin modules
            List<module> allmodules = Users.GetAllModules();
            ViewBag.allmodules = allmodules;

            List<user> users = Users.GetAllUsers();
            ViewBag.users = users;

            return View();
        }

        /// <summary>
        /// Edit a user's information.
        /// </summary>
        /// <param name="user_id">ID of the user.</param>
        /// <returns>View containing fields to allow editing of user info.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Edit(string user_id) {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            int userID = Convert.ToInt32(user_id);

            // Get the user's information
            user u = (from users in doc_db.users
                      where users.userID.Equals(userID)
                      select users).FirstOrDefault<user>();
            ViewBag.u = u;

            // Get the modules for the user we are editing
            List<module> selected_modules = Users.GetUserModules(u.userID);
            ViewBag.selected_modules = selected_modules;

            // Get the modules for the logged in user
            List<module> modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.modules = modules;

            // Get all the modules
            List<module> allmodules = Users.GetAllModules();
            ViewBag.allmodules = allmodules;

            return View();
        }

        /// <summary>
        /// Handles form submission of the user edit page.
        /// </summary>
        /// <param name="user_id">ID of the user</param>
        /// <param name="username">Username for the user.</param>
        /// <returns>Redirect on success::::Redisplays edit page with errors, if we encounter an issue.</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(string user_id, string username) {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            // Convert the user_id into an integer and make sure it's valid
            int userID = Convert.ToInt32(user_id);
            if (userID == 0) {
                return RedirectToAction("Index");
            }

            // Declare a List object to hold error messages
            List<string> error_messages = new List<string>();

            username = Request.Form["username"].Trim();
            string password1 = Request.Form["password1"].Trim();
            string password2 = Request.Form["password2"].Trim();
            string email = Request.Form["email"].Trim();
            string fname = Request.Form["fname"].Trim();
            string lname = Request.Form["lname"].Trim();
            string website = Request.Form["website"].Trim();
            string phone = Request.Form["phone"].Trim();
            string fax = Request.Form["fax"].Trim();
            string comments = Request.Form["comments"].Trim();
            string biography = Request.Form["biography"].Trim();
            string photo = Request.Form["photo"].Trim();
            
            int isActive = 0;
            if (Request.Form["isActive"] != null) {
                isActive = Convert.ToInt32(Request.Form["isActive"].Trim());
            }

            int superUser = 0;
            if (Request.Form["superUser"] != null) {
                superUser = Convert.ToInt32(Request.Form["superUser"]);
            }

            // Make sure passwords match
            if (password1 != password2) { error_messages.Add("Passwords do not match."); }

            // Make sure password is longer than 8 characters
            if (password1.Length < 4) { error_messages.Add("Password is not long enough. Shoot for 8 characters."); }

            // Make sure username is longer than 8 characters
            if (username.Length < 4) { error_messages.Add(" Username is not long enough. Shoot for 8 characters."); }

            // Make sure they entered an e-mail
            if (email.Trim().Length < 5) { error_messages.Add("You must enter a valid e-mail address"); }

            // Get the user's information
            user u = (from users in doc_db.users
                      where users.userID.Equals(user_id)
                      select users).FirstOrDefault<user>();

            int old_isActive = u.isActive;
            int new_isActive = isActive;

            if (error_messages.Count == 0) {
                u.username = username;
                u.password = password1;
                u.email = email;
                u.fname = fname;
                u.lname = lname;
                u.website = website;
                u.phone = phone.Replace("-", "");
                u.fax = fax.Replace("-", "");
                u.isActive = isActive;
                u.comments = comments;
                u.superUser = superUser;
                u.biography = biography;
                u.photo = photo;
                
                // Attempt to save the user
                try {

                    doc_db.SubmitChanges();

                    // Delete all modules for this user
                    List<user_module> users_modules = (from um in doc_db.user_modules
                                                       where um.userID.Equals(userID)
                                                       select um).ToList<user_module>();
                    // Step through the user's modules and remove them all ::: we'll add the ones that are selected in a minute
                    foreach (user_module module in users_modules) {
                        doc_db.user_modules.DeleteOnSubmit(module);
                    }

                    // Compile List of the modules selected for this user
                    List<string> user_selected_modules = new List<string>();
                    if (Request.Form["module"] != null) {
                        user_selected_modules = Request.Form["module"].Split(',').ToList<string>();
                    }

                    // Step through the select modules and insert to database
                    foreach (string moduleId in user_selected_modules) {
                        user_module newUser_module = new user_module {
                            userID = u.userID,
                            moduleID = Convert.ToInt32(moduleId)
                        };
                        doc_db.user_modules.InsertOnSubmit(newUser_module);
                    }

                    // Attempt to commit user modules
                    try {
                        doc_db.SubmitChanges();

                        if (new_isActive == 1 && old_isActive == 0) { // Send user e-mail message letting them know their account is active
                            Users.AlertNewUser(userID);
                        }

                        return RedirectToAction("Index");
                    } catch (Exception e) { // Failed to submit user modules
                        error_messages.Add(e.Message);
                    }
                    ViewBag.error_messages = error_messages;

                } catch (Exception e) { // Failed to save user info

                    error_messages.Add(e.Message);
                }
            }
            ViewBag.error_messages = error_messages;

            // Store the user object in the ViewBag
            ViewBag.u = u;

            // Get the modules for the user we are editing
            List<module> selected_modules = Users.GetUserModules(u.userID);
            ViewBag.selected_modules = selected_modules;

            // Get the modules for the logged in user
            List<module> modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.modules = modules;

            // Get all the modules
            List<module> allmodules = Users.GetAllModules();
            ViewBag.allmodules = allmodules;

            return View();
        }

        /****** AJAX Functions ***********/

        /// <summary>
        /// Get the user information and return it as JSON object.
        /// </summary>
        /// <param name="user_id">ID of the user.</param>
        /// <returns>JSON containing user information.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string GetUser(string user_id) {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            int userID = Convert.ToInt32(user_id);
            user u = Users.GetUser(userID);
            return serializer.Serialize(u);
            
        }

        /// <summary>
        /// Removes the given user from the database.
        /// </summary>
        /// <param name="userID">Primary Key to identify the user.</param>
        /// <returns>Blank string on success::::Error message if an issue is encountered.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string RemoveUser(string userID) {
            // Convert the userID into an integer
            int userId = Convert.ToInt32(userID);

            // Attempt to remove user
            if (!Users.DeleteUser(userId)) {
                return Users.error_message;
            }
            return "";
        }

        /// <summary>
        /// Removes the given Customer user from the database.
        /// </summary>
        /// <param name="userID">Primary Key to identify the user.</param>
        /// <returns>Blank string on success::::Error message if an issue is encountered.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string RemoveCustomerUser(Guid userID) {
            CustomerUser u = new CustomerUser().Get(userID);
            u.Delete();
            return "";
        }

        /// <summary>
        /// Update the entered user's record to be either active or inactive.
        /// </summary>
        /// <param name="userID">Primary Key of user.</param>
        /// <returns>String representing the success/errors encountered.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string SetUserStatus(string userID) {

            // Convert the userID and status into integers
            int userId = Convert.ToInt32(userID);

            // Set the isActive field for the user
            if(!Users.Set_isActive(userId)){
                
                return Users.error_message;
            }
            return "";
        }

        /// <summary>
        /// Update the entered user's record to be either active or inactive.
        /// </summary>
        /// <param name="userID">Primary Key of user.</param>
        /// <returns>String representing the success/errors encountered.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string SetCustomerUserStatus(Guid userID) {
            CustomerUser u = new CustomerUser().Get(userID);
            u.activate();
            return "";
        }

    }
}

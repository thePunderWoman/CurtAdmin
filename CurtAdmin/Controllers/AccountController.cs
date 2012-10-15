using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;

namespace CurtAdmin.Controllers
{
    public class AccountController : UserBaseController
    {
        //
        // GET: /Users/

        public ActionResult Index() {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            int userID = Convert.ToInt32(Session["userID"]);

            user u = new user();
            u = (from users in doc_db.users
                 where users.userID.Equals(userID)
                 select users).FirstOrDefault<user>();
            ViewBag.u = u;

            // Get the states
            List<State> states = new List<State>();
            states = (from s in doc_db.States
                     orderby s.abbr
                     select s).ToList<State>();
            ViewBag.states = states;

            // Get the user's available modules
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(userID);
            ViewBag.Modules = modules;

            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(string username) {

            DocsLinqDataContext doc_db  = new DocsLinqDataContext();
            int userID                  = Convert.ToInt32(Session["userID"]);
            string password1            = Request.Form["password1"].Trim();
            string password2            = Request.Form["password2"].Trim();
            string email                = Request.Form["email"].Trim();
            string fname                = Request.Form["fname"].Trim();
            string lname                = Request.Form["lname"].Trim();
            string website              = Request.Form["website"].Trim();
            string phone                = Request.Form["phone"].Trim().Replace("-","");
            string fax                  = Request.Form["fax"].Trim().Replace("-", "");
            string address              = Request.Form["address"].Trim();
            string city                 = Request.Form["city"].Trim();
            string biography            = Request.Form["biography"].Trim();
            string photo                = Request.Form["photo"].Trim();
            int stateID                 = Convert.ToInt32(Request.Form["stateID"].Trim());

            // Initiate error messages
            List<string> error_messages = new List<string>();

            if (password1.Length == 0) { error_messages.Add("Password must be at least 8 characters."); }
            if (password1 != password2) { error_messages.Add("Passwords must match."); }
            if (email.Length < 5) { error_messages.Add("E-mail is required."); }

            // Get the users information
            user u = new user();
            u = (from users in doc_db.users
                 where users.userID.Equals(userID)
                 select users).FirstOrDefault<user>();

            if (error_messages.Count == 0) { // Save the user's information

                u.password  = password1;
                u.email     = email;
                u.fname     = fname;
                u.lname     = lname;
                u.website   = website;
                u.phone     = phone;
                u.fax       = fax;
                u.address   = address;
                u.city      = city;
                u.stateID   = stateID;
                u.biography = biography;
                u.photo     = photo;

                try { // Attempt to update the users information
                    doc_db.SubmitChanges();
                } catch (Exception e) {
                    error_messages.Add(e.Message);
                }
            }

            // Get the states
            List<State> states = new List<State>();
            states = (from s in doc_db.States
                      orderby s.abbr
                      select s).ToList<State>();
            ViewBag.states = states;

            // Get the user's available modules
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(userID);
            ViewBag.Modules = modules;
            
            ViewBag.u = u;
            ViewBag.error_messages = error_messages;

            return View();
        }

    }
}

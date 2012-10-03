/*
 * Author       : Alex Ninneman
 * Created      : January 20, 2011
 * Description  : This controller holds all of the page/AJAX functions for dealing with documentation categories
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;

namespace CurtAdmin.Controllers
{
    public class Admin_CategoryController : AdminBaseController
    {

        protected override void OnActionExecuted(ActionExecutedContext filterContext) {
            base.OnActionExecuted(filterContext);
            ViewBag.activeModule = "Doc Categories";
        }


        /// <summary>
        /// Load all of the categories in the database so the user can make actions on them.
        /// </summary>
        /// <returns>View of categories</returns>
        public ActionResult Index() {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            // Get all categories
            List<category> categories = (from c in doc_db.categories
                                                 orderby c.catName
                                                 select c).ToList<category>();
            ViewBag.categories = categories;


            // Get the modules for the logged in user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;


            return View();
        }

        /// <summary>
        /// Loads the blank category page so that the user can add a brand new category. The user will be able to choose a category to mark as the parent category.
        /// </summary>
        /// <returns>View of new category.</returns>
        public ActionResult Add() {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            // Get all categories
            List<category> categories = (from c in doc_db.categories
                                         orderby c.catName
                                         select c).ToList<category>();
            ViewBag.categories = categories;

            // get the user modules ::: this will allow a category to be added to a module
            List<module> user_modules = new List<module>();
            user_modules = Users.GetAllModules("user");
            ViewBag.user_modules = user_modules;

            // Get the modules for the logged in user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;

            return View();
        }

        /// <summary>
        /// Handle the submission of a new category.
        /// </summary>
        /// <param name="catName"></param>
        /// <returns>Redirects to list of categories on completion. If an error is captured, the user is brought back the Add Category page to refine errors.</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Add(string catName) {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            catName = Request.Form["catName"].Trim();
            int parentID = Convert.ToInt32(Request.Form["parentID"].Trim());
            int moduleID = Convert.ToInt32(Request.Form["moduleID"].Trim());
            string comments = Request.Form["comments"].Trim();

            // Initiate our error message collection
            List<string> error_messages = new List<string>();

            // Make sure the category name is not blank.
            if (catName.Length == 0) { error_messages.Add("Cateogry name is required."); }
            

            if (error_messages.Count == 0) {
                // Create new category object
                category newCat = new category {
                    catName = catName,
                    parentID = parentID,
                    moduleID = moduleID,
                    comments = comments
                };
                doc_db.categories.InsertOnSubmit(newCat);

                try { // Attempt to save the category
                    doc_db.SubmitChanges();
                    HttpContext.Response.Redirect("~/Admin_Category");
                } catch (Exception e) {
                    error_messages.Add(e.Message);
                }
            }

            ViewBag.catName = catName;
            ViewBag.parentID = parentID;
            ViewBag.comments = comments;
            ViewBag.moduleID = moduleID;
            ViewBag.error_messages = error_messages;

            // Get all categories
            List<category> categories = (from c in doc_db.categories
                                         orderby c.catName
                                         select c).ToList<category>();
            ViewBag.categories = categories;

            // get the user modules ::: this will allow a category to be added to a module
            List<module> user_modules = new List<module>();
            user_modules = Users.GetAllModules("user");
            ViewBag.user_modules = user_modules;

            // Get the modules for the logged in user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;

            return View();
        }

        /// <summary>
        /// Edit information for a given category.
        /// </summary>
        /// <param name="cat_id"></param>
        /// <returns>View containing information for this category.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Edit(string cat_id) {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            int catID = Convert.ToInt32(cat_id);

            // Get all categories
            List<category> categories = (from cats in doc_db.categories
                                         orderby cats.catName
                                         select cats).ToList<category>();
            ViewBag.categories = categories;

            // Get the category to be updated
            category cat = new category();
            cat = (from c in doc_db.categories
                   where c.catID.Equals(catID)
                   select c).FirstOrDefault<category>();
            ViewBag.cat = cat;

            // get the user modules ::: this will allow a category to be added to a module
            List<module> user_modules = new List<module>();
            user_modules = Users.GetAllModules("user");
            ViewBag.user_modules = user_modules;

            // Get the modules for the logged in user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;

            return View();
        }

        /// <summary>
        /// Handles the submission of changes to a given category
        /// </summary>
        /// <param name="cat_id"></param>
        /// <param name="catName"></param>
        /// <returns>Redirects to list of categories on completion. If an error is captured, the user is brought back to the Edit Category page to refine errors.</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(string cat_id, string catName) {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            int catID = Convert.ToInt32(cat_id);
            catName = Request.Form["catName"].Trim();
            int parentID = Convert.ToInt32(Request.Form["parentID"].Trim());
            int moduleID = Convert.ToInt32(Request.Form["moduleID"].Trim());
            string comments = Request.Form["comments"];

            // Initiate error messages list
            List<string> error_messages = new List<string>();

            // validate category info
            if (catName.Length == 0) { error_messages.Add("Cateogry name is required."); }

            // Get the category to be updated
            category cat = new category();
            cat = (from c in doc_db.categories
                   where c.catID.Equals(catID)
                   select c).FirstOrDefault<category>();

            if (error_messages.Count == 0) { // Attempt to save category
                cat.catName = catName;
                cat.parentID = parentID;
                cat.moduleID = moduleID;
                cat.comments = comments;

                try {
                    doc_db.SubmitChanges();
                    HttpContext.Response.Redirect("~/Admin_Category");
                } catch (Exception e) {
                    error_messages.Add(e.Message);
                }
            }



            // Get all categories
            List<category> categories = (from cats in doc_db.categories
                                         orderby cats.catName
                                         select cats).ToList<category>();
            ViewBag.categories = categories;

            
            ViewBag.cat = cat;
            ViewBag.error_messages = error_messages;

            // get the user modules ::: this will allow a category to be added to a module
            List<module> user_modules = new List<module>();
            user_modules = Users.GetAllModules("user");
            ViewBag.user_modules = user_modules;

            // Get the modules for the logged in user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;

            return View();

        }

        /********* AJAX Functions ***********/

        /// <summary>
        /// Removes a category from the database and takes all items under this category and moves them to the unknown category.
        /// </summary>
        /// <param name="cat_id"></param>
        /// <returns>Blank string if successful. If error is captured - returns error message.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string RemoveCategory(string cat_id) {
            string error = "";

            try {
                // Convert cat_id into integer and instantiate LINQ object
                int catID = Convert.ToInt32(cat_id);
                DocsLinqDataContext doc_db = new DocsLinqDataContext();

                // Get the category
                category cat = (from c in doc_db.categories
                                where c.catID.Equals(catID)
                                select c).FirstOrDefault<category>();
                doc_db.categories.DeleteOnSubmit(cat);

                // Get the items under this category
                List<cat_item> items = (from ci in doc_db.cat_items
                                       where ci.catID.Equals(catID)
                                       select ci).ToList<cat_item>();

                foreach (cat_item item in items) { // Loop through the items and reset their category
                    item.catID = 1;
                }

                doc_db.SubmitChanges(); // Save changes
            } catch (Exception e) {
                error = e.Message;
            }

            return error;
        }
    }
}

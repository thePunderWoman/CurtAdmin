/*
 * Author       : Alex Ninneman
 * Created      : January 10, 2011
 * Description  : This controller works as the global controller for all admin functionality.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;

namespace CurtAdmin.Controllers
{
    public class AdminController : AdminBaseController
    {
        
        /// <summary>
        /// This is the homepage for all administrative users.
        /// </summary>
        /// <returns>View</returns>
        public ActionResult Index()
        {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            int userId = 0;
            if (Session["userID"] != null && Session["userID"].ToString().Length > 0) {
                userId = Convert.ToInt32(Session["userID"]);
            } else {
                HttpContext.Response.Redirect("~/Authenticate");
            }
            

            // Get the modules for this admin user
            List<module> modules = Users.GetUserModules(userId);
            
            ViewBag.Modules = modules;


            return View();
        }

    }
}

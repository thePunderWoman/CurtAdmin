using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Configuration;
using System.Xml.Linq;

namespace CurtAdmin.Controllers {
    public class HomeController : BaseController {

        public ActionResult Index() {

            ViewBag.Message = "Welcome "+ Session["username"];

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

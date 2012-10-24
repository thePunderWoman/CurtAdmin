using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;

namespace CurtAdmin.Controllers
{
    public class BaseController : Controller
    {
        protected override void Initialize(System.Web.Routing.RequestContext requestContext) {
            base.Initialize(requestContext);

            HttpCookie userID = new HttpCookie("");
            userID = Request.Cookies.Get("userID");

            HttpCookie username = new HttpCookie("");
            username = Request.Cookies.Get("username");

            HttpCookie superUser = new HttpCookie("");
            superUser = Request.Cookies.Get("superUser");

            HttpCookie name = new HttpCookie("");
            name = Request.Cookies.Get("name");

            try {
                string uID = null;
                try {
                    // cookie login
                    uID = userID.Value;
                } catch {
                    try {
                        uID = Session["userID"].ToString();
                    } catch { }
                }

                DocsLinqDataContext doc_db = new DocsLinqDataContext();
                user u = (from users in doc_db.users
                          where users.userID.Equals(Convert.ToInt32(uID))
                          select users).First<user>();

                Session["userID"] = u.userID;
                Session["username"] = u.username;
                Session["superUser"] = u.superUser;
                Session["name"] = u.fname + " " + u.lname;

                // Get the modules for the logged in user
                List<module> modules = new List<module>();
                modules = Users.GetUserModules(u.userID);
                ViewBag.name = u.fname + " " + u.lname;
                ViewBag.Modules = modules;
            } catch {
                // user doesn't exist
                Response.Redirect("~/Authenticate/Logout");
            }
        }

    }
}

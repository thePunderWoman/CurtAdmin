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

            Session["userID"] = userID.Value;
            Session["username"] = username.Value;
            Session["superUser"] = superUser.Value;
            Session["name"] = name.Value;

            // Get the modules for the logged in user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;
        }

    }
}

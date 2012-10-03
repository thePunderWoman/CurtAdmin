using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;

namespace CurtAdmin.Controllers
{
    public class AdminBaseController : Controller
    {
        protected override void Initialize(System.Web.Routing.RequestContext requestContext) {
            base.Initialize(requestContext);
            HttpCookie authLevel = new HttpCookie("");
            authLevel = Request.Cookies.Get("auth_level");

            HttpCookie userID = new HttpCookie("");
            userID = Request.Cookies.Get("userID");

            HttpCookie username = new HttpCookie("");
            username = Request.Cookies.Get("username");

            HttpCookie superUser = new HttpCookie("");
            superUser = Request.Cookies.Get("superUser");

            HttpCookie name = new HttpCookie("");
            name = Request.Cookies.Get("name");

            // Check the cookies to make sure the user is authenticated to the proper level
            if (authLevel == null || authLevel.Value == null || !AuthenticateUser.checkAuth(1, authLevel.Value)) {
                // Check the session to make sure the user is authenticated to the proper level
                if (Session["auth_level"] == null || !AuthenticateUser.checkAuth(1, Session["auth_level"].ToString())) {
                    HttpContext.Response.Redirect("~/Authenticate?redirectUrl="+HttpContext.Request.Url.PathAndQuery);
                }
            } else {
                Session["auth_level"] = authLevel.Value;
                Session["userID"] = userID.Value;
                Session["username"] = username.Value;
                Session["superUser"] = superUser.Value;
                Session["name"] = name.Value;
            }
            // Get the modules for the logged in user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;
        }

    }
}

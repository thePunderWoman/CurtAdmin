using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Web.Script.Serialization;

namespace CurtAdmin.Controllers
{
    public class Admin_ReviewsController : AdminBaseController
    {
        //
        // GET: /Admin_Review/

        public ActionResult Index(int id = 0)
        {
            List<ReviewDetail> reviews = ReviewModel.GetAllUnapproved();
            ViewBag.reviews = reviews;

            return View();
        }

        public string Get(int id = 0) {
            ReviewDetail review = ReviewModel.Get(id);
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(review);
        }

        public string Approve(int id = 0) {
            return ReviewModel.Approve(id);
        }

        public string Remove(int id = 0) {
            // Permanently Delete review
            if (!ReviewModel.Remove(id)) {
                return "error";
            }
            return "success";
        }
    }
}

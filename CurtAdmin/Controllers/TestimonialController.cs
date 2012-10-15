using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Web.Script.Serialization;

namespace CurtAdmin.Controllers
{
    public class TestimonialController : BaseController
    {
        //
        // GET: /Testimonial/

        public ActionResult Index()
        {
            List<Testimonial> testimonials = TestimonialModel.GetAllUnapproved();
            ViewBag.testimonials = testimonials;
            return View();
        }

        public ActionResult Approved() {
            List<Testimonial> testimonials = TestimonialModel.GetAllApproved();
            ViewBag.testimonials = testimonials;
            return View();
        }

        public string Get(int id = 0) {
            Testimonial testimonial = TestimonialModel.Get(id);
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(testimonial);
        }

        public string Approve(int id = 0) {
            return TestimonialModel.Approve(id);
        }

        public string Remove(int id = 0) {
            // Permanently Delete testimonial
            if (!TestimonialModel.Remove(id)) {
                return "error";
            }
            return "success";
        }
    }
}

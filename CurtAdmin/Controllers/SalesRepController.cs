using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;

namespace CurtAdmin.Controllers {
    public class SalesRepController : BaseController {
        protected override void OnActionExecuted(ActionExecutedContext filterContext) {
            base.OnActionExecuted(filterContext);
            ViewBag.activeModule = "ACES Vehicle Data";
        }


        public ActionResult Index() {

            // Get all the news
            List<SalesRepresentative> reps = new SalesRepresentative().GetAll();
            ViewBag.reps = reps;

            return View();
        }

        public ActionResult Edit(int id = 0) {
            SalesRepresentative rep = new SalesRepresentative().Get(id);
            ViewBag.rep = rep;
            return View();
        }

        public ActionResult Add() {
            return View("Edit");
        }

        public ActionResult Save(int id = 0, string name = "", string code = "") {

            // Get the news Item
            SalesRepresentative rep = new SalesRepresentative().Save(id, name, code);

            return RedirectToAction("Index");
        }

        public string Delete(int id = 0) {
            try {
                SalesRepresentative rep = new SalesRepresentative();
                rep.Delete(id);
                return "";
            } catch (Exception e) {
                return e.Message;
            }
        }

    }
}

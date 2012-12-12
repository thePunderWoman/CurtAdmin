using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace CurtAdmin.Controllers {
    public class LandingPagesController : BaseController {
        //
        // GET: /Website/
        protected override void OnActionExecuting(ActionExecutingContext filterContext) {
            ViewBag.activeModule = "Landing Page Management";
            List<Website> websites = new Website().GetAll();
            ViewBag.websites = websites;
            HttpCookie websiteCookie = new HttpCookie("");
            websiteCookie = Request.Cookies.Get("websiteID");
            int websiteID = 0;
            try {
                websiteID = Convert.ToInt32(websiteCookie.Value);
            } catch {}
            ViewBag.websiteID = websiteID;
        }

        public ActionResult Index() {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index","Website");
            }
            List<LandingPage> landingPages = new LandingPage().GetAll();
            ViewBag.landingPages = landingPages;

            return View();
        }

        public ActionResult Past() {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index", "Website");
            }
            List<LandingPage> landingPages = new LandingPage().GetAllPast();
            ViewBag.landingPages = landingPages;

            return View();
        }

        public ActionResult Add() {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index", "Website");
            }
            List<Website> websites = new Website().GetAll();
            ViewBag.websites = websites;
            
            return View();
        }

        public ActionResult Edit(int id = 0, string error = "") {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index", "Website");
            }
            List<Website> websites = new Website().GetAll();
            ViewBag.websites = websites;

            LandingPage landingPage = new LandingPage().Get(id);
            ViewBag.landingPage = landingPage;

            ViewBag.error = error;

            return View();
        }

        public string Remove(int id) {
            try {
                new LandingPage().Remove(id);
                return "{\"success\":true}";
            } catch {
                return "{\"success\":false}";
            }
        }

        [ValidateInput(false)]
        public ActionResult Save(int id = 0) {
            string name = Request.Form["name"] ?? null;
            int websiteID = (Request.Form["website"] != "") ? Convert.ToInt32(Request.Form["website"]) : 0;
            DateTime startDate = Convert.ToDateTime(Request.Form["startDate"]);
            DateTime endDate = Convert.ToDateTime(Request.Form["endDate"]);
            string url = Request.Form["url"] ?? null;
            string content = String.IsNullOrWhiteSpace(Request.Form["page_content"]) ? null : Request.Form["page_content"];
            string linkClasses = String.IsNullOrWhiteSpace(Request.Form["linkClasses"]) ? null : Request.Form["linkClasses"];
            string conversionID = String.IsNullOrWhiteSpace(Request.Form["conversionID"]) ? null : Request.Form["conversionID"];
            string conversionLabel = String.IsNullOrWhiteSpace(Request.Form["conversionLabel"]) ? null : Request.Form["conversionLabel"];
            bool newWindow = (Request.Form["newWindow"] == null) ? false : true;
            string menuPosition = Request.Form["menuPosition"];
            string error = "";

            LandingPage landingPage = new LandingPage();

            if (name == null || websiteID == 0 || url == null) {
                error = "Please fill out all required fields";
                if (id == 0) {
                    return RedirectToAction("Add");
                }
            } else {
                try {
                    landingPage = landingPage.Save(id, name, websiteID, startDate, endDate, url, content, linkClasses, newWindow, conversionID, conversionLabel, menuPosition);
                    id = landingPage.id;
                } catch (Exception e) {
                    error = e.Message + " " + e.StackTrace;
                }
            }

            return RedirectToAction("Edit", new { id = id, error = error });
        }

        public string AddImage(int pageID, string image) {
            List<LandingPageImage> images = new List<LandingPageImage>();
            images = new LandingPage().AddImage(pageID, image);
            return JsonConvert.SerializeObject(images);
        }

        public string RemoveImage(int id) {
            List<LandingPageImage> images = new List<LandingPageImage>();
            images = new LandingPage().RemoveImage(id);
            return JsonConvert.SerializeObject(images);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public void UpdateSort(int[] img) {
            new LandingPage().UpdateSort(img);
        }

        public string AddData(int pageID, string key, string value) {
            List<LandingPageData> datas = new List<LandingPageData>();
            datas = new LandingPage().AddData(pageID, key, value);
            return JsonConvert.SerializeObject(datas);
        }

        public string RemoveData(int id) {
            List<LandingPageData> datas = new List<LandingPageData>();
            datas = new LandingPage().RemoveData(id);
            return JsonConvert.SerializeObject(datas);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CurtAdmin.Controllers
{
    public class TechServicesController : Controller
    {
        //
        // GET: /TechServices/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult News(string successMsg = "") {

            if (successMsg != "") {
                ViewBag.successMsg = successMsg;
            }
            CurtDevDataContext db = new CurtDevDataContext();
            List<TechNews> news = db.TechNews.OrderBy(x => x.dateModified).ToList<TechNews>();
            ViewBag.news = news;
            return View();
        }

        public ActionResult EditNews(int id) {
            ViewBag.error = "";
            CurtDevDataContext db = new CurtDevDataContext();
            if (id > 0) {
                TechNews item = db.TechNews.Where(x => x.id == id).FirstOrDefault<TechNews>();
                ViewBag.item = item;
            } else {
                ViewBag.error = "Could not find the news item.";
            }

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditNews(int id, int displayOrder = 0, string title = "", string pageContent = "", string subTitle = "", string isActive = "", string showDealers = "", string showPublic = "") {
            ViewBag.error = "";
            if (id > 0) {
                CurtDevDataContext db = new CurtDevDataContext();
                TechNews item = db.TechNews.Where(x => x.id == id).FirstOrDefault<TechNews>();
                ViewBag.item = item;

                Boolean blnIsActive = false;
                blnIsActive = (isActive == "on") ? true : false;

                Boolean blnShowPublic= false;
                blnShowPublic = (showPublic == "on") ? true : false;

                Boolean blnShowDealers = false;
                blnShowDealers = (showDealers == "on") ? true : false;

                item.title = title;
                item.pageContent = pageContent;
                item.subTitle = subTitle;
                item.displayOrder = displayOrder;
                item.active = blnIsActive;
                item.showDealers = blnShowDealers;
                item.showPublic = blnShowPublic;
                item.dateModified = DateTime.Now;
                db.SubmitChanges();

                return RedirectToAction("News", new { successMsg = "The news item was successfully saved." });


            } else {
                ViewBag.error = "Could not find the news item.";
            }

            return View();
        }



        public ActionResult AddNews() {
            ViewBag.error = "";

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddNews(int displayOrder = 0, string title = "", string pageContent = "", string subTitle = "", string isActive = "", string showDealers = "", string showPublic = "") {
            try {
                TechNews item = new TechNews();
                Boolean blnIsActive = false;
                blnIsActive = (isActive == "on") ? true : false;

                Boolean blnShowPublic = false;
                blnShowPublic = (showPublic == "on") ? true : false;

                Boolean blnShowDealers = false;
                blnShowDealers = (showDealers == "on") ? true : false;

                item.title = title;
                item.pageContent = pageContent;
                item.subTitle = subTitle;
                item.displayOrder = displayOrder;
                item.active = blnIsActive;
                item.showDealers = blnShowDealers;
                item.showPublic = blnShowPublic;
                item.dateModified = DateTime.Now;
                ViewBag.item = item;
                CurtDevDataContext db = new CurtDevDataContext();
                db.TechNews.InsertOnSubmit(item);
                db.SubmitChanges();
                return RedirectToAction("News", new { successMsg = "The news item was successfully added." });
            } 

            catch (Exception e) {
                ViewBag.error = "Could not add news item: " + e.Message;
            }

            
            return View();
        }


        public string DeleteNewsItem (int id){

            CurtDevDataContext db = new CurtDevDataContext();
            TechNews item = db.TechNews.Where(x => x.id == id).FirstOrDefault<TechNews>();
            if (item != null) {
                db.TechNews.DeleteOnSubmit(item);
                db.SubmitChanges();
            } else {
                return "Could not find news item.";
            }

            return "";
        }
    }
}

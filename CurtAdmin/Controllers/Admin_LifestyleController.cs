using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Web.Script.Serialization;
using System.IO;
using System.Net;
using System.Text;

namespace CurtAdmin.Controllers {
    public class Admin_LifestyleController : AdminBaseController {

        public ActionResult Index() {
            List<DetailedCategories> cats = ProductCat.GetLifestyles();
            ViewBag.cats = cats;
            return View();
        }

        public ActionResult Add(int isLifestyle = 0) {

            // Get the categories so the use can make the new one a subcategory if they choose
            List<DetailedCategories> cats = ProductCat.GetCategories();
            ViewBag.cats = cats;
            ViewBag.isLifestyle = isLifestyle;

            return View();
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult Add(string catTitle = "", int parentID = 0, string image = "", int isLifestyle = 0, string shortDesc = "", string longDesc = "") {

            // Save the category
            List<string> error_messages = new List<string>();
            Categories cat = new Categories();
            try {
                cat = ProductCat.SaveCategory(0, catTitle, parentID, image, isLifestyle, shortDesc, longDesc);
            } catch (Exception e) {
                error_messages.Add(e.Message);
            }

            // Make sure we didn't catch any errors
            if (error_messages.Count == 0 && cat.catID > 0) {
                HttpContext.Response.Redirect("~/Admin_Lifestyle");
            } else {
                ViewBag.catTitle = catTitle;
                ViewBag.parentID = parentID;
                ViewBag.image = image;
                ViewBag.isLifestyle = isLifestyle;
                ViewBag.shortDesc = shortDesc;
                ViewBag.longDesc = longDesc;
                ViewBag.error_messages = error_messages;
            }

            // Get the categories so this category can make the new one a subcategory if they choose
            List<DetailedCategories> cats = ProductCat.GetCategories();
            ViewBag.cats = cats;

            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Edit(int catID = 0) {

            // Get this category
            DetailedCategories cat = ProductCat.GetCategory(catID);
            ViewBag.cat = cat;

            // Get the categories so the use can make the new one a subcategory if they choose
            List<DetailedCategories> cats = ProductCat.GetCategories();
            ViewBag.cats = cats;

            return View();
        }

        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        public ActionResult Edit(int catID, string catTitle = "", int parentID = 0, string image = "", int isLifestyle = 0, string shortDesc = "", string longDesc = "") {

            List<string> error_messages = new List<string>();
            DetailedCategories cat = new DetailedCategories();
            try {
                Categories category = ProductCat.SaveCategory(catID, catTitle, parentID, image, isLifestyle, shortDesc, longDesc);
            } catch (Exception e) {
                error_messages.Add(e.Message);
            }

            if (error_messages.Count == 0) {
                HttpContext.Response.Redirect("~/Admin_Lifestyle");
            }
            ViewBag.error_messages = error_messages;
            // Get this category
            cat = ProductCat.GetCategory(catID);
            ViewBag.cat = cat;

            // Get the categories so the use can make the new one a subcategory if they choose
            List<DetailedCategories> cats = ProductCat.GetCategories();
            ViewBag.cats = cats;

            return View();
        }

        public ActionResult Trailers() {
            List<Trailer> trailers = TrailerModel.GetAll();
            ViewBag.trailers = trailers;
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AddTrailer() {
            List<DetailedCategories> cats = ProductCat.GetLifestyles();
            ViewBag.cats = cats;
            ViewBag.lifestyles = new List<string>();
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddTrailer(List<string> lifestyles, string name = "", string image = "", int tw = 0, int gtw = 0, string hitchClass = "", string shortDesc = "", string message = "") {
            // Save the category
            List<string> error_messages = new List<string>();
            Trailer trailer = new Trailer();
            try {
                trailer = TrailerModel.Save(lifestyles, 0, name, image, tw, gtw, hitchClass, shortDesc, message);
            } catch (Exception e) {
                error_messages.Add(e.Message);
            }

            // Make sure we didn't catch any errors
            if (error_messages.Count == 0 && trailer.trailerID > 0) {
                return RedirectToAction("Trailers");
            } else {
                ViewBag.name = name;
                ViewBag.lifestyles = lifestyles;
                ViewBag.image = image;
                ViewBag.tw = tw;
                ViewBag.gtw = gtw;
                ViewBag.hitchClass = hitchClass;
                ViewBag.shortDesc = shortDesc;
                ViewBag.message = message;
                ViewBag.error_messages = error_messages;
            }

            List<DetailedCategories> cats = ProductCat.GetLifestyles();
            ViewBag.cats = cats;
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditTrailer(int id = 0) {
            List<DetailedCategories> cats = ProductCat.GetLifestyles();
            ViewBag.cats = cats;
            Trailer trailer = TrailerModel.Get(id);
            List<string> lifestyles = new List<string>();
            if (trailer.trailerID > 0) {
                lifestyles = TrailerModel.getTrailerLifestyles(id);
            }
            ViewBag.name = trailer.name;
            ViewBag.lifestyles = lifestyles;
            ViewBag.image = trailer.image;
            ViewBag.tw = trailer.TW;
            ViewBag.gtw = trailer.GTW;
            ViewBag.hitchClass = trailer.hitchClass;
            ViewBag.shortDesc = trailer.shortDesc;
            ViewBag.message = trailer.message;

            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditTrailer(List<string> lifestyles, int id = 0, string name = "", string image = "", int tw = 0, int gtw = 0, string hitchClass = "", string shortDesc = "", string message = "") {
            // Save the category
            List<string> error_messages = new List<string>();
            Trailer trailer = new Trailer();
            try {
                trailer = TrailerModel.Save(lifestyles, id, name, image, tw, gtw, hitchClass, shortDesc, message);
            } catch (Exception e) {
                error_messages.Add(e.Message);
            }

            // Make sure we didn't catch any errors
            if (error_messages.Count == 0 && trailer.trailerID > 0) {
                return RedirectToAction("Trailers");
            } else {
                ViewBag.name = name;
                ViewBag.lifestyles = lifestyles;
                ViewBag.image = image;
                ViewBag.tw = tw;
                ViewBag.gtw = gtw;
                ViewBag.hitchClass = hitchClass;
                ViewBag.shortDesc = shortDesc;
                ViewBag.message = message;
                ViewBag.error_messages = error_messages;
            }

            List<DetailedCategories> cats = ProductCat.GetLifestyles();
            ViewBag.cats = cats;
            return View();
        }

        public string DeleteTrailer(int id = 0) {
            try {
                TrailerModel.Delete(id);
                return null;
            } catch (Exception e) {
                return e.Message;
            }
        }

        public string AddTrailersToLifestyle(string trailers, int catID) {
            JavaScriptSerializer js = new JavaScriptSerializer();
            //List<string> trailerlist = new List<string>();
            string[] trailerlist = trailers.Split(',');
            List<Trailer> newtrailers = new List<Trailer>();
            try {
                newtrailers = TrailerModel.AddToLifestyle(trailerlist, catID);
            } catch { };
            return js.Serialize(newtrailers);
        }

        public string RemoveTrailerFromLifestyle(int trailerid, int catID) {
            try {
                TrailerModel.RemoveFromLifestyle(trailerid, catID);
                return null;
            } catch (Exception e) {
                return e.Message;
            }
        }

        public ActionResult EditTrailers(int catID = 0) {
            DetailedCategories cat = ProductCat.GetCategory(catID);
            ViewBag.category = cat;
            List<Trailer> trailers = TrailerModel.GetTrailersByLifestyle(cat.catID);
            ViewBag.trailers = trailers;

            return View();
        }

        public string GetTrailersJSON(int catID = 0) {
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<Trailer> trailers = TrailerModel.GetUnusedTrailersByLifestyle(catID);
            return js.Serialize(trailers);
        }
        
        public ActionResult Content(int catID = 0) {

            // Get this category
            DetailedCategories cat = ProductCat.GetCategory(catID);
            ViewBag.cat = cat;

            CurtDevDataContext db = new CurtDevDataContext();
            List<ContentType> types = new List<ContentType>();
            types = db.ContentTypes.OrderBy(x => x.type).ToList<ContentType>();
            ViewBag.types = types;

            return View();
        }
    }
}

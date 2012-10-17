using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Web.Script.Serialization;

namespace CurtAdmin.Controllers
{
    public class CategoriesController : BaseController {

        protected override void OnActionExecuted(ActionExecutedContext filterContext) {
            base.OnActionExecuted(filterContext);
            ViewBag.activeModule = "Product Categories";
        }

        /// <summary>
        /// Loads all the product categories in the database.
        /// </summary>
        /// <returns>View</returns>
        public ActionResult Index() {

            // Get the categories
            List<DetailedCategories> cats = ProductCat.GetCategories();
            ViewBag.cats = cats;


            // Get the number of unkown parts
            int unknown = ProductCat.GetUncategorizedParts().Count;
            ViewBag.unknown = unknown;

            return View();
        }

        /// <summary>
        /// Display a list of all the products under a given category
        /// </summary>
        /// <param name="catID">ID of the category</param>
        /// <returns>View</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult CategoryItems(int catID = 0, string unknown = "") {

            List<ConvertedPart> parts = new List<ConvertedPart>();
            if (catID != 0) {
                // Get the category
                DetailedCategories cat = new DetailedCategories();
                cat = ProductCat.GetCategory(catID);
                ViewBag.cat = cat;

                // Get the category items
                parts = ProductCat.GetCategoryItems(catID);

            } else if (catID == 0 && unknown == "unknown") {
                DetailedCategories cat = new DetailedCategories();
                cat.catTitle = "Uncategorized Parts";
                ViewBag.cat = cat;

                // Get the uncategorized parts
                parts = ProductCat.GetUncategorizedParts();

            }
            ViewBag.parts = parts;
            ViewBag.partCount = parts.Count();
            

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
        public ActionResult Add(string catTitle = "", int parentID = 0, string image = "", int isLifestyle = 0, string shortDesc = "", string longDesc = "", bool vehicleSpecific = false) {

            // Save the category
            List<string> error_messages = new List<string>();
            Categories cat = new Categories();
            try {
                cat = ProductCat.SaveCategory(0, catTitle, parentID, image, isLifestyle, shortDesc, longDesc, vehicleSpecific);
            } catch (Exception e) {
                error_messages.Add(e.Message);
            }

            // Make sure we didn't catch any errors
            if (error_messages.Count == 0 && cat.catID > 0) {
                return RedirectToAction("Index");
            } else {
                ViewBag.catTitle = catTitle;
                ViewBag.parentID = parentID;
                ViewBag.image = image;
                ViewBag.isLifestyle = isLifestyle;
                ViewBag.shortDesc = shortDesc;
                ViewBag.longDesc = longDesc;
                ViewBag.vehicleSpecific = vehicleSpecific;
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
        public ActionResult Edit(int catID, string catTitle = "", int parentID = 0, string image = "", int isLifestyle = 0, string shortDesc = "", string longDesc = "", bool vehicleSpecific = false) {

            List<string> error_messages = new List<string>();
            DetailedCategories cat = new DetailedCategories();
            try {
                Categories category = ProductCat.SaveCategory(catID, catTitle, parentID, image, isLifestyle, shortDesc, longDesc, vehicleSpecific);
            } catch (Exception e) {
                error_messages.Add(e.Message);
            }

            if (error_messages.Count == 0) {
                return RedirectToAction("Index");
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

        [AcceptVerbs(HttpVerbs.Get)]
        public string Delete(int catID = 0) {
            return ProductCat.DeleteCategory(catID);
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

        public string GetContent(int contentID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            JavaScriptSerializer js = new JavaScriptSerializer();
            FullContent content = new FullContent();
            try {
                content = (from c in db.Contents
                           join ct in db.ContentTypes on c.cTypeID equals ct.cTypeID
                           where c.contentID == contentID
                           select new FullContent {
                               contentID = c.contentID,
                               content = c.text,
                               content_type = ct.type,
                               content_type_id = ct.cTypeID
                           }).FirstOrDefault<FullContent>();
            } catch { };
            return js.Serialize(content);
        }

        [ValidateInput(false)]
        public string SaveContent(int catID = 0, int contentID = 0, int typeID = 0, string content = "") {
            CurtDevDataContext db = new CurtDevDataContext();
            JavaScriptSerializer js = new JavaScriptSerializer();
            FullContent newcontent = new FullContent();
            try {
                Content c = new Content();
                ContentBridge cb = new ContentBridge();
                if (contentID != 0) {
                    c = db.Contents.Where(x => x.contentID == contentID).First<Content>();
                    c.cTypeID = typeID;
                    c.text = content;
                } else {
                    c = new Content {
                        cTypeID = typeID,
                        text = content
                    };

                    db.Contents.InsertOnSubmit(c);
                    db.SubmitChanges();

                    cb = new ContentBridge {
                        catID = catID,
                        contentID = c.contentID
                    };
                    db.ContentBridges.InsertOnSubmit(cb);
                }
                db.SubmitChanges();

                newcontent = (from co in db.Contents
                                join ct in db.ContentTypes on co.cTypeID equals ct.cTypeID
                                where co.contentID == c.contentID
                                select new FullContent {
                                    contentID = co.contentID,
                                    content = co.text,
                                    content_type = ct.type,
                                    content_type_id = ct.cTypeID
                                }).FirstOrDefault<FullContent>();
            } catch { };
            return js.Serialize(newcontent);
        }

        public string DeleteContent(int contentID = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                ContentBridge cb = db.ContentBridges.Where(x => x.contentID.Equals(contentID)).First();
                db.ContentBridges.DeleteOnSubmit(cb);
                db.SubmitChanges();
                if (db.ContentBridges.Where(x => x.contentID.Equals(contentID)).Count() == 0) {
                    Content c = db.Contents.Where(x => x.contentID.Equals(contentID)).FirstOrDefault(); ;
                    db.Contents.DeleteOnSubmit(c);
                    db.SubmitChanges();
                }
                return "";
            } catch { return "Content could not be removed."; };
        }
    }
}

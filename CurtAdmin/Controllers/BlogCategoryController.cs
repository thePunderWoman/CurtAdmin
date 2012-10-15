using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Web.Script.Serialization;

namespace CurtAdmin.Controllers
{
    public class BlogCategoryController : BaseController
    {
        //
        // GET: /Author/

        public ActionResult Index()
        {
            // Get all the categories
            List<BlogCategory> categories = BlogCategoryModel.GetAll();
            ViewBag.categories = categories;
            return View();
        }

        public ActionResult Add()
        {
            return View();
        }

        public ActionResult Edit(int id = 0)
        {
            BlogCategory category = BlogCategoryModel.Get(id);
            ViewBag.category = category;

            if (category == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        public ActionResult Save(int id = 0, string name = "")
        {
            try
            {
                if (name.Length == 0) { throw new Exception("You must enter a category name"); }

                CurtDevDataContext db = new CurtDevDataContext();
                
                if (id == 0) {
                    BlogCategory category = new BlogCategory
                    {
                        name = name,
                        slug = UDF.GenerateSlug(name),
                        active = true
                    };
                    db.BlogCategories.InsertOnSubmit(category);
                } else {
                    BlogCategory category = db.BlogCategories.Where(x => x.blogCategoryID == id).FirstOrDefault<BlogCategory>();
                    category.name = name;
                    category.slug = UDF.GenerateSlug(name);
                }
                db.SubmitChanges();

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                if (id == 0)
                {
                    return RedirectToAction("Add", new { err = e.Message, name = name });
                }
                else
                {
                    return RedirectToAction("Edit", new { id = id, err = e.Message, name = name });
                }
            }
        }

        public string Delete(int id = 0)
        {
            try
            {
                CurtDevDataContext db = new CurtDevDataContext();
                BlogCategory category = (from c in db.BlogCategories
                                where c.blogCategoryID.Equals(id)
                                select c).FirstOrDefault<BlogCategory>();

                db.BlogCategories.DeleteOnSubmit(category);
                db.SubmitChanges();
                return "[]";
            }
            catch (Exception e)
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                return e.Message;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin.Models
{
    public class BlogCategoryModel
    {
        public static List<BlogCategory> GetAll() {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                List<BlogCategory> categories = new List<BlogCategory>();

                categories = db.BlogCategories.Where(x => x.active == true).ToList<BlogCategory>();

                return categories;
            } catch (Exception e) {
                return new List<BlogCategory>();
            }
        }

        public static BlogCategory Get(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                BlogCategory category = new BlogCategory();
                category = db.BlogCategories.Where(x => x.blogCategoryID == id).Where(x => x.active == true).FirstOrDefault<BlogCategory>();

                return category;
            } catch (Exception e) {
                return new BlogCategory();
            }
        }

        public static void Delete(int id = 0)
        {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                BlogCategory c = db.BlogCategories.Where(x => x.blogCategoryID == id).FirstOrDefault<BlogCategory>();
                c.active = false;
                db.SubmitChanges();
            } catch (Exception e) {
                throw e;
            }
        }
    }
}
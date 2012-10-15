using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;

namespace CurtAdmin.Controllers {
    public class NewsController : BaseController {

        public ActionResult Index() {

            // Get all the news
            List<NewsItem> news = NewsModel.GetAll();
            ViewBag.news = news;

            return View();
        }

        public ActionResult Add(string err = "", string t = "", string l = "", string c = "", string s = "", string e = "") {
            ViewBag.t = t;
            ViewBag.l = l;
            ViewBag.c = c;
            ViewBag.s = s;
            ViewBag.c = c;
            ViewBag.err = err;
            return View();
        }

        public ActionResult Edit(int id = 0, string err = "", string t = "", string l = "", string c = "", string s = "", string e = "") {
            ViewBag.t = t;
            ViewBag.l = l;
            ViewBag.c = c;
            ViewBag.s = s;
            ViewBag.e = e;
            ViewBag.err = err;

            if (id == 0) {
                return RedirectToAction("Index");
            }

            // Get the news Item
            NewsItem news = NewsModel.Get(id);
            ViewBag.news = news;

            return View();
        }

        [ValidateInput(false)]
        public ActionResult Save(int id = 0, string title = "", string lead = "", string content = "", string publishStart = "", string publishEnd = "") {
            try {
                if (title.Length == 0) { throw new Exception("You must enter a title"); }

                CurtDevDataContext db = new CurtDevDataContext();
                if (id == 0) {
                    NewsItem news = new NewsItem {
                        title = title,
                        lead = lead,
                        content = content,
                        publishStart = (publishStart == "") ? (DateTime?)null : Convert.ToDateTime(publishStart),
                        publishEnd = (publishEnd == "") ? (DateTime?)null : Convert.ToDateTime(publishEnd),
                        active = true,
                        slug = UDF.GenerateSlug(title)
                    };
                    db.NewsItems.InsertOnSubmit(news);
                } else {
                    NewsItem news = db.NewsItems.Where(x => x.newsItemID == id).FirstOrDefault<NewsItem>();
                    news.title = title;
                    news.slug = UDF.GenerateSlug(title);
                    news.lead = lead;
                    news.content = content;
                    news.publishStart = (publishStart == "") ? (DateTime?)null : Convert.ToDateTime(publishStart);
                    news.publishEnd = (publishEnd == "") ? (DateTime?)null : Convert.ToDateTime(publishEnd);
                }
                db.SubmitChanges();

                return RedirectToAction("Index");
            } catch (Exception e) {
                if (id == 0) {
                    return RedirectToAction("Add", new { err = e.Message, t = title, l = lead, c = content, s = publishStart, e = publishEnd });
                } else {
                    return RedirectToAction("Edit", new { id = id, err = e.Message, t = title, l = lead, c = content, s = publishStart, e = publishEnd });
                }
            }
        }

        public string Delete(int id = 0) {
            try {
                NewsModel.Delete(id);
                return "";
            } catch (Exception e) {
                return e.Message;
            }
        }

    }
}

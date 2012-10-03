using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin.Models {
    public class NewsModel {

        public static List<NewsItem> GetAll() {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                List<NewsItem> news = new List<NewsItem>();
                List<NewsItem> morenews = new List<NewsItem>();

                news = db.NewsItems.Where(x => x.active == true).Where(x => x.publishStart == null).ToList<NewsItem>();
                morenews = db.NewsItems.Where(x => x.active == true).Where(x => x.publishStart != null).OrderByDescending(x => x.publishStart).ThenByDescending(x => x.publishEnd).ToList<NewsItem>();

                news.AddRange(morenews);

                return news;
            } catch (Exception e) {
                return new List<NewsItem>();
            }
        }

        public static NewsItem Get(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                NewsItem newsitem = new NewsItem();
                newsitem = db.NewsItems.Where(x => x.newsItemID == id).FirstOrDefault<NewsItem>();

                return newsitem;
            } catch (Exception e) {
                return new NewsItem();
            }
        }

        public static void Delete(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                NewsItem n = db.NewsItems.Where(x => x.newsItemID == id).FirstOrDefault<NewsItem>();
                n.active = false;
                db.SubmitChanges();
            } catch (Exception e) {
                throw e;
            }
        }

    }
}
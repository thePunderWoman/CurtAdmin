using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin.Models {
    public class FAQModel {

        public static List<FAQ> GetAll() {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                List<FAQ> faqs = new List<FAQ>();

                faqs = db.FAQs.OrderBy(x => x.faqID).ToList<FAQ>();

                return faqs;        
            } catch (Exception e) {
                return new List<FAQ>();
            }
        }

        public static FAQ Get(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                FAQ faq = new FAQ();
                faq = db.FAQs.Where(x => x.faqID == id).FirstOrDefault<FAQ>();

                return faq;
            } catch (Exception e) {
                return new FAQ();
            }
        }

        public static void Delete(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                FAQ f = db.FAQs.Where(x => x.faqID == id).FirstOrDefault<FAQ>();
                db.FAQs.DeleteOnSubmit(f);
                db.SubmitChanges();
            } catch (Exception e) {
                throw e;
            }
        }

    }
}
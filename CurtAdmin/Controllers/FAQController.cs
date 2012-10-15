using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;

namespace CurtAdmin.Controllers {
    public class FAQController : BaseController {

        public ActionResult Index() {

            // Get all the faqs
            List<FAQ> faqs = FAQModel.GetAll();
            ViewBag.faqs = faqs;

            return View();
        }

        public ActionResult Add(string err = "", string q = "", string a = "") {
            ViewBag.q = q;
            ViewBag.a = a;
            ViewBag.err = err;
            return View();
        }

        public ActionResult Edit(int id = 0, string err = "", string q = "", string a = "") {
            ViewBag.q = q;
            ViewBag.a = a;
            ViewBag.err = err;

            if (id == 0) {
                return RedirectToAction("Index");
            }

            // Get the faq
            FAQ faq = FAQModel.Get(id);
            ViewBag.faq = faq;

            return View();
        }

        public ActionResult Save(int id = 0, string question = "", string answer = "") {
            try {
                if (question.Length == 0) { throw new Exception("You must enter a question"); }
                if (answer.Length == 0) { throw new Exception("You must enter an answer"); }

                CurtDevDataContext db = new CurtDevDataContext();
                if (id == 0) {
                    FAQ faq = new FAQ {
                        answer = answer,
                        question = question
                    };
                    db.FAQs.InsertOnSubmit(faq);
                } else {
                    FAQ faq = db.FAQs.Where(x => x.faqID == id).FirstOrDefault<FAQ>();
                    faq.question = question;
                    faq.answer = answer;
                }
                db.SubmitChanges();

                return RedirectToAction("Index");
            } catch (Exception e) {
                if (id == 0) {
                    return RedirectToAction("Add", new { err = e.Message, q = question, a = answer });
                } else {
                    return RedirectToAction("Edit", new { id = id, err = e.Message, q = question, a = answer });
                }
            }
        }

        public string Delete(int id = 0) {
            try {
                FAQModel.Delete(id);
                return "";
            } catch (Exception e) {
                return e.Message;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using CurtAdmin.Models;

namespace CurtAdmin.Controllers
{
    public class TestingController : Controller
    {

        public void SpamChris(int count = 0, string msg = "", int recip = 0) {

            Dictionary<int, string> numbers = new Dictionary<int, string>();
            numbers.Add(1, "7154561951@txt.att.net");
            numbers.Add(2, "7155591450@txt.att.net");
            numbers.Add(3, "7154564978@vtext.com");
            numbers.Add(4, "7155795315@vtext.com");
            numbers.Add(5, "7155291801@tmomail.com");

            string num = numbers[recip];
            SMS.SendSMS(num, msg, count);
            RedirectToAction("Index", "Testing");
        }

        public ActionResult Index() {
            return View();
        }

        public string AddColors() {
            try {
                Dictionary<int, int> parent_colors = new Dictionary<int, int>();
                parent_colors.Add(1, 1); // RH
                parent_colors.Add(15, 5); // A
                parent_colors.Add(30, 4); // S
                parent_colors.Add(40, 6); // ST
                parent_colors.Add(141, 3); // TB
                parent_colors.Add(153, 2); // BM
                parent_colors.Add(179, 8); // CM
                parent_colors.Add(192, 7); // E

                foreach (KeyValuePair<int, int> parent in parent_colors) {
                    int parentID = parent.Key;
                    int codeID = parent.Value;
                    CurtDevDataContext db = new CurtDevDataContext();

                    // Assign the parent categories color codes
                    Categories parent_cat = (from parents in db.Categories
                                             where parents.catID.Equals(parentID)
                                             select parents).FirstOrDefault<Categories>();
                    parent_cat.codeID = codeID;
                    db.SubmitChanges();

                    // Get the categories under this parent
                    List<Categories> cats = db.Categories.Where(x => x.parentID == parentID).ToList<Categories>();
                    if (cats.Count > 0) {
                        AssignChildren(cats, codeID);
                    }


                }
                return "";
            } catch (Exception e) {
                return e.Message;
            }

        }

        public void AssignChildren(List<Categories> cats, int codeID) {
            CurtDevDataContext db = new CurtDevDataContext();
            foreach (Categories cat in cats) {
                Categories c = (from c2 in db.Categories
                              where c2.catID.Equals(cat.catID)
                              select c2).FirstOrDefault<Categories>();
                c.codeID = codeID;
                db.SubmitChanges();

                // Get the children
                List<Categories> cats2 = (from c3 in db.Categories
                                        where c3.parentID.Equals(cat.catID)
                                        select c3).ToList<Categories>();
                if (cats2.Count > 0) {
                    AssignChildren(cats2, codeID);
                }
            }
        }

    }
}

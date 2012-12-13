using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace CurtAdmin.Models {
    public class ReviewModel {

        public static ReviewDetail Get(int id = 0) {
            ReviewDetail review = new ReviewDetail();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                review = (from r in db.Reviews
                          join c in db.Customers on r.cust_id equals c.customerID
                           where r.reviewID.Equals(id)
                           select new ReviewDetail {
                               reviewID = r.reviewID,
                               review_text = r.review_text,
                               cust_id = r.cust_id,
                               customer = c.name,
                               rating = r.rating,
                               subject = r.subject,
                               name = r.name,
                               email = r.email,
                               partID = r.partID,
                               active = r.active,
                               approved = r.approved,
                               createdDate = r.createdDate,
                               created = String.Format("{0:M/d/yyyy h:mm tt}", r.createdDate)
                           }).FirstOrDefault<ReviewDetail>();
            } catch {}
            return review;
        }

        public static List<Review> GetAll() {
            List<Review> reviews = new List<Review>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                reviews = db.Reviews.Where(x => x.active == true).OrderByDescending(x => x.createdDate).ToList<Review>();
            } catch { }
            return reviews;
        }

        public static List<ReviewDetail> GetAllUnapproved() {
            List<ReviewDetail> reviews = new List<ReviewDetail>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                reviews = (from r in db.Reviews
                           join c in db.Customers on r.cust_id equals c.customerID
                           where r.active.Equals(true) && r.approved.Equals(false)
                           orderby r.createdDate descending
                           select new ReviewDetail {
                               reviewID = r.reviewID,
                               review_text = r.review_text,
                               cust_id = r.cust_id,
                               customer = c.name,
                               rating = r.rating,
                               subject = r.subject,
                               name = r.name,
                               email = r.email,
                               partID = r.partID,
                               active = r.active,
                               approved = r.approved,
                               createdDate = r.createdDate,
                               created = String.Format("{0:M/d/yyyy h:mm tt}",r.createdDate)
                           }).ToList<ReviewDetail>();
            } catch { }
            return reviews;
        }

        public static List<ReviewDetail> GetAllApproved() {
            List<ReviewDetail> reviews = new List<ReviewDetail>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                reviews = (from r in db.Reviews
                           join c in db.Customers on r.cust_id equals c.customerID
                           where r.active.Equals(true) && r.approved.Equals(true)
                           orderby r.createdDate descending
                           select new ReviewDetail {
                               reviewID = r.reviewID,
                               review_text = r.review_text,
                               cust_id = r.cust_id,
                               customer = c.name,
                               rating = r.rating,
                               subject = r.subject,
                               name = r.name,
                               email = r.email,
                               partID = r.partID,
                               active = r.active,
                               approved = r.approved,
                               createdDate = r.createdDate,
                               created = String.Format("{0:M/d/yyyy h:mm tt}", r.createdDate)
                           }).ToList<ReviewDetail>();
            } catch { }
            return reviews;
        }

        public static List<ReviewDetail> GetReviews(int partID = 0) {
            List<ReviewDetail> reviews = new List<ReviewDetail>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                reviews = (from r in db.Reviews
                           join c in db.Customers on r.cust_id equals c.customerID
                           where r.active.Equals(true) && r.partID.Equals(partID)
                           orderby r.createdDate descending
                           select new ReviewDetail {
                               reviewID = r.reviewID,
                               review_text = r.review_text,
                               cust_id = r.cust_id,
                               customer = c.name,
                               rating = r.rating,
                               subject = r.subject,
                               name = r.name,
                               email = r.email,
                               partID = r.partID,
                               active = r.active,
                               approved = r.approved,
                               createdDate = r.createdDate,
                               created = String.Format("{0:M/d/yyyy h:mm tt}", r.createdDate)
                           }).ToList<ReviewDetail>();
            } catch { }
            return reviews;
        }

        public static string Add(int partID = 0, int rating = 0, string subject = "", string review_text = "", string name = "", string email = "", int reviewID = 0) {
            if (partID == 0 || review_text.Length == 0) {
                return "{\"error\":\"Invalid data.\"}";
            }

            try {
                CurtDevDataContext db = new CurtDevDataContext();
                Review r = new Review();
                if (reviewID == 0) {
                    // Create new review
                    r = new Review {
                        partID = partID,
                        rating = rating,
                        subject = subject,
                        review_text = review_text,
                        name = name,
                        email = email,
                        createdDate = DateTime.Now,
                        approved = false,
                        active = true,
                        cust_id = 1
                    };
                    db.Reviews.InsertOnSubmit(r);
                    db.SubmitChanges();

                } else {
                    r = (from rev in db.Reviews
                         where rev.reviewID.Equals(reviewID)
                         select rev).FirstOrDefault<Review>();
                    r.name = name;
                    r.review_text = review_text;
                    r.rating = rating;
                    r.subject = subject;
                    r.email = email;
                    db.SubmitChanges();

                }

                r = Get(r.reviewID);
                try {
                    ProductModels.UpdatePart((int)review.partID);
                } catch { }

                JavaScriptSerializer js = new JavaScriptSerializer();
                return js.Serialize(r);

            } catch (Exception e) {
                return "{\"error\":\"" + e.Message + "\"}";
            }
        }

        public static Boolean Remove(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                Review review = db.Reviews.Where(x => x.reviewID == id).First<Review>();
                review.active = false;
                db.SubmitChanges();
                try {
                    ProductModels.UpdatePart((int)review.partID);
                } catch { }
                return true;
            } catch { return false; }
        }

        public static string Approve(int id = 0) {
            string approvedmsg = "0";
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                Review review = db.Reviews.Where(x => x.reviewID == id).First<Review>();
                if (review.approved) {
                    review.approved = false;
                } else {
                    review.approved = true;
                    approvedmsg = "1";
                }
                db.SubmitChanges();
                ProductModels.UpdatePart((int)review.partID);
                return approvedmsg;
            } catch { return approvedmsg; }
        }
    }

    public class ReviewDetail : Review {
        public string customer { get; set; }
        public string created { get; set; }
    }
}
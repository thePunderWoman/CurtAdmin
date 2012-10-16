using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace CurtAdmin.Controllers
{
    public class ResourcesController : BaseController
    {
        //
        // GET: /Account/

        public List<int> super_users = new List<int> { 1, 7, 23 };

        public ActionResult Index()
        {
            int userID = Convert.ToInt32(Session["userID"]);
            List<resource_listing> listings = new List<resource_listing>();

            if (super_users.Contains(userID)) {
                Response.Redirect("~/Account/FullList");
            } else {
                DocsLinqDataContext db = new DocsLinqDataContext();

                // Get the listings avaialble for this user.
                
                listings = (from r in db.resource_listings
                            join ru in db.resource_users on r.resourceID equals ru.resourceID
                            where ru.userID.Equals(userID)
                            select r).Distinct().OrderBy(x => x.resource_name).ToList<resource_listing>();
            }
            ViewBag.listings = listings;
            ViewBag.userID = userID;
            ViewBag.supers = super_users;
            return View();
        }

        /// <summary>
        /// Edit page for all super users.
        /// </summary>
        /// <returns>View</returns>
        public ActionResult FullList() {
            int userID = Convert.ToInt32(Session["userID"]);
            if (!super_users.Contains(userID)) {
                Response.Redirect("~/Account");
            }
            DocsLinqDataContext db = new DocsLinqDataContext();

            List<resource_listing> listings = new List<resource_listing>();
            listings = (from r in db.resource_listings
                        select r).Distinct().OrderBy(x => x.resource_name).ToList<resource_listing>();

            List<user> users = new List<user>();
            users = (from u in db.users
                        select u).OrderBy(x => x.lname).ToList<user>();


            ViewBag.listings = listings;
            ViewBag.users = users;
            return View();
        }

        public string Add(string name = "", string url = "", string username = "", string password = "", string comments = "") {
            string response = "";
            try {
                DocsLinqDataContext db = new DocsLinqDataContext();
                resource_listing listing = new resource_listing {
                    resource_name = name,
                    resource_url = url,
                    username = username,
                    password = password,
                    comments = comments
                };
                db.resource_listings.InsertOnSubmit(listing);
                db.SubmitChanges();
                JavaScriptSerializer js = new JavaScriptSerializer();
                response = js.Serialize(listing);
            }catch(Exception e){
                response = "{\"error\":\""+ e.Message + "\"]";
            }
            return response;
        }

        public string Update(int resourceID = 0, string name = "", string url = "", string username = "", string password = "", string comments = "") {
            string response = "";
            try {
                DocsLinqDataContext db = new DocsLinqDataContext();
                resource_listing listing = (from r in db.resource_listings
                                            where r.resourceID.Equals(resourceID)
                                            select r).FirstOrDefault<resource_listing>();
                listing.resource_name = name;
                listing.resource_url = url;
                listing.username = username;
                listing.password = password;
                listing.comments = comments;

                db.SubmitChanges();

                JavaScriptSerializer js = new JavaScriptSerializer();
                response = js.Serialize(listing);
            } catch (Exception e) {
                response = "{\"error\":\"" + e.Message + "\"]";
            }
            return response;
        }

        public string Remove(int resourceID = 0) {
            string response = "";
            try {
                DocsLinqDataContext db = new DocsLinqDataContext();
                resource_listing listing = (from r in db.resource_listings
                                            where r.resourceID.Equals(resourceID)
                                            select r).FirstOrDefault<resource_listing>();
                db.resource_listings.DeleteOnSubmit(listing);
                db.SubmitChanges();
            } catch (Exception e) {
                response = e.Message;
            }
            return response;
        }

        public string GetResourceUsers(int resourceID = 0) {
            string response = "";
            try {
                DocsLinqDataContext db = new DocsLinqDataContext();
                List<resource_slim_user> users = new List<resource_slim_user>();
                users = (from u in db.users
                         join ru in db.resource_users on u.userID equals ru.userID
                         where ru.resourceID.Equals(resourceID)
                         select new resource_slim_user { 
                            user = u.fname + " " + u.lname,
                            username = u.username,
                            userID = u.userID
                         }).Distinct().OrderBy(x => x.user).ToList<resource_slim_user>();
                JavaScriptSerializer js = new JavaScriptSerializer();
                response = js.Serialize(users);
            } catch (Exception e) {

            }
            return response;
        }

        public string AddUserToResource(int resourceID = 0, int userID = 0) {
            string response = "";
            try {
                DocsLinqDataContext db = new DocsLinqDataContext();

                // Make sure this record doesn't already exist.
                int existing = (from ru in db.resource_users
                                where ru.resourceID.Equals(resourceID) && ru.userID.Equals(userID)
                                select ru).Count();
                if (existing > 0) {
                    return "{\"error\":\"This record exists.\"]";
                }

                // Create new record
                resource_user new_ru = new resource_user {
                    resourceID = resourceID,
                    userID = userID
                };
                db.resource_users.InsertOnSubmit(new_ru);
                db.SubmitChanges();

                resource_slim_user user = (from u in db.users
                                           join r in db.resource_users on u.userID equals r.userID
                                           where r.resource_user_key.Equals(new_ru.resource_user_key)
                                           select new resource_slim_user{
                                               user = u.fname + " " + u.lname,
                                               username = u.username,
                                               userID = u.userID
                                           }).FirstOrDefault<resource_slim_user>();

                JavaScriptSerializer js = new JavaScriptSerializer();
                response = js.Serialize(user);
            } catch (Exception e) {
                response = "{\"error\":\"" + e.Message + "\"]";
            }
            return response;       
        }

        public string RemoveUserFromResource(int resourceID = 0, int userID = 0) {
            string response = "";
            try {
                DocsLinqDataContext db = new DocsLinqDataContext();
                resource_user ru = (from r in db.resource_users
                                    where r.resourceID.Equals(resourceID) && r.userID.Equals(userID)
                                    select r).FirstOrDefault<resource_user>();
                db.resource_users.DeleteOnSubmit(ru);
                db.SubmitChanges();
            } catch (Exception e) {
                response = e.Message;
            }
            return response;
        }
    }

    public class resource_slim_user {
        public string user { get; set; }
        public string username { get; set; }
        public int userID { get; set; }
    }
}

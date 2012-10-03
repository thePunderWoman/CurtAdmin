using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Web.Script.Serialization;
using System.Net.Mail;

namespace CurtAdmin.Controllers {
    public class Admin_ForumController : AdminBaseController {

        /* Start Group Methods */
        public ActionResult Index() {

            List<FullGroup> groups = ForumModel.GetAllGroups();
            ViewBag.groups = groups;

            return View();

        }

        public ActionResult AddGroup() {

            ViewBag.name = "";
            ViewBag.description = "";
            ViewBag.error = "";
            return View();

        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SaveGroup(int id = 0, string name = "", string description = "") {
            try {
                if (name.Length == 0) throw new Exception("Name is required.");
                CurtDevDataContext db = new CurtDevDataContext();
                if (id == 0) {
                    ForumGroup f = new ForumGroup {
                        name = name,
                        description = description,
                        createdDate = DateTime.Now,
                    };
                    db.ForumGroups.InsertOnSubmit(f);
                } else {
                    ForumGroup f = db.ForumGroups.Where(x => x.forumGroupID == id).First<ForumGroup>();
                    f.name = name;
                    f.description = description;
                }
                db.SubmitChanges();
                return RedirectToAction("index");

            } catch (Exception e) {
                TempData["error"] = e.Message;
                TempData["name"] = name;
                TempData["description"] = description;
                if (id == 0) {
                    return RedirectToAction("AddGroup");
                } else {
                    return RedirectToAction("EditGroup", new { id = id });
                }
            }
        }

        public ActionResult EditGroup(int id = 0) {
            FullGroup group = ForumModel.GetGroup(id);
            ViewBag.group = group;
            ViewBag.name = ((string)TempData["name"] != null) ? (string)TempData["name"] : group.name;
            ViewBag.description = ((string)TempData["description"] != null) ? (string)TempData["description"] : group.description;
            ViewBag.error = ((string)TempData["error"] != null) ? (string)TempData["error"] : "";

            return View();

        }

        public ActionResult DeleteGroup(int id = 0) {
            bool success = ForumModel.DeleteGroup(id);
            return RedirectToAction("index");

        }
        /* End Group Methods */

        /* Start Topic Methods */
        public ActionResult Topics(int id = 0) {

            FullGroup group = ForumModel.GetGroup(id);
            ViewBag.group = group;

            return View();

        }

        public ActionResult AddTopic(int id = 0) {

            FullGroup group = ForumModel.GetGroup(id);
            ViewBag.group = group;
            ViewBag.name = ((string)TempData["name"] != null) ? (string)TempData["name"] : "";
            ViewBag.description = ((string)TempData["description"] != null) ? (string)TempData["description"] : "";
            ViewBag.closed = ((string)TempData["closed"] != null) ? Convert.ToBoolean((string)TempData["closed"]) : false;
            ViewBag.image = ((string)TempData["image"] != null) ? (string)TempData["image"] : "";
            ViewBag.error = ((string)TempData["error"] != null) ? (string)TempData["error"] : "";

            return View();

        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SaveTopic(int id = 0, int groupid = 0, string name = "", string description = "", string image = null, bool closed = true) {
            try {
                if (groupid == 0) throw new Exception("Group is required.");
                if (name.Length == 0) throw new Exception("Name is required.");
                CurtDevDataContext db = new CurtDevDataContext();
                if (id == 0) {
                    ForumTopic t = new ForumTopic {
                        name = name,
                        description = description,
                        closed = closed,
                        active = true,
                        image = image,
                        TopicGroupID = groupid,
                        createdDate = DateTime.Now,
                    };
                    db.ForumTopics.InsertOnSubmit(t);
                } else {
                    ForumTopic t = db.ForumTopics.Where(x => x.topicID == id).First<ForumTopic>();
                    t.name = name;
                    t.description = description;
                    t.closed = closed;
                    t.image = image;
                }
                db.SubmitChanges();
                return RedirectToAction("topics", new { id = groupid });

            } catch (Exception e) {
                TempData["error"] = e.Message;
                TempData["name"] = name;
                TempData["description"] = description;
                TempData["closed"] = closed;
                TempData["image"] = image;
                if (id == 0) {
                    return RedirectToAction("AddTopic", new { id = groupid });
                } else {
                    return RedirectToAction("EditTopic", new { id = id });
                }
            }
        }

        public ActionResult EditTopic(int id = 0) {

            FullTopic topic = ForumModel.GetTopic(id);
            ViewBag.topic = topic;
            ViewBag.groupid = topic.TopicGroupID;
            ViewBag.name = ((string)TempData["name"] != null) ? (string)TempData["name"] : topic.name;
            ViewBag.description = ((string)TempData["description"] != null) ? (string)TempData["description"] : topic.description;
            ViewBag.closed = ((string)TempData["closed"] != null) ? Convert.ToBoolean((string)TempData["closed"]) : topic.closed;
            ViewBag.image = ((string)TempData["image"] != null) ? (string)TempData["image"] : topic.image;
            ViewBag.error = ((string)TempData["error"] != null) ? (string)TempData["error"] : "";

            return View();

        }

        public ActionResult DeleteTopic(int id = 0) {
            int groupid = ForumModel.GetTopic(id).TopicGroupID;
            bool success = ForumModel.DeleteTopic(id);
            return RedirectToAction("topics", new { id = groupid });

        }
        /* End Topic Methods */

        /* Start Thread Methods */
        public ActionResult Threads(int id = 0) {

            FullTopic topic = ForumModel.GetTopic(id);
            ViewBag.topic = topic;

            return View();

        }

        public ActionResult AddPost(int id = 0) {

            FullTopic topic = ForumModel.GetTopic(id);
            ViewBag.topic = topic;
            ViewBag.titlestr = "";
            ViewBag.post = "";
            ViewBag.sticky = false;
            ViewBag.error = "";

            return View();

        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddPost(int id = 0, string titlestr = "", string post = "", bool sticky = false) {
            string remoteip = (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null) ? Request.ServerVariables["HTTP_X_FORWARDED_FOR"] : Request.ServerVariables["REMOTE_ADDR"];
            try {
                if (id == 0) throw new Exception("Topic is required.");
                if (titlestr.Trim().Length == 0) throw new Exception("Title is required.");
                if (post.Trim().Length == 0) throw new Exception("Post content is required.");
                CurtDevDataContext db = new CurtDevDataContext();

                ForumThread t = new ForumThread {
                    topicID = id,
                    active = true,
                    closed = false,
                    createdDate = DateTime.Now
                };
                db.ForumThreads.InsertOnSubmit(t);
                db.SubmitChanges();
                user u = Users.GetUser(Convert.ToInt32(Session["userID"]));

                ForumPost p = new ForumPost {
                    threadID = t.threadID,
                    title = titlestr,
                    post = post,
                    IPAddress = remoteip,
                    createdDate = DateTime.Now,
                    approved = true,
                    active = true,
                    flag = false,
                    notify = false,
                    parentID = 0,
                    name = u.fname + " " + u.lname,
                    company = "CURT Manufacturing",
                    email = u.email,
                    sticky = sticky
                };
                db.ForumPosts.InsertOnSubmit(p);
                db.SubmitChanges();

                return RedirectToAction("threads", new { id = id });

            } catch (Exception e) {
                FullTopic topic = ForumModel.GetTopic(id);
                ViewBag.topic = topic;
                ViewBag.error = e.Message;
                ViewBag.titlestr = titlestr;
                ViewBag.post = post;
                ViewBag.sticky = sticky;
                return View();
            }
        }

        public ActionResult Thread(int id = 0) {
            CurtDevDataContext db = new CurtDevDataContext();

            Thread thread = ForumModel.GetThread(id);
            ViewBag.thread = thread;
            int groupid = db.ForumTopics.Where(x => x.topicID == thread.topicID).Select(x => x.TopicGroupID).First();
            ViewBag.groupid = groupid;

            return View();

        }
        /* End Thread Methods */

        /* Start Spam Methods */
        public ActionResult spam() {

            List<Post> posts = ForumModel.GetSpam();
            ViewBag.posts = posts;

            return View();

        }
        /* End Spam Methods*/

        /* Start Moderation Methods */
        public ActionResult moderate() {

            List<Post> posts = ForumModel.GetUnapproved();
            ViewBag.posts = posts;

            return View();

        }
        /* End Moderation Methods*/

        /* Start IP Blocking Management Methods */
        public ActionResult blockedIPs() {

            List<BlockedIPAddress> ips = ForumModel.GetBlockedIPs();
            ViewBag.ips = ips;

            return View();

        }

        public ActionResult RemoveIPBlock(int id = 0) {

            bool success = ForumModel.RemoveIPBlock(id);

            return RedirectToAction("blockedIPs");

        }
        /* End IP Blocking Management Methods*/

        /* Start AJAX Methods */

        public string DeletePost(int id = 0) {
            string error = "";
            CurtDevDataContext db = new CurtDevDataContext();
            ForumPost p = db.ForumPosts.Where(x => x.postID == id).First<ForumPost>();
            int count = db.ForumPosts.Where(x => x.parentID == id).Count();
            if (count == 0) {
                p.active = false;
                db.SubmitChanges();
            } else {
                error = "Cannot delete since the message has replies";
            }
            return error;
        }

        public string FlagPost(int id = 0) {
            string error = "";
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                ForumPost p = db.ForumPosts.Where(x => x.postID == id).First<ForumPost>();
                p.active = false;
                p.flag = true;
                p.approved = false;
                db.SubmitChanges();
            } catch (Exception e) {
                error = e.Message;
            }
            return error;
        }

        public string UnFlagPost(int id = 0) {
            string error = "";
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                ForumPost p = db.ForumPosts.Where(x => x.postID == id).First<ForumPost>();
                p.active = true;
                p.flag = false;
                p.approved = true;
                db.SubmitChanges();
            } catch (Exception e) {
                error = e.Message;
            }
            return error;
        }

        public string BlockIP(int id = 0, string reason = "") {
            string error = "";
            try {
                user u = Users.GetUser(Convert.ToInt32(Session["userID"]));
                CurtDevDataContext db = new CurtDevDataContext();
                ForumPost p = db.ForumPosts.Where(x => x.postID == id).First<ForumPost>();
                p.active = false;
                p.flag = true;
                p.approved = false;

                IPBlock ipb = new IPBlock {
                    IPAddress = p.IPAddress,
                    createdDate = DateTime.Now,
                    reason = reason,
                    userID = u.userID
                };

                db.IPBlocks.InsertOnSubmit(ipb);
                db.SubmitChanges();
            } catch (Exception e) {
                error = e.Message;
            }
            return error;
        }
        
        public string Approve(int id = 0) {
            string approval = "";
            CurtDevDataContext db = new CurtDevDataContext();
            ForumPost p = db.ForumPosts.Where(x => x.postID == id).First<ForumPost>();
            if (p.approved) {
                p.approved = false;
                approval = "Approve";
            } else {
                p.approved = true;
                approval = "Unapprove";
            }
            db.SubmitChanges();
            return approval;
        }

        public string SavePost(int threadid, int postid = 0, int parentid = 0, bool edit = false, string title = "", string post = "", bool sticky = false) {
            string remoteip = (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null) ? Request.ServerVariables["HTTP_X_FORWARDED_FOR"] : Request.ServerVariables["REMOTE_ADDR"];
            JavaScriptSerializer js = new JavaScriptSerializer();
            ForumPost p = new ForumPost();
            try {
                if (threadid == 0) throw new Exception("ThreadID is required.");
                if (title.Trim().Length == 0) throw new Exception("Title is required.");
                if (post.Trim().Length == 0) throw new Exception("Post content is required.");
                CurtDevDataContext db = new CurtDevDataContext();
                user u = Users.GetUser(Convert.ToInt32(Session["userID"]));

                if (edit) {
                    p = db.ForumPosts.Where(x => x.postID == postid).First<ForumPost>();
                    p.title = title;
                    p.post = post;
                    p.sticky = sticky;
                } else {
                    p = new ForumPost {
                        threadID = threadid,
                        title = title,
                        post = post,
                        IPAddress = remoteip,
                        createdDate = DateTime.Now,
                        approved = true,
                        active = true,
                        flag = false,
                        notify = false,
                        parentID = parentid,
                        name = u.fname + " " + u.lname,
                        company = "CURT Manufacturing",
                        email = u.email,
                        sticky = sticky
                    };
                    db.ForumPosts.InsertOnSubmit(p);
                    sendNotifications(threadid);
                }
                db.SubmitChanges();
            } catch (Exception e) {}
            return GetPost(p.postID);
        }

        public string GetPost(int postID = 0) {
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(ForumModel.GetPost(postID));
        }
        /* End AJAX Methods */

        /* Start Email Methods */
        private void sendNotifications(int threadid = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            SmtpClient SmtpServer = new SmtpClient();
            List<string> emails = new List<string>();
            Thread t = ForumModel.GetThread(threadid);
            emails = db.ForumPosts.Where(x => x.threadID == threadid).Where(x => x.notify == true).Select(x => x.email).Distinct().ToList();
            foreach (string email in emails) {
                try {
                    MailMessage mail = new MailMessage();

                    mail.To.Add(email);
                    mail.Subject = "CURT Manufacturing Forum Reply Notification";

                    mail.IsBodyHtml = true;
                    string htmlBody;

                    htmlBody = "<div style='margin-top: 15px;font-family: Arial;font-size: 10pt;'>";
                    htmlBody += "<h4>Hi There!</h4>";
                    htmlBody += "<p>Someone replied to your post on the CURT Manufacturing forum. Visit the following link to see the reply:</p>";
                    htmlBody += "<p style='margin:2px 0px'><a href='http://beta.curtmfg.com/Forum/Discussion/" + threadid + "/" + UDF.GenerateSlug(t.firstPost.title) + "'>" + t.firstPost.title + "</a></p>";
                    htmlBody += "______________________________________________________________________";
                    htmlBody += "<br /><span style='color:#999'>Thank you,</span>";
                    htmlBody += "<br /><br /><br />";
                    htmlBody += "<span style='line-height:75px;color:#999'>CURT Manufacturing Forums</span>";
                    htmlBody += "<p style='font-size: 11px;'>To unsubscribe from future notifications, click the unsubscribe link. <a href='http://beta.curtmfg.com/Forum/Unsubscribe/" + threadid + "?email=" + email + "'>Unsubscribe</a></p>";
                    htmlBody += "</div>";

                    mail.Body = htmlBody;

                    SmtpServer.Send(mail);
                } catch { };
            }
        }
        /* End Email Methods */ 
    }
}

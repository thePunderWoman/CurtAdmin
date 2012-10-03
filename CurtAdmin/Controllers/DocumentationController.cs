using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;


namespace CurtAdmin.Controllers
{
    public class DocumentationController : UserBaseController
    {
        protected override void OnActionExecuted(ActionExecutedContext filterContext) {
            base.OnActionExecuted(filterContext);
            ViewBag.activeModule = "Documentation";
        }

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public ActionResult Index() {

            // Get the this modules information
            module m = new module();
            m = Mods.GetModuleByName("Documentation");
            ViewBag.module = m;

            // Get the categories for this module
            List<category> mod_categories = new List<category>();
            mod_categories = Documentation.GetParentCategories(m.moduleID);
            ViewBag.cateogries = mod_categories;

            // Get the modules for this user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;

            return View();
        }

        /// <summary>
        /// Categories the specified cat_id.
        /// </summary>
        /// <param name="cat_id">The cat_id.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Category(string cat_id) {

            // Convert cat_id into an integer
            int catID = Convert.ToInt32(cat_id);
            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            // Get the information for this category
            category cat = new category();
            cat = Documentation.GetCategory(catID);
            ViewBag.category = cat;

            // Get the items associated with this category
            List<docItem> items = new List<docItem>();
            items = Documentation.GetCategoryItems(catID);
            ViewBag.items = items;


            // Get the this modules information
            module m = new module();
            m = Mods.GetModuleByName("Documentation");
            ViewBag.module = m;

            // Get the categories for this module
            List<category> mod_categories = new List<category>();
            mod_categories = Documentation.GetParentCategories(m.moduleID);
            ViewBag.cateogries = mod_categories;

            // Get the sub categories for this category
            List<category> sub_categories = new List<category>();
            sub_categories = Documentation.GetSubCategories(catID);
            if (sub_categories.Count == 0) { // Get parent categories
                sub_categories = (from c in doc_db.categories
                                  where c.parentID.Equals(cat.parentID)
                                  select c).ToList<category>();
            }
            ViewBag.sub_categories = sub_categories;

            // Get the modules for this user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;

            return View();
        }

        /// <summary>
        /// Items the specified item_id.
        /// </summary>
        /// <param name="item_id">The item_id.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Item(string item_id) {

            // Convert cat_id into an integer
            int itemID = Convert.ToInt32(item_id);
            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            // Get the item information
            docItem item = Documentation.GetItem(itemID);
            ViewBag.item = item;

            // Get the comments on this item
            List<UserComment> comments = new List<UserComment>();
            comments = Documentation.GetItemComments(itemID);
            ViewBag.comments = comments;

            // Get the documents for this item
            List<document> docs = (from d in doc_db.documents
                                       join id in doc_db.itemDocs on d.docID equals id.docID
                                       where id.itemID.Equals(itemID)
                                       select d).ToList<document>();
            ViewBag.docs = docs;

            // Get the this modules information
            module m = new module();
            m = Mods.GetModuleByName("Documentation");
            ViewBag.module = m;

            // Get the categories for this module
            List<category> mod_categories = new List<category>();
            mod_categories = Documentation.GetParentCategories(m.moduleID);
            ViewBag.cateogries = mod_categories;


            // Get the modules for this user
            List<module> modules = new List<module>();
            modules = Users.GetUserModules(Convert.ToInt32(Session["userID"]));
            ViewBag.Modules = modules;

            return View();
        }

        /// <summary>
        /// Adds the comment.
        /// </summary>
        /// <param name="item_id">The item_id.</param>
        /// <param name="comment">The comment.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [AcceptVerbs(HttpVerbs.Get)]
        public string AddComment(int item_id, string comment, int replyTo = 0) {
            int userID = Convert.ToInt32(Session["userID"]);
            CommentResponseObject comment_response = new CommentResponseObject();
            string error = "";
            if (userID > 0) {
                
                try {
                    DocsLinqDataContext doc_db = new DocsLinqDataContext();
                    itemComment new_comment = new itemComment {
                        userID = userID,
                        comment = Uri.UnescapeDataString(comment),
                        itemID = item_id,
                        parentComment = replyTo,
                        dateAdded = DateTime.Now
                    };
                    doc_db.itemComments.InsertOnSubmit(new_comment);
                    doc_db.SubmitChanges();

                    user u = (from users in doc_db.users
                              where users.userID.Equals(userID)
                              select users).Single<user>();
                    comment_response = new CommentResponseObject{
                        error = error,
                        itemID = item_id,
                        commentID = new_comment.commentID,
                        comment = comment,
                        userID = userID,
                        fname = u.fname,
                        lname = u.lname,
                        isAdmin = u.isAdmin
                    };
                    Documentation.SendNewComment(new_comment.commentID, item_id);

                } catch (Exception e) {
                    comment_response = new CommentResponseObject {
                        error = e.Message
                    };
                    
                }
            }
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return serializer.Serialize(comment_response);
        }

        /// <summary>
        /// Removes the comment.
        /// </summary>
        /// <param name="comment_id">The comment_id.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [AcceptVerbs(HttpVerbs.Get)]
        public string RemoveComment(int comment_id) {
            string error = "";
            if (comment_id > 0) {
                try {
                    DocsLinqDataContext doc_db = new DocsLinqDataContext();
                    itemComment comment = (from ic in doc_db.itemComments
                                           where ic.commentID.Equals(comment_id)
                                           select ic).Single<itemComment>();
                    doc_db.itemComments.DeleteOnSubmit(comment);

                    // Get all of the replies tied to this comment
                    List<itemComment> replies = (from ic in doc_db.itemComments
                                                 where ic.itemID.Equals(comment.parentComment)
                                                 select ic).ToList<itemComment>();
                    foreach (itemComment reply in replies){
                        error = RemoveComment(reply.itemID);
                    }

                    doc_db.SubmitChanges();
                } catch (Exception e) {
                    error = e.Message;
                }
            }
            return error;
        }
    }
}

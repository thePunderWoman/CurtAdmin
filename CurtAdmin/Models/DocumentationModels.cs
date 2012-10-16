/*
 * Author       : Alex Ninneman
 * Created      : January 20, 2011
 * Description  : This model will be used to create methods that can't be used to getting and settings certain data that is relevant to documentation.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Text;
using System.Data.Linq;
using System.Net.Mail;

namespace CurtAdmin.Models {

    public class Documentation {


        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <param name="catID">The cat ID.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static docCategory GetCategory(int catID = 0) {
            docCategory cat = new docCategory();
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            if (catID > 0) {
                cat = (from c in doc_db.docCategories
                       where c.catID.Equals(catID)
                       select c).Single<docCategory>();
            }
            return cat;
        }


        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <param name="moduleID">The module ID.</param>
        /// <returns>List of category</returns>
        /// <remarks></remarks>
        public static List<docCategory> GetCategories(int moduleID = 0) {
            List<docCategory> cats = new List<docCategory>();
            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            if (moduleID != 0) {
                cats = (from c in doc_db.docCategories
                        orderby c.catName
                        select c).ToList<docCategory>();
            } else {
                cats = doc_db.docCategories.OrderBy(c => c.catName).ToList<docCategory>();
            }

            return cats;
        }

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <param name="catID">The cat ID.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<docCategory> GetSubCategories(int catID = 0) {
            List<docCategory> cats = new List<docCategory>();
            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            if (catID != 0) {
                cats = (from c in doc_db.docCategories
                        where c.parentID.Equals(catID)
                        select c).ToList<docCategory>();
            }

            return cats;
        }

        /// <summary>
        /// Gets the parent categories.
        /// </summary>
        /// <param name="moduleID">The module ID.</param>
        /// <returns>List of categories</returns>
        /// <remarks></remarks>
        public static List<docCategory> GetParentCategories(int moduleID = 0) {
            List<docCategory> cats = new List<docCategory>();
            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            if (moduleID != 0) {
                cats = (from c in doc_db.docCategories
                        where c.parentID.Equals(0)
                        orderby c.catName
                        select c).ToList<docCategory>();
            } else {
                cats = (from c in doc_db.docCategories
                        where c.parentID.Equals(0)
                        orderby c.catName
                        select c).ToList<docCategory>();
            }
            return cats;

        }

        /// <summary>
        /// Gets the category items.
        /// </summary>
        /// <param name="catID">The cat ID.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<docItem> GetCategoryItems(int catID = 0) {
            List<docItem> items = new List<docItem>();
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            items = (from di in doc_db.docItems
                     join ci in doc_db.cat_items on di.itemID equals ci.itemID
                     where ci.catID.Equals(catID)
                     select di).Distinct().ToList<docItem>();
            return items;
        }

        /// <summary>
        /// Gets all items.
        /// </summary>
        /// <returns>List of docItems</returns>
        /// <remarks></remarks>
        public static List<docItem> GetAllItems() {

            List<docItem> items = new List<docItem>();
            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            items = (from di in doc_db.docItems
                     orderby di.itemID
                     select di).ToList<docItem>();
            return items;
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <param name="itemID">The item ID.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static docItem GetItem(int itemID = 0) {
            docItem item = new docItem();
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            item = (from i in doc_db.docItems
                    where i.itemID.Equals(itemID)
                    select i).Single<docItem>();
            return item;
        }

        /// <summary>
        /// Gets the item comments.
        /// </summary>
        /// <param name="itemID">The item ID.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<UserComment> GetItemComments(int itemID = 0) {
            List<UserComment> comments = new List<UserComment>();
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            comments = (from ic in doc_db.itemComments
                        join u in doc_db.users on ic.userID equals u.userID
                        where ic.itemID.Equals(itemID) && ic.parentComment.Equals(0)
                        select new UserComment { 
                            commentID = ic.commentID,
                            userID = ic.userID,
                            comment = ic.comment,
                            dateAdded = Convert.ToDateTime(ic.dateAdded),
                            itemID = ic.itemID,
                            parentComment = ic.parentComment,
                            fname = u.fname,
                            lname = u.lname
                        }).ToList<UserComment>();
            return comments;
        }

        /// <summary>
        /// Gets the item comment replies.
        /// </summary>
        /// <param name="commentID">The comment ID.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<UserComment> GetItemCommentReplies(int commentID = 0) {
            List<UserComment> comments = new List<UserComment>();
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            comments = (from ic in doc_db.itemComments
                        join u in doc_db.users on ic.userID equals u.userID
                        where ic.parentComment.Equals(commentID)
                        select new UserComment {
                            commentID = ic.commentID,
                            userID = ic.userID,
                            comment = ic.comment,
                            dateAdded = Convert.ToDateTime(ic.dateAdded),
                            itemID = ic.itemID,
                            parentComment = ic.parentComment,
                            fname = u.fname,
                            lname = u.lname
                        }).ToList<UserComment>();
            return comments;
        }

        /// <summary>
        /// Gets the doc types.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<documentType> GetDocTypes() {
            List<documentType> docTypes = new List<documentType>();
            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            docTypes = doc_db.documentTypes.OrderBy(d => d.type).ToList<documentType>();

            return docTypes;
        }


        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="itemID">The item ID.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string RemoveItem(int itemID) {
            string error = "";
            try {
                DocsLinqDataContext doc_db = new DocsLinqDataContext();

                // Get the item and remove it
                docItem item = (from di in doc_db.docItems
                                where di.itemID.Equals(itemID)
                                select di).Single<docItem>();
                doc_db.docItems.DeleteOnSubmit(item);

                // Get the category entrees for this item and remove them
                List<cat_item> item_cats = (from ci in doc_db.cat_items
                                            where ci.itemID.Equals(item.itemID)
                                            select ci).ToList<cat_item>();
                foreach (cat_item cat in item_cats) {
                    doc_db.cat_items.DeleteOnSubmit(cat);
                }

                // Get the document entrees for this item and remove them [itemDoc]
                List<itemDoc> item_docs = (from id in doc_db.itemDocs
                                           where id.itemID.Equals(item.itemID)
                                           select id).ToList<itemDoc>();
                foreach (itemDoc doc in item_docs) {
                    doc_db.itemDocs.DeleteOnSubmit(doc);
                }

                // Get the comments on this item and remove them [itemComments]
                List<itemComment> comments = (from ic in doc_db.itemComments
                                                  where ic.itemID.Equals(item.itemID)
                                                  select ic).ToList<itemComment>();
                foreach (itemComment comment in comments) {
                    doc_db.itemComments.DeleteOnSubmit(comment);
                }

                doc_db.SubmitChanges(); // Commit all changes
            } catch (Exception e) {
                error = e.Message;
            }
            return error;
        }

        /// <summary>
        /// Sends the new comment notification via email.
        /// </summary>
        /// <param name="commentID">The comment ID.</param>
        /// <param name="itemID">The item ID.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string SendNewComment(int commentID, int itemID) {
            string error = "";
            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            // Get the author of the item
            user thisUser = (from users in doc_db.users
                             join ic in doc_db.itemComments on users.userID equals ic.userID
                             where ic.commentID.Equals(commentID)
                             select users).Single<user>();

            // Get the comment and the user who commented
            UserComment comment = (from ic in doc_db.itemComments
                                    join u in doc_db.users on ic.userID equals u.userID
                                    where ic.commentID.Equals(commentID)
                                    select new UserComment {
                                        commentID = ic.commentID,
                                        userID = ic.userID,
                                        comment = ic.comment,
                                        dateAdded = Convert.ToDateTime(ic.dateAdded),
                                        itemID = ic.itemID,
                                        parentComment = ic.parentComment,
                                        fname = u.fname,
                                        lname = u.lname
                                    }).Single<UserComment>();

            // Get the item
            docItem item = (from di in doc_db.docItems
                            where di.itemID.Equals(itemID)
                            select di).Single<docItem>();

            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient();

            mail.To.Add(thisUser.email);
            mail.Subject = "CURT Administration - "+ comment.fname + " " + comment.lname + " Commented on your item.";

            mail.IsBodyHtml = true;
            string htmlBody;

            htmlBody = "<div style='margin-top: 15px;font-family: Arial;font-size: 10pt;'>";
            htmlBody += "<h4>Dear " + thisUser.fname + " " + thisUser.lname + ",</h4>";
            htmlBody += "<p>"+comment.fname + " " + comment.lname +" posted a comment on  "+ item.itemName +".</p>";
            htmlBody += "<div style='width: auto;padding: 10px;border: 1px dotted #999'>";
            htmlBody += "<p style='width: 100%;color:#343434'><strong>" + comment.comment + "</strong></p>";
            htmlBody += "</div>";
            htmlBody += "<p>You may log into your account and view the comment at <a href='http://admin.curtmfg.com'>http://admin.curtmfg.com</a></p>";
            htmlBody += "______________________________________________________________________";
            htmlBody += "<p>If you feel this has been sent by mistake, please contact Web Support at <a href='mailto:websupport@curtmfg.com' target='_blank'>websupport@curtmfg.com</a>.</p>";
            htmlBody += "<br /><span style='color:#999'>Thank you,</span>";
            htmlBody += "<br /><br /><br />";
            htmlBody += "<span style='line-height:75px;color:#999'>CURT Administration</span>";
            htmlBody += "</div>";

            mail.Body = htmlBody;

            SmtpServer.Send(mail);

            return error;
        }

        /// <summary>
        /// Delete a given document from the syste,
        /// </summary>
        /// <param name="docID">ID of the document.</param>
        public static void DeleteDocument(int docID = 0) {
            if (docID <= 0) { throw new Exception("Doc ID is invalid."); }
            DocsLinqDataContext db = new DocsLinqDataContext();

            document doc = new document();
            doc = (from d in db.documents
                   where d.docID.Equals(docID)
                   select d).FirstOrDefault<document>();

            List<itemDoc> item_docs = new List<itemDoc>();
            item_docs = (from id in db.itemDocs
                         where id.docID.Equals(doc.docID)
                         select id).ToList<itemDoc>();

            string dir = @AppDomain.CurrentDomain.BaseDirectory.Replace("\\\\","\\") + doc.documentPath.Replace("~", "").Replace("/","\\").Replace("\\\\","\\");
            //string file = System.IO.Directory.GetFiles(dir)[0];
            System.IO.File.Delete(dir);

            db.documents.DeleteOnSubmit(doc);
            db.itemDocs.DeleteAllOnSubmit<itemDoc>(item_docs);
            db.SubmitChanges();
        }

    }
}

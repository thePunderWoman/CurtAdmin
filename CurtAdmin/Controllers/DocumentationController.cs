using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.IO;
using System.Security.AccessControl;

namespace CurtAdmin.Controllers
{
    /// <summary>
    /// Documentation Items Controller
    /// </summary>
    public class DocumentationController : BaseController
    {

        /* Begin Category methods */

        /// <summary>
        /// Load all of the categories in the database so the user can make actions on them.
        /// </summary>
        /// <returns>View of categories</returns>
        public ActionResult Index() {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            // Get all categories
            List<docCategory> categories = (from c in doc_db.docCategories
                                            orderby c.catName
                                            select c).ToList<docCategory>();
            ViewBag.categories = categories;

            return View();
        }

        /// <summary>
        /// Loads the blank category page so that the user can add a brand new category. The user will be able to choose a category to mark as the parent category.
        /// </summary>
        /// <returns>View of new category.</returns>
        public ActionResult AddCategory() {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();

            // Get all categories
            List<docCategory> categories = (from c in doc_db.docCategories
                                         orderby c.catName
                                            select c).ToList<docCategory>();
            ViewBag.categories = categories;

            return View();
        }

        /// <summary>
        /// Handle the submission of a new category.
        /// </summary>
        /// <param name="catName"></param>
        /// <returns>Redirects to list of categories on completion. If an error is captured, the user is brought back the Add Category page to refine errors.</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddCategory(string catName) {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            catName = Request.Form["catName"].Trim();
            int parentID = Convert.ToInt32(Request.Form["parentID"].Trim());
            string comments = Request.Form["comments"].Trim();

            // Initiate our error message collection
            List<string> error_messages = new List<string>();

            // Make sure the category name is not blank.
            if (catName.Length == 0) { error_messages.Add("Category name is required."); }


            if (error_messages.Count == 0) {
                // Create new category object
                docCategory newCat = new docCategory {
                    catName = catName,
                    parentID = parentID,
                    moduleID = 0,
                    comments = comments
                };
                doc_db.docCategories.InsertOnSubmit(newCat);

                try { // Attempt to save the category
                    doc_db.SubmitChanges();
                    return RedirectToAction("Index");
                } catch (Exception e) {
                    error_messages.Add(e.Message);
                }
            }

            ViewBag.catName = catName;
            ViewBag.parentID = parentID;
            ViewBag.comments = comments;
            ViewBag.error_messages = error_messages;

            // Get all categories
            List<docCategory> categories = (from c in doc_db.docCategories
                                         orderby c.catName
                                            select c).ToList<docCategory>();
            ViewBag.categories = categories;

            return View();
        }

        /// <summary>
        /// Edit information for a given category.
        /// </summary>
        /// <param name="cat_id"></param>
        /// <returns>View containing information for this category.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditCategory(string cat_id) {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            int catID = Convert.ToInt32(cat_id);

            // Get all categories
            List<docCategory> categories = (from cats in doc_db.docCategories
                                         orderby cats.catName
                                            select cats).ToList<docCategory>();
            ViewBag.categories = categories;

            // Get the category to be updated
            docCategory cat = new docCategory();
            cat = (from c in doc_db.docCategories
                   where c.catID.Equals(catID)
                   select c).FirstOrDefault<docCategory>();
            ViewBag.cat = cat;

            return View();
        }

        /// <summary>
        /// Handles the submission of changes to a given category
        /// </summary>
        /// <param name="cat_id"></param>
        /// <param name="catName"></param>
        /// <returns>Redirects to list of categories on completion. If an error is captured, the user is brought back to the Edit Category page to refine errors.</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditCategory(string cat_id, string catName) {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            int catID = Convert.ToInt32(cat_id);
            catName = Request.Form["catName"].Trim();
            int parentID = Convert.ToInt32(Request.Form["parentID"].Trim());
            string comments = Request.Form["comments"];

            // Initiate error messages list
            List<string> error_messages = new List<string>();

            // validate category info
            if (catName.Length == 0) { error_messages.Add("Category name is required."); }

            // Get the category to be updated
            docCategory cat = new docCategory();
            cat = (from c in doc_db.docCategories
                   where c.catID.Equals(catID)
                   select c).FirstOrDefault<docCategory>();

            if (error_messages.Count == 0) { // Attempt to save category
                cat.catName = catName;
                cat.parentID = parentID;
                cat.moduleID = 0;
                cat.comments = comments;

                try {
                    doc_db.SubmitChanges();
                    return RedirectToAction("Index");
                } catch (Exception e) {
                    error_messages.Add(e.Message);
                }
            }

            // Get all categories
            List<docCategory> categories = (from cats in doc_db.docCategories
                                         orderby cats.catName
                                            select cats).ToList<docCategory>();
            ViewBag.categories = categories;


            ViewBag.cat = cat;
            ViewBag.error_messages = error_messages;

            return View();

        }

        /* End Category Methods */
        /* Item Methods */

        /// <summary>
        /// Display all of the documentation items in the database in a tabular format so that we can make actions on them.
        /// </summary>
        /// <returns>View of all documentation items</returns>
        public ActionResult Items()
        {

            // Get all document items
            List<docItem> items = Documentation.GetAllItems();
            ViewBag.items = items;

            return View();
        }

        /// <summary>
        /// Display all of the documentation items for a given category
        /// </summary>
        /// <param name="catID">The category to list items for.</param>
        /// <returns>View of category items in a tabular format.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ViewCatItems(int catID) {

            // Get the categories items
            List<docItem> items = Documentation.GetCategoryItems(catID);
            ViewBag.items = items;

            // Get the category to be updated
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            docCategory cat = (from c in doc_db.docCategories
                   where c.catID.Equals(catID)
                   select c).FirstOrDefault<docCategory>();
            ViewBag.cat = cat;

            return View("Items");
        }


        /// <summary>
        /// Adds new documentation item instance.
        /// </summary>
        /// <returns>View</returns>
        /// <remarks></remarks>
        public ActionResult AddItem() {

            // Get all of the categories
            List<docCategory> categories = Documentation.GetCategories();
            ViewBag.categories = categories;            

            // get all of the users ::: this will allow us to designate the author of the new item
            List<user> users = Users.GetAllUsers(1);
            ViewBag.users = users;

            return View();
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddItem(string itemName) {

            itemName = Request.Form["itemName"].Trim();
            string itemDescription = Request.Form["itemDescription"].Trim();
            string executionExample = Request.Form["executionExample"].Trim();
            string resultExample = Request.Form["resultExample"].Trim();
            string codeLink = Request.Form["codeLink"].Trim();
            int author = Convert.ToInt32(Request.Form["author"].Trim());
            string itemHTML = Request.Form["html"].Trim();
            List<string> catArray = new List<string>();
            catArray = (Request.Form["cat"] != null)?Request.Form["cat"].Trim().Split(',').ToList<string>():new List<string>();
            int item_id = 0;


            // Validate the fields
            List<string> error_messages = new List<string>();
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            if (itemName.Length == 0) { error_messages.Add("You must enter a name for this item."); }
            if (itemDescription.Length == 0) { error_messages.Add("You must enter a brief overview of this item."); }
            //if (catArray.Count == 0) { error_messages.Add("You did not specify a category for this item."); }


            if (error_messages.Count == 0) { // Add new item to database

                // Create new itemDoc object
                docItem item = new docItem { 
                    itemName = itemName,
                    itemDescription = itemDescription,
                    executionExample = executionExample,
                    resultExample = resultExample,
                    codeLink = codeLink,
                    dateModified = DateTime.Now,
                    author = author,
                    itemHTML = itemHTML
                };
                doc_db.docItems.InsertOnSubmit(item);
                item_id = item.itemID;
                try { // Commit new item

                    doc_db.SubmitChanges();

                    // Get the itemID of the newly created entry
                    int itemID = item.itemID;

                    // Now we need to associate this item with the selected categories
                    if (catArray.Count > 0) {
                        foreach (string cat in catArray) {
                            if (cat.Trim().Length > 0) {
                                cat_item ci = new cat_item {
                                    itemID = itemID,
                                    catID = Convert.ToInt32(cat)
                                };
                                doc_db.cat_items.InsertOnSubmit(ci);
                            }
                        }
                    }

                    try { // Commit cat_items and redirect
                        doc_db.SubmitChanges();
                        return RedirectToAction("Items");
                    } catch (Exception e) {
                        error_messages.Add(e.Message);
                    }

                } catch (Exception e) {
                    error_messages.Add(e.Message);
                }
            }

            // Store the fields in the ViewBag
            ViewBag.itemName = itemName;
            ViewBag.itemDescription = itemDescription;
            ViewBag.executionExample = executionExample;
            ViewBag.resultExample = resultExample;
            ViewBag.codeLink = codeLink;
            ViewBag.author = author;
            ViewBag.itemHTML = itemHTML;
            ViewBag.error_messages = error_messages;

            // Get all of the categories
            List<docCategory> categories = Documentation.GetCategories();
            ViewBag.categories = categories;

            // get all of the users ::: this will allow us to designate the author of the new item
            List<user> users = Users.GetAllUsers(1);
            ViewBag.users = users;

            return View();
        }

        /// <summary>
        /// Edit a given instance of a documentation item.
        /// </summary>
        /// <param name="item_id">The ID of the documenation item.</param>
        /// <returns>View containing all of the items information.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult EditItem(string item_id) {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            int itemID = Convert.ToInt32(item_id);

            // Get the item
            docItem item = (from di in doc_db.docItems
                           where di.itemID.Equals(itemID)
                           select di).Single<docItem>();
            ViewBag.item = item;

            // Get the cateogries that this item is associated with
            List<docCategory> item_cats = new List<docCategory>();
            item_cats = (from ci in doc_db.cat_items
                         join cats in doc_db.docCategories on ci.catID equals cats.catID
                         where ci.itemID.Equals(item.itemID)
                         select cats).ToList<docCategory>();
            ViewBag.item_cats = item_cats;

            // get the comments on this item
            List<UserComment> comments = Documentation.GetItemComments(itemID);
            ViewBag.comments = comments;


            // Get all of the categories
            List<docCategory> categories = Documentation.GetCategories();
            ViewBag.categories = categories;

            // get all of the users ::: this will allow us to designate the author of the new item
            List<user> users = Users.GetAllUsers(1);
            ViewBag.users = users;

            // Get all the documents
            List<document> documents = new List<document>();
            documents = (from d in doc_db.documents
                         join id in doc_db.itemDocs on d.docID equals id.docID
                         where id.itemID.Equals(item_id)
                         select d).ToList<document>();
            ViewBag.documents = documents;


            return View();

        }

        /// <summary>
        /// Handles the submission of the form data for editing a documentation item.
        /// </summary>
        /// <param name="item_id">ID of the documentation item.</param>
        /// <param name="itemName">Name of the documentation item.</param>
        /// <returns>Redirects to the documentation items list if successful. Displays the edit page with error messages if save fails.</returns>
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditItem(string item_id, string itemName) {

            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            int itemID = Convert.ToInt32(item_id);

            // Get the item
            docItem item = (from di in doc_db.docItems
                            where di.itemID.Equals(itemID)
                            select di).Single<docItem>();

            // Reassign form fields
            itemName = Request.Form["itemName"].Trim();
            string itemDescription = Request.Form["itemDescription"].Trim().Replace("\r\n","<br />").Replace("\n","<br />").Replace("\r","<br />");
            string executionExample = Request.Form["executionExample"].Trim().Replace("\r\n", "<br />").Replace("\n", "<br />").Replace("\r", "<br />");
            string resultExample = Request.Form["resultExample"].Trim().Replace("\r\n", "<br />").Replace("\n", "<br />").Replace("\r", "<br />");
            string codeLink = Request.Form["codeLink"].Trim();
            string html = Request.Form["html"].Trim();
            int author = Convert.ToInt32(Request.Form["author"].Trim());
            string[] saved_cats = (Request.Form["cat"] != null)?Request.Form["cat"].Trim().Split(','):new string[' '];

            // Validate the submitted info
            List<string> error_messages = new List<string>();
            if (itemName.Length == 0) { error_messages.Add("You must enter a name for this item."); }
            if (itemDescription.Length == 0) { error_messages.Add("You must enter a brief overview of this item."); }
            if (saved_cats.Length == 0) { error_messages.Add("You did not specify a category for this item."); }
            // End Validation

            if (error_messages.Count == 0) { // Save the item

                try {
                    item.itemName = itemName;
                    item.itemDescription = itemDescription;
                    item.executionExample = executionExample;
                    item.resultExample = resultExample;
                    item.codeLink = codeLink;
                    item.author = author;
                    item.itemHTML = html;
                    item.dateModified = DateTime.Now;

                    doc_db.SubmitChanges();

                    // Handle File Upload
                    HttpPostedFileBase hpf = Request.Files[0] as HttpPostedFileBase;
                    if(hpf.ContentLength > 0){
                        string dir = AppDomain.CurrentDomain.BaseDirectory + "/Content/APIDocs/" + itemName;
                        if (!Directory.Exists(dir)) {
                            Directory.CreateDirectory(dir);
                        }
                        string savedFileName = Path.Combine(dir,Path.GetFileName(hpf.FileName));
                        hpf.SaveAs(savedFileName);

                        List<document> existing_docs = new List<document>();
                        existing_docs = (from d in doc_db.documents
                                         where d.documentPath.Equals("/Content/APIDocs/" + itemName + "/" + Path.GetFileName(hpf.FileName))
                                         select d).ToList<document>();
                        foreach (document existing_doc in existing_docs) {
                            itemDoc item_doc = (from item_docs in doc_db.itemDocs
                                                where item_docs.docID.Equals(existing_doc.docID)
                                                select item_docs).FirstOrDefault<itemDoc>();
                            doc_db.itemDocs.DeleteOnSubmit(item_doc);
                        }
                        doc_db.documents.DeleteAllOnSubmit<document>(existing_docs);
                        doc_db.SubmitChanges();

                        document doc = new document {
                            documentPath = "~/Content/APIDocs/"+itemName+"/"+hpf.FileName,
                            documentTitle = Request.Form["documentName"].Trim(),
                            dateAdded = DateTime.Now
                        };
                        doc_db.documents.InsertOnSubmit(doc);
                        doc_db.SubmitChanges();

                        itemDoc id = new itemDoc {
                            itemID = item.itemID,
                            docID = doc.docID
                        };
                        doc_db.itemDocs.InsertOnSubmit(id);
                        doc_db.SubmitChanges();
                    }

                    return RedirectToAction("Items");
                } catch (Exception e) {
                    error_messages.Add(e.Message);
                }
            }
            ViewBag.error_messages = error_messages;


            ViewBag.item = item;

            // Get the cateogries that this item is associated with
            List<docCategory> item_cats = new List<docCategory>();
            item_cats = (from ci in doc_db.cat_items
                         join cats in doc_db.docCategories on ci.catID equals cats.catID
                         where ci.itemID.Equals(item.itemID)
                         select cats).ToList<docCategory>();
            ViewBag.item_cats = item_cats;

            // get the comments on this item
            List<UserComment> comments = Documentation.GetItemComments(item.itemID);
            ViewBag.comments = comments;


            // Get all of the categories
            List<docCategory> categories = Documentation.GetCategories();
            ViewBag.categories = categories;

            // get all of the users ::: this will allow us to designate the author of the new item
            List<user> users = Users.GetAllUsers(1);
            ViewBag.users = users;

            return View();
        }


        /******* AJAX *************/

        /// <summary>
        /// Removes a category from the database and takes all items under this category and moves them to the unknown category.
        /// </summary>
        /// <param name="cat_id"></param>
        /// <returns>Blank string if successful. If error is captured - returns error message.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string RemoveCategory(string cat_id) {
            string error = "";

            try {
                // Convert cat_id into integer and instantiate LINQ object
                int catID = Convert.ToInt32(cat_id);
                DocsLinqDataContext doc_db = new DocsLinqDataContext();

                // Get the category
                docCategory cat = (from c in doc_db.docCategories
                                where c.catID.Equals(catID)
                                   select c).FirstOrDefault<docCategory>();
                doc_db.docCategories.DeleteOnSubmit(cat);

                // Get the items under this category
                List<cat_item> items = (from ci in doc_db.cat_items
                                        where ci.catID.Equals(catID)
                                        select ci).ToList<cat_item>();

                foreach (cat_item item in items) { // Loop through the items and reset their category
                    item.catID = 1;
                }

                doc_db.SubmitChanges(); // Save changes
            } catch (Exception e) {
                error = e.Message;
            }

            return error;
        }

        /// <summary>
        /// Remove a documentation item.
        /// </summary>
        /// <param name="item_id">ID of the item to be removed.</param>
        /// <returns>Blank string if successful. Error message if we encounter an error.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string RemoveItem(string item_id) {
            int itemID = Convert.ToInt32(item_id);
            string error = "";
            if (itemID != 0) {
                error = Documentation.RemoveItem(itemID);
            }
            return error;
        }

        /// <summary>
        /// Removes the association with the given category.
        /// </summary>
        /// <param name="cat_id">ID of the category to be removed from.</param>
        /// <param name="item_id">ID of the item to act on.</param>
        /// <returns>Blank string if successful. Error message if we encounter an error.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteItemCategory(int cat_id, int item_id) {
            string error = "";
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            if (cat_id > 0 && item_id > 0) {
                try{
                    cat_item cat = new cat_item();
                    cat = (from ci in doc_db.cat_items
                           where ci.catID.Equals(cat_id) && ci.itemID.Equals(item_id)
                           select ci).Single<cat_item>();
                    doc_db.cat_items.DeleteOnSubmit(cat);
                    doc_db.SubmitChanges();

                }catch(Exception e){
                    error = e.Message;
                }
            } else {
                error = "Error: Unsatisfied data.";
            }

            return error;
        }

        /// <summary>
        /// Adds an association to a category for a given documentation item.
        /// </summary>
        /// <param name="cat_id">ID of the category.</param>
        /// <param name="item_id">ID of the item.</param>
        /// <returns>Blank string if successful. Error message if we encounter an error.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string AddItemCategory(int cat_id, int item_id) {
            string error = "";
            if (cat_id > 0 && item_id > 0) {
                try {
                    DocsLinqDataContext doc_db = new DocsLinqDataContext();
                    cat_item newCatItem = new cat_item {
                        catID = cat_id,
                        itemID = item_id
                    };
                    doc_db.cat_items.InsertOnSubmit(newCatItem);
                    doc_db.SubmitChanges();
                } catch (Exception e) {
                    error = e.Message;
                }

            } else {
                error = "Error: Unsatisfied data.";
            }
            return error;
        }


        /// <summary>
        /// Delete Document from system
        /// </summary>
        /// <param name="docID">ID of document</param>
        /// <returns>string</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public string DeleteDocument(int docID = 0) {
            try {
                Documentation.DeleteDocument(docID);
                return "";
            } catch (Exception e) {
                return e.Message;
            }
        }
    }
}

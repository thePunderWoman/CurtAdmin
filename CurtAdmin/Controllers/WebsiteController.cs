using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Web.Script.Serialization;

namespace CurtAdmin.Controllers {
    public class WebsiteController : BaseController {
        //
        // GET: /Website/
        protected override void OnActionExecuting(ActionExecutingContext filterContext) {
            ViewBag.activeModule = "Website Management";
            List<Website> websites = new Website().GetAll();
            ViewBag.websites = websites;
            HttpCookie websiteCookie = new HttpCookie("");
            websiteCookie = Request.Cookies.Get("websiteID");
            int websiteID = 0;
            try {
                websiteID = Convert.ToInt32(websiteCookie.Value);
            } catch { }
            ViewBag.websiteID = websiteID;
        }

        public ActionResult Index() {
            return View();
        }

        public ActionResult ChooseWebsite(int id = 0) {
            if (id > 0) {
                HttpCookie websiteID = new HttpCookie("websiteID");
                websiteID.Value = id.ToString();
                websiteID.Expires = DateTime.Now.AddYears(2);
                Response.Cookies.Add(websiteID);
            }
            return RedirectToAction("Contents");
        }

        public ActionResult Contents(int id = 0) {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index");
            }
            CurtDevDataContext db = new CurtDevDataContext();
            List<SiteContent> contents = SiteContentModel.GetAll(websiteID);
            ViewBag.contents = contents;
            if (id == 0) {
                menuWithContent menu = MenuModel.GetPrimary(websiteID);
                ViewBag.menu = menu;
            } else {
                menuWithContent menu = MenuModel.GetMenu(id, websiteID);
                if (menu == null || menu.menuID == 0) {
                    return RedirectToRoute("Contents");
                }
                ViewBag.menu = menu;
            }

            return View();
        }

        public ActionResult Menus() {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index");
            }

            List<Menu> menus = new List<Menu>();
            CurtDevDataContext db = new CurtDevDataContext();
            menus = (db.Menus.Where(x => x.active == true && x.websiteID.Equals(websiteID)).OrderBy(x => x.isPrimary).ThenBy(x => x.showOnSitemap).ThenBy(x => x.sort).ToList<Menu>());
            ViewBag.menus = menus;

            return View();
        }

        public ActionResult SetPrimaryMenu(int id = 0) {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index");
            }
            MenuModel.SetPrimary(id, websiteID);
            return RedirectToAction("Menus");
        }

        public ActionResult AddMenu() {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index");
            }
            string error = "";
            CurtDevDataContext db = new CurtDevDataContext();

            #region Form Submission
            try {
                if (Request.Form["btnSave"] != null) {


                    // Save form values
                    string name = (Request.Form["menu_name"] != null) ? Request.Form["menu_name"] : "";
                    string displayname = (Request.Form["display_name"] != null) ? Request.Form["display_name"] : "";
                    bool requireAuthentication = (Request.Form["requireAuthentication"] != null) ? true : false;
                    bool showOnSitemap = (Request.Form["showOnSitemap"] != null) ? true : false;

                    // Validate the form fields
                    if (name.Length == 0) throw new Exception("Name is required.");

                    // Create the new customer and save
                    Menu new_menu = new Menu {
                        menu_name = name,
                        display_name = displayname,
                        requireAuthentication = requireAuthentication,
                        showOnSitemap = showOnSitemap,
                        sort = ((showOnSitemap) ? (db.Menus.Where(x => x.showOnSitemap == true).OrderByDescending(x => x.sort).Select(x => x.sort).FirstOrDefault() + 1) : 1),
                        active = true,
                        websiteID = websiteID
                    };

                    db.Menus.InsertOnSubmit(new_menu);
                    db.SubmitChanges();
                    return RedirectToAction("Menus");
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            ViewBag.error = error;

            return View();
        }

        public ActionResult EditMenu(int id = 0) {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index");
            }
            string error = "";
            string message = "";
            CurtDevDataContext db = new CurtDevDataContext();

            Menu menu = db.Menus.Where(x => x.menuID == id && x.websiteID.Equals(websiteID)).FirstOrDefault<Menu>();
            if (menu == null) {
                return RedirectToAction("Menus");
            }
            #region Form Submission
            try {
                if (Request.Form["btnSave"] != null) {


                    // Save form values
                    string name = (Request.Form["menu_name"] != null) ? Request.Form["menu_name"] : "";
                    string displayname = (Request.Form["display_name"] != null) ? Request.Form["display_name"] : "";
                    bool requireAuthentication = (Request.Form["requireAuthentication"] != null) ? true : false;
                    bool showOnSitemap = (Request.Form["showOnSitemap"] != null) ? true : false;

                    // Validate the form fields
                    if (name.Length == 0) throw new Exception("Name is required.");

                    // Create the new customer and save
                    menu.menu_name = name;
                    menu.display_name = displayname;
                    menu.requireAuthentication = requireAuthentication;
                    menu.showOnSitemap = showOnSitemap;
                    menu.sort = ((showOnSitemap) ? (db.Menus.Where(x => x.showOnSitemap == true).OrderByDescending(x => x.sort).Select(x => x.sort).FirstOrDefault() + 1) : 1);

                    db.SubmitChanges();
                    message = "Changes saved successfully";
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            ViewBag.menu = menu;
            ViewBag.error = error;
            ViewBag.message = message;

            return View();
        }

        public ActionResult AddLink(int id = 0) {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index");
            }
            string error = "";
            CurtDevDataContext db = new CurtDevDataContext();
            Menu m = db.Menus.Where(x => x.menuID.Equals(id) && x.websiteID.Equals(websiteID)).FirstOrDefault<Menu>();
            if (m == null || m.menuID == 0) {
                return RedirectToAction("Menus");
            }

            #region Form Submission
            try {
                if (Request.Form["btnSave"] != null) {


                    // Save form values
                    string name = (Request.Form["link_name"] != null) ? Request.Form["link_name"] : "";
                    string value = (Request.Form["link_value"] != null) ? Request.Form["link_value"] : "";
                    bool linkTarget = (Request.Form["link_target"] == null) ? false : true;

                    // Validate the form fields
                    if (name.Length == 0) throw new Exception("Link name is required.");
                    if (value.Length == 0) throw new Exception("Link value is required.");

                    // Create the new customer and save
                    Menu_SiteContent new_item = new Menu_SiteContent {
                        menuID = m.menuID,
                        menuTitle = name,
                        menuLink = value,
                        menuSort = (db.Menu_SiteContents.Where(x => x.menuID == m.menuID).Where(x => x.parentID == null).Count()) + 1,
                        linkTarget = linkTarget
                    };

                    db.Menu_SiteContents.InsertOnSubmit(new_item);
                    db.SubmitChanges();
                    return RedirectToRoute("ContentMenu", new { id = id });
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            ViewBag.error = error;
            ViewBag.menuID = id;

            return View();
        }

        public ActionResult EditLink(int id = 0) {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index");
            }
            string error = "";
            string message = "";
            CurtDevDataContext db = new CurtDevDataContext();

            Menu_SiteContent item = db.Menu_SiteContents.Where(x => x.menuContentID == id && x.Menu.websiteID.Equals(websiteID)).FirstOrDefault<Menu_SiteContent>();
            if (item == null || item.menuContentID == 0) {
                return RedirectToAction("Menus");
            }
            #region Form Submission
            try {
                if (Request.Form["btnSave"] != null) {


                    // Save form values
                    string name = (Request.Form["link_name"] != null) ? Request.Form["link_name"] : "";
                    string value = (Request.Form["link_value"] != null) ? Request.Form["link_value"] : "";
                    bool linkTarget = (Request.Form["link_target"] == null) ? false : true;

                    // Validate the form fields
                    if (name.Length == 0) throw new Exception("Link name is required.");
                    if (value.Length == 0) throw new Exception("Link value is required.");

                    // Create the new customer and save
                    item.menuTitle = name;
                    item.menuLink = value;
                    item.linkTarget = linkTarget;

                    db.SubmitChanges();
                    message = "Changes saved successfully";
                }
            } catch (Exception e) {
                error = e.Message;
            }
            #endregion

            ViewBag.item = item;
            ViewBag.error = error;
            ViewBag.message = message;

            return View();
        }

        public ActionResult SetPrimaryContent(int id = 0, int menuid = 0) {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index");
            }
            SiteContentModel.SetPrimary(id, websiteID);
            return RedirectToRoute("ContentMenu", new { id = menuid });
        }

        [ValidateInput(false)]
        public ActionResult AddContent(int id = 0) {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index");
            }
            string error = "";
            CurtDevDataContext db = new CurtDevDataContext();
            Menu m = db.Menus.Where(x => x.menuID.Equals(id) && x.websiteID.Equals(websiteID)).FirstOrDefault<Menu>();
            if (m == null || m.menuID == 0) {
                return RedirectToAction("Contents");
            }

            #region Form Submission
            if (Request.Form.Count > 0) {
                try {

                    // Save form values
                    string title = (Request.Form["page_title"] != null) ? Request.Form["page_title"] : "";
                    bool addtomenu = (Request.Form["addtomenu"] != null) ? true : false;
                    bool requireAuthentication = (Request.Form["requireAuthentication"] != null) ? true : false;
                    // Validate the form fields
                    if (title.Length == 0) throw new Exception("Page Title is required.");

                    // Create the new customer and save
                    SiteContent new_page = new SiteContent {
                        page_title = title,
                        keywords = Request.Form["keywords"],
                        meta_description = Request.Form["meta_description"],
                        meta_title = Request.Form["meta_title"],
                        canonical = (Request.Form["canonical"].Trim() != "") ? Request.Form["canonical"].Trim() : null,
                        published = Convert.ToBoolean(Request.Form["publish"]),
                        createdDate = DateTime.Now,
                        lastModified = DateTime.Now,
                        active = true,
                        isPrimary = false,
                        slug = UDF.GenerateSlug(title),
                        requireAuthentication = requireAuthentication,
                        websiteID = websiteID
                    };

                    db.SiteContents.InsertOnSubmit(new_page);
                    db.SubmitChanges();

                    SiteContentRevision revision = new SiteContentRevision {
                        content_text = Request.Form["page_content"],
                        contentID = new_page.contentID,
                        active = true,
                        createdOn = DateTime.Now
                    };

                    if (addtomenu) {
                        Menu_SiteContent menuitem = new Menu_SiteContent {
                            menuID = m.menuID,
                            contentID = new_page.contentID,
                            menuSort = (db.Menu_SiteContents.Where(x => x.menuID == m.menuID).Where(x => x.parentID == null).Count()) + 1,
                            linkTarget = false
                        };
                        db.Menu_SiteContents.InsertOnSubmit(menuitem);
                    }

                    db.SiteContentRevisions.InsertOnSubmit(revision);
                    db.SubmitChanges();

                    return RedirectToRoute("ContentMenu", new { id = id });
                } catch (Exception e) {
                    error = e.Message;
                }
            }
            #endregion

            ViewBag.error = error;
            ViewBag.menuID = id;

            return View();
        }

        [ValidateInput(false)]
        public ActionResult EditContent(int id = 0, int revisionID = 0) {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index");
            }
            string error = "";
            CurtDevDataContext db = new CurtDevDataContext();

            SiteContent content = db.SiteContents.Where(x => x.contentID == id && x.websiteID.Equals(websiteID)).FirstOrDefault<SiteContent>();
            if (content == null || content.contentID == 0) {
                return RedirectToAction("Contents");
            }

            #region Form Submission
            if (Request.Form.Count > 0) {
                try {

                    // Save form values
                    string title = (Request.Form["page_title"] != null) ? Request.Form["page_title"] : "";
                    bool requireAuthentication = (Request.Form["requireAuthentication"] != null) ? true : false;

                    // Validate the form fields
                    if (title.Length == 0) throw new Exception("Page Title is required.");

                    // Create the new customer and save
                    content.page_title = title;
                    content.keywords = Request.Form["keywords"];
                    content.meta_description = Request.Form["meta_description"];
                    content.meta_title = Request.Form["meta_title"];
                    content.canonical = (Request.Form["canonical"].Trim() != "") ? Request.Form["canonical"].Trim() : null;
                    content.slug = UDF.GenerateSlug(title);
                    if (Request.Form["publish"] == null) {
                        content.published = false;
                    } else {
                        content.published = Convert.ToBoolean(Request.Form["publish"]);
                    }
                    content.lastModified = DateTime.Now;
                    content.requireAuthentication = requireAuthentication;
                    SiteContentRevision revision = new SiteContentRevision();
                    try {
                        revision = db.SiteContentRevisions.Where(x => x.contentID == content.contentID)
                                                    .Where(x => x.revisionID == revisionID).First<SiteContentRevision>();
                    } catch {
                        revision = db.SiteContentRevisions.Where(x => x.contentID == content.contentID)
                                                    .Where(x => x.active == true).First<SiteContentRevision>();
                    }
                    revision.content_text = Request.Form["page_content"];
                    db.SubmitChanges();
                    ViewBag.message = "Content Page Updated Successfully!";
                } catch (Exception e) {
                    error = e.Message;
                }
            }
            #endregion

            ViewBag.error = error;
            ViewBag.content = SiteContentModel.GetPage(id); ;
            ViewBag.revisionID = revisionID;
            return View();
        }

        public ActionResult CopyRevision(int id = 0) {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index");
            }
            // Remove content page from menu
            int contentid = SiteContentModel.CopyRevision(id);
            return RedirectToRoute("ContentEdit", new { id = contentid });
        }

        public ActionResult ActivateRevision(int id = 0) {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index");
            }
            // Remove content page from menu
            int contentid = SiteContentModel.ActivateRevision(id);
            return RedirectToRoute("ContentEdit", new { id = contentid });
        }

        public ActionResult DeleteRevision(int id = 0) {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index");
            }
            // Remove content page from menu
            int contentid = SiteContentModel.DeleteRevision(id);
            return RedirectToRoute("ContentEdit", new { id = contentid });
        }

        public ActionResult RemoveContent(int id = 0) {
            int websiteID = ViewBag.websiteID ?? 0;
            if (websiteID == 0) {
                return RedirectToAction("Index");
            }
            // Remove content page from menu
            int menu = MenuModel.RemoveContent(id);
            return RedirectToRoute("ContentMenu", new { id = menu });
        }

        public string RemoveContentAjax(int id = 0) {
            // Remove content page from menu
            int menu = MenuModel.RemoveContent(id);
            return "";
        }

        public string DeleteContent(int id = 0) {
            // Permanently Delete content page
            if (!SiteContentModel.Remove(id)) {
                return "error";
            }
            return "success";
        }

        public string AddContentToMenu() {
            // Add content page to menu
            JavaScriptSerializer js = new JavaScriptSerializer();
            int websiteID = ViewBag.websiteID;
            int menuid = Convert.ToInt16(Request.QueryString["menuid"]);
            int pageid = Convert.ToInt16(Request.QueryString["contentid"]);
            Menu_SiteContent menupage = MenuModel.AddContent(menuid, pageid, websiteID);
            var menustuff = new {
                menuContentID = menupage.menuContentID,
                menuSort = menupage.menuSort,
                parentID = (menupage.parentID == null) ? 0 : menupage.parentID,
                contentID = menupage.contentID,
                pagetitle = menupage.SiteContent.page_title,
                published = menupage.SiteContent.published
            };
            return js.Serialize(menustuff);
        }

        public string CheckContent(int id = 0) {
            // Add content page to menu
            CurtDevDataContext db = new CurtDevDataContext();
            JavaScriptSerializer js = new JavaScriptSerializer();
            var menus = (from m in db.Menus
                         join mc in db.Menu_SiteContents on m.menuID equals mc.menuID
                         where mc.contentID.Equals(id)
                         select new {
                             menuName = m.menu_name
                         }).ToList();
            return js.Serialize(menus);
        }

        public string MenuSort(int id = 0) {
            MenuModel.Sort(id, Request.QueryString["page[]"]);
            return "";
        }

        public string RemoveMenu(int menuID = 0) {
            // Attempt to remove menu
            if (!MenuModel.Remove(menuID)) {
                return "Error";
            }
            return "";
        }
    }
}

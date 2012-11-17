using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace CurtAdmin.Models {
    public class MenuModel {

        public static Menu Get(int id = 0) {
            Menu menu = new Menu();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                menu = db.Menus.Where(x => x.menuID == id).First<Menu>();
            } catch {}
            return menu;
        }

        public static menuWithContent GetPrimary(int websiteID = 0) {
            menuWithContent menu = new menuWithContent();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                menu = (from m in db.Menus
                        where m.isPrimary == true && m.websiteID.Equals(websiteID)
                        select new menuWithContent {
                            menuID = m.menuID,
                            menu_name = m.menu_name,
                            isPrimary = m.isPrimary,
                            active = m.active
                        }).First<menuWithContent>();
                List<menuItem> contents = (from msc in db.Menu_SiteContents
                                           where msc.menuID.Equals(menu.menuID)
                                           orderby msc.parentID, msc.menuSort
                                           select new menuItem {
                                               menuContentID = msc.menuContentID,
                                               menuID = msc.menuID,
                                               menuSort = msc.menuSort,
                                               menuTitle = msc.menuTitle,
                                               menuLink = msc.menuLink,
                                               parentID = msc.parentID,
                                               contentID = msc.contentID,
                                               content = (from sc in db.SiteContents
                                                          where sc.contentID.Equals(msc.contentID)
                                                          select new ContentPage {
                                                              contentID = sc.contentID,
                                                              page_title = sc.page_title,
                                                              content_type = sc.content_type,
                                                              lastModified = sc.lastModified,
                                                              createdDate = sc.createdDate,
                                                              published = sc.published,
                                                              meta_title = sc.meta_title,
                                                              meta_description = sc.meta_description,
                                                              keywords = sc.keywords,
                                                              active = sc.active,
                                                              isPrimary = sc.isPrimary,
                                                              revisions = (db.SiteContentRevisions.Where(x => x.contentID == sc.contentID).ToList<SiteContentRevision>())
                                                          }).FirstOrDefault<ContentPage>()
                                           }).ToList<menuItem>();
                menu.contents = contents.ToLookup(k => (k.parentID == null) ? 0 : k.parentID);
                return menu;
            } catch { return menu; }
        }

        public static menuWithContent GetMenu(int id = 0, int websiteID = 0) {
            menuWithContent menu = new menuWithContent();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                menu = (from m in db.Menus
                        where m.menuID == id && m.websiteID.Equals(websiteID)
                        select new menuWithContent {
                            menuID = m.menuID,
                            menu_name = m.menu_name,
                            isPrimary = m.isPrimary,
                            active = m.active
                        }).First<menuWithContent>();

                List<menuItem> contents = (from msc in db.Menu_SiteContents
                                              where msc.menuID.Equals(menu.menuID)
                                              orderby msc.parentID, msc.menuSort
                                              select new menuItem {
                                                  menuContentID = msc.menuContentID,
                                                  menuID = msc.menuID,
                                                  menuSort= msc.menuSort,
                                                  menuTitle = msc.menuTitle,
                                                  menuLink = msc.menuLink,
                                                  parentID = msc.parentID,
                                                  contentID = msc.contentID,
                                                  content = (from sc in db.SiteContents
                                                                 where sc.contentID.Equals(msc.contentID)
                                                                 select new ContentPage {
                                                                     contentID = sc.contentID,
                                                                     page_title = sc.page_title,
                                                                     content_type = sc.content_type,
                                                                     lastModified = sc.lastModified,
                                                                     createdDate = sc.createdDate,
                                                                     published = sc.published,
                                                                     meta_title = sc.meta_title,
                                                                     meta_description = sc.meta_description,
                                                                     keywords = sc.keywords,
                                                                     active = sc.active,
                                                                     isPrimary = sc.isPrimary,
                                                                     revisions = (db.SiteContentRevisions.Where(x => x.contentID == sc.contentID).ToList<SiteContentRevision>())
                                                                 }).FirstOrDefault<ContentPage>()
                                                }).ToList<menuItem>();
                menu.contents = contents.ToLookup(k => (k.parentID == null) ? 0 : k.parentID);
                return menu;
            } catch { return menu; }
        }
        
        public static Menu SetPrimary(int id = 0, int websiteID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            Menu menu = db.Menus.Where(x => x.menuID == id).FirstOrDefault<Menu>();
            if (menu != null) {
                if (menu.isPrimary == true) {
                    menu.isPrimary = false;
                    db.SubmitChanges();
                } else {
                    Menu primarymenu = db.Menus.Where(x => x.isPrimary == true && x.websiteID.Equals(websiteID)).FirstOrDefault<Menu>();
                    if (primarymenu != null) {
                        primarymenu.isPrimary = false;
                    }
                    menu.isPrimary = true;
                    db.SubmitChanges();
                }
            }
            return menu;
        }

        public static void Sort(int menuid = 0, string pages = "") {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                List<string> pagedata = pages.Split(',').ToList<string>();
                foreach (string page in pagedata) {
                    string[] pageinfo = page.Split('-');
                    Menu_SiteContent item = db.Menu_SiteContents.Where(x => x.menuContentID == Convert.ToInt16(pageinfo[0])).First();
                    item.menuSort = Convert.ToInt32(pageinfo[2]);
                    if (pageinfo[1] == "0") {
                        item.parentID = null;
                    } else {
                        item.parentID = Convert.ToInt32(pageinfo[1]);
                    }
                    db.SubmitChanges();
                }
            } catch {}
        }

        public static Boolean Remove(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                Menu menu = db.Menus.Where(x => x.menuID == id).First<Menu>();
                db.Menus.DeleteOnSubmit(menu);
                db.SubmitChanges();
                return true;
            } catch { return false; }
        }

        public static Menu_SiteContent AddContent(int menuid, int contentid, int websiteID) {
            CurtDevDataContext db = new CurtDevDataContext();
            Menu_SiteContent menuitem = new Menu_SiteContent {
                menuID = menuid,
                contentID = contentid,
                menuSort = (db.Menu_SiteContents.Where(x => x.menuID == menuid).Where(x => x.parentID == null).Count()) + 1
            };

            db.Menu_SiteContents.InsertOnSubmit(menuitem);
            db.SubmitChanges();
            return menuitem;
        }

        public static int RemoveContent(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                // get the item to be deleted
                Menu_SiteContent menuitem = db.Menu_SiteContents.Where(x => x.menuContentID == id).First<Menu_SiteContent>();

                // get the menu id of the current menu
                int menu = menuitem.menuID;

                // get all the items in the menu that are on the same level of the item to be deleted
                List<Menu_SiteContent> menuitems = db.Menu_SiteContents.Where(x => x.parentID == menuitem.parentID).OrderBy(x => x.menuSort).ToList<Menu_SiteContent>();

                // get all the items in the menu that are the children of the item to be deleted
                List<Menu_SiteContent> childitems = db.Menu_SiteContents.Where(x => x.parentID == id).OrderBy(x => x.menuSort).ToList<Menu_SiteContent>();

                int sort = 0;
                // loop through the menu items on the same level an adjust the sort of each
                foreach (Menu_SiteContent item in menuitems) {
                    sort++;
                    if (item.menuContentID == menuitem.menuContentID) {
                        // skip this item, but set any children that it may have to be the next level up and set the sort
                        foreach (Menu_SiteContent childitem in childitems) {
                            sort++;
                            childitem.parentID = menuitem.parentID;
                            childitem.menuSort = sort;
                        }
                    } else {
                        // adjust the sort of the item
                        item.menuSort = sort;
                    }
                }
                db.Menu_SiteContents.DeleteOnSubmit(menuitem);
                // find children and move them up a level
                db.SubmitChanges();
                return menu;
            } catch { return 0; }
        }

    }

    public class menuWithContent : Menu {
        public ILookup<int?, menuItem> contents { get; set; }

        public bool hasChildren(int parentID = 0) {
            if (this.contents.Contains(parentID)) {
                return true;
            }
            return false;
        }
        public int getChildrenCount(int parentID = 0) {
            if (this.contents.Contains(parentID)) {
                return this.contents[parentID].Count();
            }
            return 0;
        }
        public string getChildrenIDs(int parentID = 0) {
            if (this.contents.Contains(parentID)) {
                string childlist = "";
                List<menuItem> children = getChildren(parentID);
                for (int i = 0; i < children.Count(); i++) {
                    if (i != 0) {
                        childlist += ",";
                    }
                    childlist += children[i].menuContentID;
                }
                return childlist;
            }
            return "";
        }
        public List<menuItem> getChildren(int parentID = 0) {
            return this.contents[parentID].ToList<menuItem>();
        }
    }

    public class menuItem : Menu_SiteContent {
        public ContentPage content { get; set; }

        public bool hasContent() {
            if (this.content != null) {
                return true;
            }
            return false;
        }
    }
}
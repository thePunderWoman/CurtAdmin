using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CurtAdmin.Models;
using System.Text.RegularExpressions;

namespace CurtAdmin.Models {
    public class UDF {
        public static string writeContentTree(menuWithContent menu, int parentid, int level) {
            string pagecontent = "";
            string editlink = "";
            string primarylink = "";
            level++;
            List<menuItem> menuitems = menu.getChildren(parentid);
            foreach (menuItem menuitem in menuitems) {
                editlink = "<a href=\"/Website/Link/Edit/" + menuitem.menuContentID + "\"><img src=\"/Content/img/pencil.png\" alt=\"Edit Link\" title=\"Edit Link\" /></a>";
                primarylink = "";
                if (menuitem.hasContent() && menuitem.content.isPrimary) {
                    primarylink = "<a href=\"/Website/SetPrimaryContent/" + menuitem.contentID + "/" + menu.menuID + "\"><img src=\"/Content/img/check.png\" alt=\"Primary Page\" title=\"Primary Page\" /></a> ";
                } else if (menuitem.hasContent()) {
                    primarylink = "<a href=\"/Website/SetPrimaryContent/" + menuitem.contentID + "/" + menu.menuID + "\"><img src=\"/Content/img/makeprimary.png\" alt=\"Make This Page the Primary Page\" title=\"Make This Page the Primary Page\" /></a> ";
                    editlink = "<a href=\"/Website/Content/Edit/" + menuitem.contentID + "\"><img src=\"/Content/img/pencil.png\" alt=\"Edit Page\" title=\"Edit Page\" /></a>";
                };
                pagecontent += "<li id=\"item_" + menuitem.menuContentID + "\" class=\"level_" + level + (((menuitem.hasContent() && menuitem.content.published) || (!menuitem.hasContent())) ? " published" : "") + "\">" +
                    "<span class=\"handle\">↕</span> <span class=\"title\">" + ((menuitem.hasContent()) ? menuitem.content.page_title : (menuitem.menuTitle + " (link)")) + "</span>" +
                    "<span class=\"controls\">" +
                        primarylink +
                        editlink +
                        " <a href=\"/Website/RemoveContent/" + menuitem.menuContentID + "\" class=\"remove\" id=\"remove_" + menuitem.menuContentID + "\"><img src=\"/Content/img/delete.png\" alt=\"Remove Page From Menu\" title=\"Remove Page From Menu\" /></a>" +
                    "</span>" +
                    "<span id=\"meta_" + menuitem.menuContentID + "\">" +
                        "<input type=\"hidden\" id=\"parent_" + menuitem.menuContentID + "\" value=\"" + ((menuitem.parentID == null) ? 0 : menuitem.parentID) + "\" />" +
                        "<input type=\"hidden\" id=\"children_" + menuitem.menuContentID + "\" value=\"" + menu.getChildrenIDs(menuitem.menuContentID) + "\" />" +
                        "<input type=\"hidden\" id=\"count_" + menuitem.menuContentID + "\" value=\"" + menu.getChildrenCount(menuitem.menuContentID) + "\" />" +
                        "<input type=\"hidden\" id=\"sort_" + menuitem.menuContentID + "\" value=\"" + menuitem.menuSort + "\" />" +
                        "<input type=\"hidden\" id=\"depth_" + menuitem.menuContentID + "\" value=\"" + level + "\" />" +
                    "</span>" +
                    "<ul id=\"transport_" + menuitem.menuContentID + "\"></ul>" +
                    "</li>";
                if (menu.hasChildren(menuitem.menuContentID)) {
                    pagecontent += writeContentTree(menu, menuitem.menuContentID, level);
                }
            }
            return pagecontent;
        }

        public static string GenerateSlug(string phrase = "") {
            string str = RemoveAccent(phrase).ToLower();

            str = Regex.Replace(str, @"[^a-z0-9\s-]", ""); // invalid chars           
            str = Regex.Replace(str, @"\s+", " ").Trim(); // convert multiple spaces into one space   
            str = str.Trim(); // cut and trim it   
            str = Regex.Replace(str, @"\s", "_"); // underscores

            return str;
        }

        public static string RemoveAccent(string txt = "") {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }
    }

}
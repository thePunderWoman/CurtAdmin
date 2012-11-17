using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace CurtAdmin.Models {
    public class SiteContentModel {

        public static SiteContent Get(int id = 0) {
            SiteContent content = new SiteContent();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                content = db.SiteContents.Where(x => x.contentID == id).First<SiteContent>();
            } catch {}
            return content;
        }

        public static List<SiteContent> GetAll(int websiteID = 0) {
            List<SiteContent> contents = new List<SiteContent>();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                contents = db.SiteContents.Where(x => x.active == true && x.websiteID.Equals(websiteID)).OrderBy(x => x.page_title).ToList<SiteContent>();
            } catch { }
            return contents;
        }

        public static ContentPage GetPage(int id = 0) {
            ContentPage content = new ContentPage();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                content = (from s in db.SiteContents
                           where s.contentID == id
                           select new ContentPage {
                               contentID = s.contentID,
                               page_title = s.page_title,
                               content_type = s.content_type,
                               lastModified = s.lastModified,
                               createdDate = s.createdDate,
                               published = s.published,
                               meta_title = s.meta_title,
                               meta_description = s.meta_description,
                               keywords = s.keywords,
                               canonical = s.canonical,
                               active = s.active,
                               isPrimary = s.isPrimary,
                               slug = s.slug,
                               requireAuthentication = s.requireAuthentication,
                               revisions = (db.SiteContentRevisions.Where(x => x.contentID == s.contentID).ToList<SiteContentRevision>())
                           }).First<ContentPage>();
                return content;
            } catch { return content; }
        }

        public static ContentPage GetPrimary() {
            ContentPage content = new ContentPage();
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                content = (from s in db.SiteContents
                        where s.isPrimary == true
                        select new ContentPage {
                            contentID = s.contentID,
                            page_title = s.page_title,
                            content_type = s.content_type,
                            lastModified = s.lastModified,
                            createdDate = s.createdDate,
                            published = s.published,
                            meta_title = s.meta_title,
                            meta_description = s.meta_description,
                            keywords = s.keywords,
                            canonical = s.canonical,
                            active = s.active,
                            isPrimary = s.isPrimary,
                            slug = s.slug,
                            requireAuthentication = s.requireAuthentication,
                            revisions = (db.SiteContentRevisions.Where(x => x.contentID == s.contentID).ToList<SiteContentRevision>())
                        }).First<ContentPage>();
                return content;
            } catch { return content; }
        }

        public static SiteContent SetPrimary(int id = 0, int websiteID = 0) {
            CurtDevDataContext db = new CurtDevDataContext();
            SiteContent content = db.SiteContents.Where(x => x.contentID == id).FirstOrDefault<SiteContent>();
            if (content != null) {
                if (content.isPrimary == true) {
                    content.isPrimary = false;
                    db.SubmitChanges();
                } else {
                    SiteContent primarypage = db.SiteContents.Where(x => x.isPrimary == true && x.websiteID.Equals(websiteID)).FirstOrDefault<SiteContent>();
                    if (primarypage != null) {
                        primarypage.isPrimary = false;
                    }
                    content.isPrimary = true;
                    db.SubmitChanges();
                }
            }
            return content;
        }

        public static Boolean Remove(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                SiteContent content = db.SiteContents.Where(x => x.contentID == id).First<SiteContent>();
                content.active = false;
                db.SubmitChanges();
                return true;
            } catch { return false; }
        }

        public static int CopyRevision(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                SiteContentRevision revision = db.SiteContentRevisions.Where(x => x.revisionID == id).First<SiteContentRevision>();
                SiteContentRevision revcopy = new SiteContentRevision {
                    active = false,
                    createdOn = DateTime.Now,
                    content_text = revision.content_text,
                    contentID = revision.contentID
                };
                db.SiteContentRevisions.InsertOnSubmit(revcopy);
                db.SubmitChanges();
                return revcopy.contentID;
            } catch { return 0; }
        }

        public static int ActivateRevision(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                int contentID = db.SiteContentRevisions.Where(x => x.revisionID == id).Select(x => x.contentID).First();
                List<SiteContentRevision> revisions = db.SiteContentRevisions.Where(x => x.contentID.Equals(contentID)).ToList<SiteContentRevision>();
                foreach (SiteContentRevision r in revisions) {
                    if (r.revisionID != id) {
                        r.active = false;
                    } else {
                        r.active = true;
                    }
                }
                db.SubmitChanges();
                return contentID;
            } catch { return 0; }
        }

        public static int DeleteRevision(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                SiteContentRevision revision = db.SiteContentRevisions.Where(x => x.revisionID == id).First<SiteContentRevision>();
                int contentID = revision.contentID;
                if (!(bool)revision.active) {
                    db.SiteContentRevisions.DeleteOnSubmit(revision);
                    db.SubmitChanges();
                }
                return contentID;
            } catch { return 0; }
        }
    }

    public class ContentPage : SiteContent {
        public List<SiteContentRevision> revisions { get; set; }
    }
}
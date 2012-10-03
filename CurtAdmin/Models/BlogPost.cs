using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CurtAdmin.Models {
    public class BlogPostModel {
        public static List<PostWithCategories> GetAll() {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                List<PostWithCategories> posts = new List<PostWithCategories>();

                posts = (from p in db.BlogPosts
                        where p.active.Equals(true)
                        orderby p.publishedDate, p.createdDate
                        select new PostWithCategories {
                            blogPostID = p.blogPostID,
                            post_title = p.post_title,
                            slug = p.slug,
                            userID = p.userID,
                            post_text = p.post_text,
                            publishedDate = p.publishedDate,
                            createdDate = p.createdDate,
                            lastModified = p.lastModified,
                            meta_title = p.meta_title,
                            meta_description = p.meta_description,
                            active = p.active,
                            author = GetAuthor(p.userID),
                            categories = (from c in db.BlogCategories join pc in db.BlogPost_BlogCategories on c.blogCategoryID equals pc.blogCategoryID where pc.blogPostID.Equals(p.blogPostID) select c).ToList<BlogCategory>(),
                            comments = (from cm in db.Comments where cm.blogPostID.Equals(p.blogPostID) && cm.approved.Equals(true) && cm.active.Equals(true) select cm).ToList<Comment>(),
                            mod_comments = (from cm in db.Comments where cm.blogPostID.Equals(p.blogPostID) && cm.approved.Equals(false) && cm.active.Equals(true) select cm).ToList<Comment>()
                        }).ToList<PostWithCategories>();

                return posts;
            } catch (Exception e) {
                return new List<PostWithCategories>();
            }
        }

        public static List<BlogPost> GetAllPublished() {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                List<BlogPost> posts = new List<BlogPost>();

                posts = db.BlogPosts.Where(x => x.active == true).Where(x => x.publishedDate != null).OrderBy(x => x.publishedDate).ToList<BlogPost>();

                return posts;
            } catch (Exception e) {
                return new List<BlogPost>();
            }
        }

        public static PostWithCategories Get(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                PostWithCategories post = new PostWithCategories();
                //post = db.Posts.Where(x => x.postID == id).Where(x => x.active == true).FirstOrDefault<Post>();
                post = (from p in db.BlogPosts
                         where p.blogPostID.Equals(id)
                         select new PostWithCategories {
                             blogPostID = p.blogPostID,
                             post_title = p.post_title,
                             slug = p.slug,
                             userID = p.userID,
                             post_text = p.post_text,
                             publishedDate = p.publishedDate,
                             createdDate = p.createdDate,
                             lastModified = p.lastModified,
                             keywords = p.keywords,
                             meta_title = p.meta_title,
                             meta_description = p.meta_description,
                             active = p.active,
                             author = GetAuthor(p.userID),
                             categories = (from c in db.BlogCategories join pc in db.BlogPost_BlogCategories on c.blogCategoryID equals pc.blogCategoryID where pc.blogPostID.Equals(p.blogPostID) select c).ToList<BlogCategory>(),
                             comments = (from cm in db.Comments where cm.blogPostID.Equals(p.blogPostID) && cm.approved.Equals(true) && cm.active.Equals(true) select cm).ToList<Comment>(),
                             mod_comments = (from cm in db.Comments where cm.blogPostID.Equals(p.blogPostID) && cm.approved.Equals(false) && cm.active.Equals(true) select cm).ToList<Comment>()
                         }).First<PostWithCategories>();

                return post;
            } catch (Exception e) {
                return new PostWithCategories();
            }
        }

        public static void Delete(int id = 0) {
            try {
                CurtDevDataContext db = new CurtDevDataContext();
                BlogPost p = db.BlogPosts.Where(x => x.blogPostID == id).FirstOrDefault<BlogPost>();
                p.active = false;
                db.SubmitChanges();
            } catch (Exception e) {
                throw e;
            }
        }

        private static user GetAuthor(int id = 0) {
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            return (from u in doc_db.users where u.userID.Equals(id) select u).First<user>();
        }
    }

    public class PostWithCategories : BlogPost {
        public user author { get; set; }
        public List<BlogCategory> categories { get; set; }
        public List<Comment> comments { get; set; }
        public List<Comment> mod_comments { get; set; }
    }
}
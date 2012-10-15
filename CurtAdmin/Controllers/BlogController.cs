using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;

namespace CurtAdmin.Controllers
{
    public class BlogController : BaseController
    {
        //
        // GET: /Blog/

        public ActionResult Index()
        {
            // Get all the posts
            List<PostWithCategories> posts = BlogPostModel.GetAll();
            ViewBag.posts = posts;

            return View();
        }

        [ValidateInput(false)]
        public ActionResult Save(List<string> categories, int id = 0, int userid = 0, bool publish = false, string title = "", int contentID = 0, string content = "", string meta_title = "", string meta_description = "", string keywords = "") {
            try {
                if (title.Length == 0) { throw new Exception("You must enter a title for the post"); }

                CurtDevDataContext db = new CurtDevDataContext();
                if (id == 0) {
                    BlogPost post = new BlogPost {
                        userID = userid,
                        post_title = title,
                        slug = UDF.GenerateSlug(title),
                        post_text = content,
                        createdDate = DateTime.Now,
                        lastModified = DateTime.Now,
                        meta_title = meta_title.Trim(),
                        meta_description = meta_description.Trim(),
                        keywords = keywords,
                        active = true
                    };
                    if (publish)
                        post.publishedDate = DateTime.Now;
                    db.BlogPosts.InsertOnSubmit(post);
                    db.SubmitChanges();

                    if (categories.Count() > 0) {
                        foreach (string cat in categories) {
                            if (cat != "") {
                                try {
                                    BlogPost_BlogCategory postcat = new BlogPost_BlogCategory {
                                        blogPostID = post.blogPostID,
                                        blogCategoryID = Convert.ToInt32(cat)
                                    };
                                    db.BlogPost_BlogCategories.InsertOnSubmit(postcat);
                                } catch { }
                            }
                        }
                    }
                } else {
                    BlogPost post = db.BlogPosts.Where(x => x.blogPostID == id).FirstOrDefault<BlogPost>();

                    post.meta_title = meta_title.Trim();
                    post.meta_description = meta_description.Trim();
                    post.keywords = keywords.Trim();
                    post.userID = userid;
                    post.lastModified = DateTime.Now;
                    post.post_title = title.Trim();
                    post.slug = UDF.GenerateSlug(title.Trim());
                    post.post_text = content.Trim();

                    if (publish) {
                        if (post.publishedDate == null) { post.publishedDate = DateTime.Now; }
                    } else {
                        post.publishedDate = null;
                    }

                    List<BlogPost_BlogCategory> postcats = db.BlogPost_BlogCategories.Where(x => x.blogPostID == post.blogPostID).ToList<BlogPost_BlogCategory>();
                    db.BlogPost_BlogCategories.DeleteAllOnSubmit<BlogPost_BlogCategory>(postcats);
                    db.SubmitChanges();

                    if (categories.Count() > 0) {
                        foreach (string cat in categories) {
                            if (cat != "") {
                                try {
                                    BlogPost_BlogCategory postcat = new BlogPost_BlogCategory {
                                        blogPostID = post.blogPostID,
                                        blogCategoryID = Convert.ToInt32(cat)
                                    };
                                    db.BlogPost_BlogCategories.InsertOnSubmit(postcat);
                                } catch { }
                            }
                        }
                    }
                }
                db.SubmitChanges();

                return RedirectToAction("Index");
            } catch (Exception e) {
                ViewBag.err = e.Message;
                ViewBag.content = content;
                ViewBag.title = title;
                ViewBag.meta_title = meta_title;
                ViewBag.meta_description = meta_description;
                ViewBag.keywords = keywords;
                ViewBag.userid = userid;
                if (id == 0) {
                    return RedirectToAction("Add", new { err = e.Message });
                } else {
                    return RedirectToAction("Edit", new { id = id, err = e.Message });
                }
            }
        }

        public ActionResult Add() {
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            List<user> authors = doc_db.users.Where(x => x.isActive == 1).OrderBy(x => x.lname).ToList<user>();
            ViewBag.authors = authors;

            List<BlogCategory> categories = BlogCategoryModel.GetAll();
            ViewBag.categories = categories;

            return View();
        }

        public ActionResult Edit(int id = 0)
        {
            // Get all the Authors
            DocsLinqDataContext doc_db = new DocsLinqDataContext();
            List<user> authors = doc_db.users.Where(x => x.isActive == 1).OrderBy(x => x.lname).ToList<user>();
            ViewBag.authors = authors;

            List<BlogCategory> categories = BlogCategoryModel.GetAll();
            ViewBag.categories = categories;

            PostWithCategories post = BlogPostModel.Get(id);
            ViewBag.post = post;

            return View();
        }

        public string Delete(int id = 0) {
            try {
                BlogPostModel.Delete(id);
                return "";
            } catch (Exception e) {
                return e.Message;
            }
        }

    }
}

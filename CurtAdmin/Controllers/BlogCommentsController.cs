using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CurtAdmin.Models;
using System.Web.Script.Serialization;

namespace CurtAdmin.Controllers
{
    public class BlogCommentsController : BaseController
    {
        //
        // GET: /Post/

        public ActionResult Index() {

            List<CommentWithPost> comments = BlogCommentModel.GetAll();
            ViewBag.comments = comments;
            return View();
        }

        public ActionResult ViewComment(int id = 0)
        {
            CommentWithPost comment = BlogCommentModel.Get(id);
            ViewBag.comment = comment;

            return View();
        }

        public string CommentJSON(int id = 0) {
            JavaScriptSerializer js = new JavaScriptSerializer();
            CommentNoPost comment = BlogCommentModel.GetNoPost(id);
            return js.Serialize(comment);
        }

        public ActionResult PostComments(int id = 0)
        {
            CurtDevDataContext db = new CurtDevDataContext();
            PostWithCategories post = BlogPostModel.Get(id);
            List<Comment> comments = BlogCommentModel.GetAllByPost(id);

            ViewBag.post = post;
            ViewBag.comments = comments;

            return View();
        }

        public void Approve(int id = 0)
        {
            try
            {
                BlogCommentModel.Approve(id);
                
            }
            catch {}
            Response.Redirect("/Blog#tab=comments"); 
        }

        public string ApproveAjax(int id = 0) {
            try {
                BlogCommentModel.Approve(id);
                return CommentJSON(id);
            } catch (Exception e) { return e.Message; }
        }

        public string Delete(int id = 0)
        {
            try
            {
                BlogCommentModel.Delete(id);
                return "[]";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}

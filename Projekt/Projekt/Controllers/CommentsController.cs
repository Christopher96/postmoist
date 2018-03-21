using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Projekt.Models;

namespace Projekt.Controllers
{
    public class CommentsController : Controller
    {
        private ProjectDBEntities db = new ProjectDBEntities();

        public ActionResult Index(int post_id, int? page)
        {
            var dbComments = db.Posts.Include(c => c.User).AsEnumerable().Where(c => c.post_id == post_id).OrderBy(c => c.created);
            var comments = from c in dbComments select c;

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            return View(comments.ToPagedList(pageNumber, pageSize));
        }

        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Normal")]
        public ActionResult Create(int post_id, string comment)
        {
            if (ModelState.IsValid)
            {
                Comment newComment = new Comment();
                newComment.comment = comment;
                newComment.user_id = (int)Session["user_id"];
                newComment.post_id = post_id;
                newComment.created = DateTime.Now;
                db.Comments.Add(newComment);
                db.SaveChanges();
            }

            return RedirectToAction("Details", "Posts", new { id = post_id });
        }

        // GET: Posts/Delete/5
        [Authorize(Roles = "Admin, Normal")]
        public ActionResult Delete(int? id)
        {
            Comment comment = db.Comments.Find(id);
            int post_id = comment.post_id;

            if (comment.user_id == (int)Session["user_id"])
            {
                db.Comments.Remove(comment);
                db.SaveChanges();
            }

            return RedirectToAction("Details", "Posts", new { id = post_id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using PagedList;
using Projekt.Models;

namespace Projekt.Controllers
{
    public class PostsController : Controller
    {
        private ProjectDBEntities db = new ProjectDBEntities();

        // GET: Posts
        public ActionResult Index(string sort, string search, string filterSearch, int? page, bool filterUser = false)
        {
            var dbPosts = db.Posts.Include(p => p.Image).Include(p => p.User).AsEnumerable().OrderBy(p => p.title, StringComparer.CurrentCulture);
            var posts = from m in dbPosts select m;

            if (search == null)
            {
                search = filterSearch;
            }
            else
            {
                page = 1;
            }

            ViewBag.FilterSearch = search;

            if (!string.IsNullOrEmpty(search))
            {
                posts = posts.Where(
                     p => p.title.ToUpper().Contains(search.ToUpper()) ||
                     p.description.ToUpper().Contains(search.ToUpper()) ||
                     p.User.username.ToUpper().Contains(search.ToUpper())
                );
            }

            if (filterUser)
            {
                if (User.Identity.IsAuthenticated)
                {
                    posts = posts.Where(
                        i => i.user_id == (int)Session["user_id"]
                    );
                }
            }

            ViewBag.FilterUser = filterUser;
            ViewBag.SortTitle = string.IsNullOrEmpty(sort) ? "title_asc" : "";
            ViewBag.SortCreate = sort == "created_desc" ? "created_asc" : "created_desc";
            ViewBag.Sort = sort;

            switch (sort)
            {
                case "title_desc":
                    posts = posts.OrderByDescending(m => m.title);
                    break;
                case "created_desc":
                    posts = posts.OrderByDescending(m => m.created);
                    break;
                case "created_asc":
                    posts = posts.OrderBy(m => m.created);
                    break;
                default:
                    posts = posts.OrderBy(m => m.title);
                    break;
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            return View(posts.ToPagedList(pageNumber, pageSize));
        }

        // GET: Posts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // GET: Posts/Create
        [Authorize(Roles = "Normal")]
        public ActionResult Create()
        {
            return View();
        }


        // POST: Posts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Normal")]
        public ActionResult Create(Post post, HttpPostedFileBase ImageData)
        {
            Image newImg = new Image();

            if (IsImage(ImageData))
            {
                newImg.image = new byte[ImageData.ContentLength];
                newImg.title = ImageData.FileName;
                ImageData.InputStream.Read(newImg.image, 0, ImageData.ContentLength);
            }
            else
            {
                ModelState.AddModelError("ImageData", "You must upload an image.");
            }

            if (ModelState.IsValid)
            {
                db.Images.Add(newImg);
                if(db.SaveChanges() > 0)
                {
                    int imgId = newImg.image_id;

                    post.image_id = imgId;
                    post.user_id = (int)Session["user_id"];
                    post.created = DateTime.Now;

                    db.Posts.Add(post);
                    if(db.SaveChanges() > 0)
                    {
                        int postId = post.post_id;
                        return RedirectToAction("Details", new { id = postId });
                    }
                    else
                    {
                        db.Images.Remove(newImg);
                        db.SaveChanges();
                    }
                }
                

            }

            ModelState.AddModelError(String.Empty, "There was an error creating the post.");

            return View(post);
        }

        private bool IsImage(HttpPostedFileBase file)
        {
            if(file == null)
            {
                return false;
            }
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        // GET: Posts/Edit/5
        [Authorize(Roles = "Normal")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            ViewBag.image_id = new SelectList(db.Images, "image_id", "title", post.image_id);
            ViewBag.user_id = new SelectList(db.Users, "user_id", "username", post.user_id);
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Normal")]
        public ActionResult Edit([Bind(Include = "post_id,user_id,image_id,title,description,created")] Post post)
        {
            if (ModelState.IsValid)
            {
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.image_id = new SelectList(db.Images, "image_id", "title", post.image_id);
            ViewBag.user_id = new SelectList(db.Users, "user_id", "username", post.user_id);
            return View(post);
        }

        // GET: Posts/Delete/5
        [Authorize(Roles = "Normal")]
        public ActionResult Delete(int? id)
        {
            Post post = db.Posts.Find(id);
            if(post.user_id == (int)Session["user_id"])
            {
                db.Images.Remove(post.Image);
                db.Comments.RemoveRange(post.Comments);
                db.Posts.Remove(post);
                db.SaveChanges();
            }
            
            return RedirectToAction("Index");
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

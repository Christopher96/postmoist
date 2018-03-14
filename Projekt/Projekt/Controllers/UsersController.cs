using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PasswordSecurity;
using Projekt.Models;

namespace Projekt.Controllers
{
    public class UsersController : Controller
    {
        private ProjectDBEntities db = new ProjectDBEntities();

        // GET: Users
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "username,password")] UserLogin user)
        {
            if (ModelState.IsValid)
            {
                var dbUser = db.Users.SingleOrDefault(u => u.username.Equals(user.username));
                if (dbUser != null)
                {
                    if(PasswordStorage.VerifyPassword(user.password, dbUser.password_hash))
                    {
                        Session["user_id"] = dbUser.user_id;
                        FormsAuthentication.SetAuthCookie(dbUser.username, false);
                        return RedirectToRoute(new { controller = "Posts", action = "Index" });
                    }
                }

                ModelState.AddModelError(String.Empty, "Invalid username or password.");
            }
            Debug.WriteLine(user.username + " " + user.password);

            return View(user);
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "username,password,confirm_password")] UserRegister user)
        {
            if (ModelState.IsValid)
            {
                var dbUser = db.Users.SingleOrDefault(u => u.username.Equals(user.username));
                if (dbUser == null)
                {
                    User newUser = new User();
                    newUser.username = user.username;
                    newUser.password_hash = PasswordStorage.CreateHash(user.password);
                    newUser.role = "Normal";
                    newUser.created = DateTime.Now;
                    db.Users.Add(newUser);
                    db.SaveChanges();

                    return RedirectToRoute(new { controller = "Users", action = "Login" });
                }

                ModelState.AddModelError(String.Empty, "A user with this username already exists.");
            }
            return View(user);
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "user_id,username,password_hash,role,created")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "user_id,username,password_hash,role,created")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Blog.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Text.RegularExpressions;

namespace Blog.Controllers
{
    [RequireHttps]
    public class CommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Comments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(int Id, string message)
        {
            Regex tagRegex = new Regex(@"<\s*([^ >]+)[^>]*>.*?<\s*/\s*\1\s*>");
            var slug = db.Posts.AsNoTracking().FirstOrDefault(b => b.ID == Id).Slug;
            if (message.Trim().Length < 3 || tagRegex.IsMatch(message))
            {
                TempData["warning"] = "warn";
                return RedirectToAction("details", "posts", new { slug = slug });
            }           
            var comment = new Comment();
            comment.AuthorId = User.Identity.GetUserId();
            comment.postID = Id;
            comment.Body = message.Trim();
            comment.Created = DateTimeOffset.Now;

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return RedirectToAction("details", "posts", new { slug = slug });
            }
            else { return RedirectToAction("details", "posts", new { slug = slug }); }

            //ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", comment.AuthorId);
            //ViewBag.postID = new SelectList(db.Posts, "ID", "Title", comment.postID);
            //return View(comment);
        }

        // GET: Comments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("index", "Home");
            }
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            if (UserManager.IsInRole(User.Identity.GetUserId(), "Admin") ||
                UserManager.IsInRole(User.Identity.GetUserId(), "Moderator"))
            {
                ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", comment.AuthorId);
                ViewBag.postID = new SelectList(db.Posts, "ID", "Title", comment.postID);
                return View(comment);
            } 
            else { return RedirectToAction("index", "Home"); }
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,postID,AuthorId,Body,Created,Updated,UpdateReason")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.Updated = DateTimeOffset.Now;
                db.Entry(comment).State = EntityState.Modified;
                db.SaveChanges();
                var postId = db.Comments.AsNoTracking().Where(b => b.ID == comment.ID).Select(b => b.postID).ToList().ElementAt(0);
                var slug = db.Posts.AsNoTracking().FirstOrDefault(b => b.ID == postId).Slug;
                return RedirectToAction(slug, "Blog");
            }
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", comment.AuthorId);
            ViewBag.postID = new SelectList(db.Posts, "ID", "Title", comment.postID);
            return View(comment);
        }

        // GET: Comments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("index", "Home");
            }
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            if (UserManager.IsInRole(User.Identity.GetUserId(), "Admin") ||
                UserManager.IsInRole(User.Identity.GetUserId(), "Moderator"))
            {
                return View(comment);
            }
            else { return RedirectToAction("index", "Home"); }
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var postId = db.Comments.AsNoTracking().Where(b => b.ID == id).Select(b => b.postID).ToList().ElementAt(0);
            var slug = db.Posts.AsNoTracking().FirstOrDefault(b => b.ID == postId).Slug;
            Comment comment = db.Comments.Find(id);
            db.Comments.Remove(comment);
            db.SaveChanges();
            return RedirectToAction(slug, "Blog");
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

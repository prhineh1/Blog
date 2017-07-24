using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Blog.Models;
using Blog.Helpers;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;


namespace Blog.Controllers
{
    [RequireHttps]
    public class PostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Posts
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("index", "Home");
            }

            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            if (UserManager.IsInRole(User.Identity.GetUserId(), "Admin"))
            {
                return View(db.Posts.ToList());
            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        // GET: Posts/Details/5
        public ActionResult Details(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.Post post = db.Posts.FirstOrDefault(p => p.Slug == slug);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        // GET: Posts/Create
        public ActionResult Create()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("index", "Home");
            }

            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            if (UserManager.IsInRole(User.Identity.GetUserId(), "Admin"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        // POST: Posts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Body,Published,Abstract,MediaURL")] Models.Post post, 
            HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                var slug = StringUtilities.URLFriendly(post.Title);

                if (string.IsNullOrWhiteSpace(slug))
                {
                    ModelState.AddModelError("Title", "Invalid Title");
                    return View(post);
                }
                if (db.Posts.Any(p => p.Slug == slug))
                {
                    ModelState.AddModelError("Title", "the title must be unique");
                    return View(post);
                }

                if (ImageUploadValidator.IsWebFriendlyImage(image))
                { 

                var notStored = true;
                try
                {

                    foreach (var img in Directory.GetFiles(Path.Combine(Server.MapPath("~/Images/"))))
                    {
                        var justImg = Path.GetFileName(img);
                        if (Path.GetFileName(image.FileName) == justImg)
                        {
                            post.MediaURL = "/Images/" + Path.GetFileName(image.FileName);
                            notStored = false;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return View(post);
                }

                if (notStored)
                    {
                        var fileName = Path.GetFileName(image.FileName);
                        string completeName = DateTime.Now.ToString("hh.mm.ss.ffffff") + "_" + fileName;
                        image.SaveAs(Path.Combine(Server.MapPath("~/Images/"), completeName));
                        post.MediaURL = "/Images/" + completeName;
                    }           
                }

                post.Slug = slug;
                post.Created = DateTimeOffset.Now;
                db.Posts.Add(post);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(post);
        }
        // GET: Posts/Edit/5
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
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("index", "Home");
            }

            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            if (UserManager.IsInRole(User.Identity.GetUserId(), "Admin"))
            {
                return View(post);
            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Title,Body,Published,Abstract,MediaURL,Created")] Models.Post post,
            HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                    var slug = StringUtilities.URLFriendly(post.Title);
                 
                    if (db.Posts.Any(p => p.Slug == slug && p.ID != post.ID))
                    {
                        ModelState.AddModelError("Title", "the title must be unique");
                        return View(post);
                    }

                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var notStored = true;
                    try
                    {
                      
                        foreach (var img in Directory.GetFiles(Path.Combine(Server.MapPath("~/Images/"))))
                        {
                            var justImg = Path.GetFileName(img);
                            if (Path.GetFileName(image.FileName) == justImg)
                            {
                                post.MediaURL = "/Images/" + Path.GetFileName(image.FileName);
                                notStored = false;
                                break;
                            }
                        }
                    }
                    catch  (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return View(post);
                    }

                    if (notStored)
                    {
                        var fileName = Path.GetFileName(image.FileName);
                        string completeName = DateTime.Now.ToString("hh.mm.ss.ffffff") + "_" + fileName;
                        image.SaveAs(Path.Combine(Server.MapPath("~/Images/"), completeName));
                        post.MediaURL = "/Images/" + completeName;
                    }
                }

                //The User can no longer add an updated date during an edit so we store it programmatically
                post.Slug = slug;
                post.Updated = DateTimeOffset.Now;
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(post);
        }
        // GET: Posts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("index", "Home");
            }

            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            if (UserManager.IsInRole(User.Identity.GetUserId(), "Admin"))
            {
                return View(post);
            }
            else
            {
                return RedirectToAction("index", "Home");
            }
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Models.Post post = db.Posts.Find(id);
            db.Posts.Remove(post);
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

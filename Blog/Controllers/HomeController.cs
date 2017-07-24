using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blog.Models;
using System.Net.Mail;
using System.Configuration;
using System.Threading.Tasks;
using PagedList;
using PagedList.Mvc;
using System.Text.RegularExpressions;

namespace Blog.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Contact(EmailModel model)
        {
            if (ModelState.IsValid)
            {
                Regex tagRegex = new Regex(@"<\s*([^ >]+)[^>]*>.*?<\s*/\s*\1\s*>");
                if (model.Body.Trim().Length < 3 || tagRegex.IsMatch(model.Body))
                {
                    TempData["warning"] = "Message must be at least 3 chars long.";
                    return View(model);
                }
                try
                {
                    var body = "<p> Email From: {0}({1})</p><p>Subject: <strong>{2}</strong></p><p>{3}</p>";
                    var from = "My blog<rhinehartphillip@gmail.com>";

                    var email = new MailMessage(from,
                        ConfigurationManager.AppSettings["emailto"])
                    {
                        Subject = "Blog Contact Email",
                        Body = string.Format(body, model.FromName, model.FromEmail, model.Subject, model.Body),

                        IsBodyHtml = true
                    };

                    var svc = new PersonalEmail();
                    await svc.SendAsync(email);

                    return RedirectToAction("Contact");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.FromResult(0);
                }
            }
            return View(model);
        }
        
        public ActionResult Index(int? page, string searchStr, string month=null, string year=null, string category= null)
        {
            if (category != null)
            {
                category = "#" + category;
                ViewBag.Search = searchStr;
                var blogListCat = IndexSearch(searchStr);

                var pageSizeCat = 4;
                var pageNumberCat = (page ?? 1);

                return View(blogListCat.Where(p => p.Published)
                                       .Where(p => p.Abstract.ToString().Contains(category))
                                       .ToPagedList(pageNumberCat, pageSizeCat));
            }
            if (month != null && year != null)
            {
                ViewBag.Search = searchStr;
                var blogListArch = IndexSearch(searchStr);

                var pageSizeArch = 4;
                var pageNumberArch = (page ?? 1);
                
                return View(blogListArch.Where(p => p.Published)
                                        .Where(p => p.Created.Year.ToString() == year)
                                        .Where(p => p.Created.Month.ToString() == month)
                                        .ToPagedList(pageNumberArch, pageSizeArch));

            }
            ViewBag.Search = searchStr;
            var blogList = IndexSearch(searchStr);

            var pageSize = 4;
            var pageNumber = (page ?? 1);

            return View((blogList.Where(p => p.Published).ToPagedList(pageNumber, pageSize)));
        }

        public ActionResult IndexCat(int? page, string searchStr, string category=null)
        {
            if (category != null)
            {
                category = "#" + category;
                ViewBag.Search = searchStr;
                var blogListCat = IndexSearch(searchStr);

                var pageSizeCat = 4;
                var pageNumberCat = (page ?? 1);

                var x = blogListCat.Where(p => p.Published)
                                       .Where(p => p.Abstract.ToString().Contains(category));
                                       

                return View("~/Views/Home/index.cshtml", blogListCat.Where(p => p.Published)
                                       .Where(p => p.Abstract.ToString().Contains(category))
                                       .ToPagedList(pageNumberCat, pageSizeCat));
            }     
            ViewBag.Search = searchStr;
            var blogList = IndexSearch(searchStr);

            var pageSize = 4;
            var pageNumber = (page ?? 1);

            return View("~/Views/Home/index.cshtml", (blogList.Where(p => p.Published).ToPagedList(pageNumber, pageSize)));
        }

        public IQueryable<Post> IndexSearch(string searchStr)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            IQueryable<Post> result = null;
            if (searchStr != null)
            {
                result = db.Posts.AsQueryable();
                result = result.Where(p => p.Title.Contains(searchStr) ||
                                      p.Body.Contains(searchStr) ||
                                      p.Comments.Any(c => c.Body.Contains(searchStr) ||
                                                     c.Author.FirstName.Contains(searchStr) ||
                                                     c.Author.LastName.Contains(searchStr) ||
                                                     c.Author.DisplayName.Contains(searchStr) ||
                                                     c.Author.Email.Contains(searchStr)));
            }
            else
            {
                result = db.Posts.AsQueryable();
            }

            return result.OrderByDescending(p => p.Created);
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}
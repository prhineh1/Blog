using Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Blog.ViewModel
{
    public class Recent
    {
        public static IEnumerable<Blog.Models.Post> recentPost()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            return db.Posts.OrderByDescending(p => p.Created).Where(p => p.Published).ToList();
        }

        public static IEnumerable<Blog.Models.Comment> recentComment()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            return db.Comments.OrderByDescending(p => p.Created).ToList();
        }

        public static List<List<string>> recentCommentPost()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            List<List<string>> commentedPost = new List<List<string>>();
            List<string> slug = new List<string>();
            List<string> title = new List<string>();
            var postIden = db.Comments.OrderByDescending(p => p.Created).Select(p => p.postID).ToList();
            foreach (int id in postIden)
            {
                slug.Add(db.Posts.FirstOrDefault(p => p.ID == id).Slug);
                title.Add(db.Posts.FirstOrDefault(p => p.ID == id).Title);
            }
            commentedPost.Add(title);
            commentedPost.Add(slug);
            return commentedPost;
        }

        public static int commentCount(int id)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            return db.Comments.Where(p => p.postID == id).Count();
        }
    }
}
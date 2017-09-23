using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Models
{
    public class Post
    {
        public Post()
        {
            this.Comments = new HashSet<Comment>();
            CategoryDictionary = new Dictionary<string, string>()
            {
                {"Auto", "Auto" },
                {"Food", "Food" },
                {"Entertainment", "Entertainment" }
            };
        }
        public int ID { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public string Title { get; set; }
        [AllowHtml]
        public string Abstract { get; set; }
        public string Slug { get; set; }
        [AllowHtml]
        public string Body { get; set; }
        public string MediaURL { get; set; }
        public bool Published { get; set; }
        public string Category { get; set; }
        public Dictionary<string, string> CategoryDictionary { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
    }
}
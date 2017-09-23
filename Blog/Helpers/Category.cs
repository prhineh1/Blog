using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Blog.Helpers
{
    public class Category
    {
        public Dictionary<string, string> CategoryDictionary = new Dictionary<string, string>()
        {
                {"Auto", "Auto" },
                {"Food", "Food" },
                {"Entertainment", "Entertainment" }
        };
    }
}
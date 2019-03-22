using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebPreviewTool.Models
{
    public class ToolUser
    {
        public string id { get; set; }
        public string email { get; set; }
        public List<Bookmark> bookmarks { get; set; }

        public ToolUser(string id, string email, List<Bookmark> bookmarks)
        {
            this.id = id;
            this.email = email;
            this.bookmarks = bookmarks;
        }
    }
}
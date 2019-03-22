using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace WebPreviewTool.Models
{

    public class Bookmark
    {
        public List<string> tags = new List<string>();
        public string url { get; set; }
        public string imgLoc { get; set; }
        public string id { get; set; }
        public List<string> collections = new List<string>();

        public Bookmark()
        {

        }

        public Bookmark(string url, string imgLoc, string id)
        {
            this.id = id;
            this.url = url;
            this.imgLoc = imgLoc;
        }
    }
}
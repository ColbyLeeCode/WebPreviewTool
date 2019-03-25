using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebPreviewTool.Models;

namespace WebPreviewTool.ViewModel
{
    public class MarkDisplayVM
    {
        public List<Bookmark> bookmarks { get; set; }
        public List<string> tags { get; set; }
        public List<string> collections { get; set; }
        public string selectedCollection { get; set; }

        public MarkDisplayVM(List<Bookmark> bookmarks, List<string> collections, List<string> tags, string selectedCollection = null)
        {
            this.bookmarks = bookmarks;
            this.tags = tags;
            this.collections = collections;
            this.selectedCollection = selectedCollection;
        }
    }
}
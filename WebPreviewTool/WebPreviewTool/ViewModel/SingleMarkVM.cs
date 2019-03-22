using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebPreviewTool.Models;

namespace WebPreviewTool.ViewModel
{
   
    public class SingleMarkVM
    {
        public Bookmark Bm { get; set; }
        public List<string> Collections { get; set; }
        public List<string> Tags { get; set; }

        public SingleMarkVM(Bookmark bm, List<string> collections, List<string> tags)
        {
            this.Bm = bm;
            this.Collections = collections;
            this.Tags = tags;
        }

        public SingleMarkVM()
        {
            this.Collections = new List<string>();
            this.Tags = new List<string>();
        }
    }
}
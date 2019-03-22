using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebPreviewTool.Models
{
    public class CollectionsInfo
    {
        public List<string> Collections { get; set; }
        public List<string> Tags { get; set; }

        public CollectionsInfo(List<string> Collections, List<string> Tags)
        {
            this.Collections = Collections;
            this.Tags = Tags;
        }

        public CollectionsInfo()
        {
            Collections = new List<string>();
            Tags = new List<string>();
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebPreviewTool.Models
{
    public class CollectionsAdd
    {
        [Required]
        public string collect { get; set; }
    }
}
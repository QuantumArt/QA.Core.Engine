using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
#pragma warning disable 1591

namespace QA.Core.Engine.Editing.Models
{
    public class NodeEditingModel
    {
        public int ItemId { get; set; }

        [Required]
        //[MinLength(2)]
        //[MaxLength(50)]
        public string Name { get; set; }
        [Required]
        //[MinLength(2)]
        //[MaxLength(250)]
        public string Title { get; set; }
        public TagBuilder Control { get; set; }

        public string ZoneName { get; set; }

        public int ParentId { get; set; }

        public string TypeName { get; set; }
        public bool ForcePublish { get; set; }

        public bool IsPublished { get; set; }

        public string ReturnUrl { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace QA.Core.Engine.Editing.Models
{
    public class MoveNodeModel
    {
        [Required]
        public int TargetNodeId { get; set; }
        [Required]
        public int DestinationNodeId { get; set; }
        [Required]
        public string MovementType { get; set; }
        [Required]
        public string ZoneName { get; set; }

    }
}
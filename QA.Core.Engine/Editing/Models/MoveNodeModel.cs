using System.ComponentModel.DataAnnotations;
#pragma warning disable 1591

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

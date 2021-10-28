using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TreeStructureManagement.Models
{
    /// <summary>
    /// Model class for transferring a parent's children to a new parent
    /// </summary>
    public class DeleteAndMoveDto
    {
        [Key]
        [Required()]
        public long NodeId { get; set; }

        [Required()]
        [Display(Name = "New parent")]
        public long TargetId { get; set; }
    }
}

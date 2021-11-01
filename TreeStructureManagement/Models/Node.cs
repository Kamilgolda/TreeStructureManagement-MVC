using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TreeStructureManagement.Models
{
    /// <summary>
    /// The class representing the node of the tree structure
    /// </summary>
    public class Node
    {
        [Key]
        [Display(Name = "ID")]
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ0-9.\s]*$", ErrorMessage = "Characters are not allowed. (letter, number, whitespace)")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Parent")]
        public long? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Node Parent { get; set; }

        public virtual ICollection<Node> Children { get; set; }
    }
}

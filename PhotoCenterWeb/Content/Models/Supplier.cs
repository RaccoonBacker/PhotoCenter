using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoCenterWeb.Models
{
    [Table("Suppliers")]
    public class Supplier
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [StringLength(150)]
        [Column("Name")]
        public string Name { get; set; }

        [StringLength(100)]
        [Column("ContactPerson")]
        public string ContactPerson { get; set; }

        [StringLength(30)]
        [Column("Phone")]
        public string Phone { get; set; }

        [StringLength(150)]
        [Column("Email")]
        public string Email { get; set; }

        [StringLength(255)]
        [Column("Address")]
        public string Address { get; set; }

        // Навигационные свойства
        public virtual ICollection<SupplyRequest> SupplyRequests { get; set; }

        public Supplier()
        {
            SupplyRequests = new HashSet<SupplyRequest>();
        }
    }
}
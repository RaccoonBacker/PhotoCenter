using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoCenterWeb.Models
{
    [Table("Services")]
    public class Service
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [StringLength(150)]
        [Column("Name")]
        public string Name { get; set; }

        [StringLength(500)]
        [Column("Description")]
        public string Description { get; set; }

        [Column("Price")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(20)]
        [Column("ServiceType")]
        public string ServiceType { get; set; } // 'Печать', 'Съемка', 'Ретушь'

        // Навигационные свойства
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<ServiceMaterial> ServiceMaterials { get; set; }


        public Service()
        {
            OrderDetails = new HashSet<OrderDetail>();
            ServiceMaterials = new HashSet<ServiceMaterial>();
        }
    }
}
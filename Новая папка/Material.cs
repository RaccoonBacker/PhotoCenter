using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoCenterWeb.Models
{
    [Table("Materials")]
    public class Material
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [StringLength(150)]
        [Column("Name")]
        public string Name { get; set; }

        [Column("Quantity")]
        public decimal Quantity { get; set; }

        [Required]
        [StringLength(30)]
        [Column("Unit")]
        public string Unit { get; set; }

        [Column("MinQuantity")]
        public decimal MinQuantity { get; set; }

        [Column("LastSupplyDate")]
        public DateTime? LastSupplyDate { get; set; }

        // Навигационные свойства
        public virtual ICollection<SupplyRequest> SupplyRequests { get; set; }
        public virtual ICollection<ServiceMaterial> ServiceMaterials { get; set; }

        public Material()
        {
            SupplyRequests = new HashSet<SupplyRequest>();
            ServiceMaterials = new HashSet<ServiceMaterial>();
        }
    }
}
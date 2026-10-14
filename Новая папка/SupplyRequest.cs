using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoCenterWeb.Models
{
    [Table("SupplyRequests")]
    public class SupplyRequest
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Column("MaterialID")]
        public int MaterialID { get; set; }

        [Column("SupplierID")]
        public int SupplierID { get; set; }

        [Column("Quantity")]
        public decimal Quantity { get; set; }

        [Column("RequestDate")]
        public DateTime RequestDate { get; set; }

        [Required]
        [StringLength(20)]
        [Column("Status")]
        public string Status { get; set; } // 'New', 'Ordered', 'Delivered', 'Cancelled'

        // Навигационные свойства
        [ForeignKey("MaterialID")]
        public virtual Material Material { get; set; }

        [ForeignKey("SupplierID")]
        public virtual Supplier Supplier { get; set; }
    }
}
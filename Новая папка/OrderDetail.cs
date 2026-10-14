using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoCenterWeb.Models
{
    [Table("OrderDetails")]
    public class OrderDetail
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Column("OrderID")]
        public int OrderID { get; set; }

        [Column("ServiceID")]
        public int? ServiceID { get; set; } // nullable

        [Column("ProductID")]
        public int? ProductID { get; set; } // nullable

        [Column("Quantity")]
        public int Quantity { get; set; }

        [Column("Price")]
        public decimal Price { get; set; }

        // Навигационные свойства
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }

        [ForeignKey("ServiceID")]
        public virtual Service Service { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }
    }
}
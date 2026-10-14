using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoCenterWeb.Models
{
    [Table("Products")]
    public class Product
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

        [Column("QuantityInStock")]
        public int QuantityInStock { get; set; }

        // Навигационные свойства
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }
    }
}
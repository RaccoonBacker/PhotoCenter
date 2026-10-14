using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoCenterWeb.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Column("ClientID")]
        public int ClientID { get; set; }

        [Column("EmployeeID")]
        public int? EmployeeID { get; set; } // nullable

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [Required]
        [StringLength(20)]
        [Column("Status")]
        public string Status { get; set; } // 'New', 'InProgress', 'Ready', 'Completed', 'Cancelled'

        [Column("TotalPrice")]
        public decimal TotalPrice { get; set; }

        // Навигационные свойства
        [ForeignKey("ClientID")]
        public virtual Client Client { get; set; }

        [ForeignKey("EmployeeID")]
        public virtual Employee Employee { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }

        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
            Reviews = new HashSet<Review>();
        }
    }
}
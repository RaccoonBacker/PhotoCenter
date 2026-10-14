using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoCenterWeb.Models
{
    [Table("Reviews")]
    public class Review
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Column("ClientID")]
        public int ClientID { get; set; }

        [Column("OrderID")]
        public int OrderID { get; set; }

        [StringLength(1000)]
        [Column("Text")]
        public string Text { get; set; }

        [Column("Rating")]
        public byte Rating { get; set; } // 1-5

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        // Навигационные свойства
        [ForeignKey("ClientID")]
        public virtual Client Client { get; set; }

        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }
    }
}
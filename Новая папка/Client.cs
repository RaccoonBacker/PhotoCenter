using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoCenterWeb.Models
{
    [Table("Clients")]
    public class Client
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [StringLength(30)]
        [Column("Phone")]
        public string Phone { get; set; }

        [StringLength(150)]
        [Column("Email")]
        public string Email { get; set; }

        [Column("RegistrationDate")]
        public DateTime RegistrationDate { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        public virtual ICollection<Order> Orders { get; set; }

        public virtual ICollection<Review> Reviews { get; set; }

        public Client()
        {
            Orders = new HashSet<Order>();
            Reviews = new HashSet<Review>();
        }
    }
}
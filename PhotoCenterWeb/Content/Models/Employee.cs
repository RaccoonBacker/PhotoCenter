using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoCenterWeb.Models
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        [Column("FirstName")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        [Column("LastName")]
        public string LastName { get; set; }

        [StringLength(100)]
        [Column("Position")]
        public string Position { get; set; }

        [StringLength(150)]
        [Column("Specialization")]
        public string Specialization { get; set; }

        // Навигационные свойства
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        public virtual ICollection<Order> Orders { get; set; }

        public Employee()
        {
            Orders = new HashSet<Order>();
        }
    }
}
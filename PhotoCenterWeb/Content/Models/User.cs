using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoCenterWeb.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Login")]
        public string Login { get; set; }

        [Required]
        [StringLength(255)]
        [Column("PasswordHash")]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(20)]
        [Column("Role")]
        public string Role { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; }

    }
}
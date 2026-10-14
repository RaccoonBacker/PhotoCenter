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

        [StringLength(100)]
        [Column("FirstName")]
        public string FirstName { get; set; }

        [StringLength(100)]
        [Column("LastName")]
        public string LastName { get; set; }

        [Required]
        [StringLength(20)]
        [Column("Role")]
        public string Role { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; }

        [Column("MustChangePassword")]
        public bool MustChangePassword { get; set; }
    }
}
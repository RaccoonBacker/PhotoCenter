using System.ComponentModel.DataAnnotations;

namespace PhotoCenterWeb.DTOs.Auth
{
    public class LoginDto
    {
        [Required]
        [StringLength(100)]
        public string Login { get; set; }

        [Required]
        [StringLength(200)]
        public string Password { get; set; }
    }
}
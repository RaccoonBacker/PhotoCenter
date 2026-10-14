using System.ComponentModel.DataAnnotations;

namespace PhotoCenterWeb.DTOs.Users
{
    public class UserUpdateDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string Login { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        [RegularExpression("^(Client|Employee|Admin)$",
            ErrorMessage = "Недопустимая роль пользователя.")]
        public string Role { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }
    }
}
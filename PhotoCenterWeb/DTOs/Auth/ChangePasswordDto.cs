using System.ComponentModel.DataAnnotations;

namespace PhotoCenterWeb.DTOs.Auth
{
    public class ChangePasswordDto
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 8,
            ErrorMessage = "Пароль должен быть не короче 8 символов.")]
        public string NewPassword { get; set; }
    }
}
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using BCryptLibrary = BCrypt.Net.BCrypt;
using PhotoCenterWeb.DTOs.Auth;
using PhotoCenterWeb.Helpers;
using PhotoCenterWeb.Models;
using System.Data.Entity;

namespace PhotoCenterWeb.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthApiController : ApiController
    {
        [HttpPost]
        [Route("login")]
        public async Task<IHttpActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Login) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Логин и пароль обязательны.");
            }

            using (var context = new PhotoCenterContext())
            {
                var user = await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u =>
                        u.Login == dto.Login &&
                        u.IsActive);

                if (user == null)
                    return Unauthorized();

                bool passwordValid;

                try
                {
                    passwordValid = BCryptLibrary.Verify(
                        dto.Password,
                        user.PasswordHash
                    );
                }
                catch
                {
                    return Unauthorized();
                }

                if (!passwordValid)
                    return Unauthorized();

                var token = JwtHelper.GenerateToken(
                    user.ID,
                    user.Login,
                    user.Role
                );

                return Ok(new
                {
                    token = token,
                    user = new
                    {
                        id = user.ID,
                        login = user.Login,
                        role = user.Role,
                        isActive = user.IsActive,
                        mustChangePassword = user.MustChangePassword
                    }
                });
            }
        }

        [HttpPost]
        [Route("change-password")]
        [Authorize]
        public async Task<IHttpActionResult> ChangePassword(
            ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var identity = User.Identity as System.Security.Claims.ClaimsIdentity;

            if (identity == null)
                return Unauthorized();

            var userIdClaim = identity.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            );

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            using (var context = new PhotoCenterContext())
            {
                var user = await context.Users.FindAsync(userId);

                if (user == null)
                    return Unauthorized();

                bool oldPasswordValid;

                try
                {
                    oldPasswordValid = BCryptLibrary.Verify(
                        dto.OldPassword,
                        user.PasswordHash
                    );
                }
                catch
                {
                    return BadRequest("Неверный текущий пароль.");
                }

                if (!oldPasswordValid)
                    return BadRequest("Неверный текущий пароль.");

                user.PasswordHash =
                    BCryptLibrary.HashPassword(dto.NewPassword);

                user.MustChangePassword = false;

                await context.SaveChangesAsync();

                return Ok(new { message = "Пароль изменён." });
            }
        }
    }
}
using System.Linq;
using System.Web.Http;
using BCryptLibrary = BCrypt.Net.BCrypt;
using PhotoCenterWeb.Helpers;
using PhotoCenterWeb.Models;

namespace PhotoCenterWeb.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthApiController : ApiController
    {
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Login) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Логин и пароль обязательны.");
            }

            using (var context = new PhotoCenterContext())
            {
                var user = context.Users
                    .FirstOrDefault(u =>
                        u.Login == dto.Login &&
                        u.IsActive);

                if (user == null)
                {
                    return Unauthorized();
                }

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
                    // Некорректный или неподдерживаемый формат хеша
                    return Unauthorized();
                }

                if (!passwordValid)
                {
                    return Unauthorized();
                }

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
                        isActive = user.IsActive
                    }
                });
            }
        }
    }

    public class LoginDto
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using BCryptLibrary = BCrypt.Net.BCrypt;
using PhotoCenterWeb.DTOs.Users;
using PhotoCenterWeb.Models;
using System.Data.Entity;

namespace PhotoCenterWeb.Controllers
{
    [RoutePrefix("api/users")]
    [Authorize(Roles = "Admin")]
    public class UsersApiController : ApiController
    {
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAll(
            string role = null,
            bool? isActive = null,
            int limit = 100)
        {
            using (var context = new PhotoCenterContext())
            {
                var query = context.Users
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrEmpty(role))
                    query = query.Where(u => u.Role == role);

                if (isActive.HasValue)
                    query = query.Where(u =>
                        u.IsActive == isActive.Value);

                var users = await query
                    .OrderBy(u => u.ID)
                    .Take(limit)
                    .ToListAsync();

                var userIds = users
                    .Select(u => u.ID)
                    .ToList();

                var clients = await context.Clients
                    .AsNoTracking()
                    .Where(c => userIds.Contains(c.UserID))
                    .ToDictionaryAsync(c => c.UserID);

                var employees = await context.Employees
                    .AsNoTracking()
                    .Where(e => userIds.Contains(e.UserID))
                    .ToDictionaryAsync(e => e.UserID);

                var result = users
                    .Select(u =>
                    {
                        Client client = null;
                        Employee employee = null;

                        clients.TryGetValue(u.ID, out client);
                        employees.TryGetValue(u.ID, out employee);

                        return new
                        {
                            id = u.ID,
                            login = u.Login,
                            firstName = u.FirstName ?? "",
                            lastName = u.LastName ?? "",
                            name = BuildFullName(
                                u.FirstName,
                                u.LastName
                            ),
                            role = u.Role,
                            isActive = u.IsActive,
                            mustChangePassword = u.MustChangePassword,
                            phone = client?.Phone ?? "",
                            email = client?.Email ?? "",
                            clientId = client != null
                                ? (int?)client.ID
                                : null,
                            employeeId = employee != null
                                ? (int?)employee.ID
                                : null
                        };
                    })
                    .ToList();

                return Ok(result);
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Create(
            [FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto == null)
                return BadRequest("Данные пользователя обязательны.");

            using (var context = new PhotoCenterContext())
            {
                var loginExists = await context.Users
                    .AnyAsync(u => u.Login == dto.Login);

                if (loginExists)
                {
                    return BadRequest(
                        "Пользователь с таким логином уже существует."
                    );
                }

                var nameParts = SplitName(dto.Name);

                var user = new User
                {
                    Login = dto.Login.Trim(),

                    PasswordHash =
                        BCryptLibrary.HashPassword("qwerty1234"),

                    FirstName = nameParts.FirstName,
                    LastName = nameParts.LastName,
                    Role = dto.Role,
                    IsActive = dto.IsActive ?? true,
                    MustChangePassword = true
                };

                context.Users.Add(user);

                if (dto.Role == "Client")
                {
                    context.Clients.Add(new Client
                    {
                        User = user,
                        Phone = dto.Phone,
                        Email = dto.Email,
                        RegistrationDate = DateTime.Now
                    });
                }
                else if (dto.Role == "Employee")
                {
                    context.Employees.Add(new Employee
                    {
                        User = user
                    });
                }

                await context.SaveChangesAsync();

                return Ok(new { id = user.ID });
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IHttpActionResult> Update(
            int id,
            [FromBody] UserUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto == null)
                return BadRequest("Данные пользователя обязательны.");

            using (var context = new PhotoCenterContext())
            {
                var user = await context.Users.FindAsync(id);

                if (user == null)
                    return NotFound();

                if (!string.IsNullOrWhiteSpace(dto.Login))
                {
                    var loginExists = await context.Users.AnyAsync(
                        u => u.ID != id && u.Login == dto.Login
                    );

                    if (loginExists)
                    {
                        return BadRequest(
                            "Пользователь с таким логином уже существует."
                        );
                    }

                    user.Login = dto.Login.Trim();
                }

                if (!string.IsNullOrWhiteSpace(dto.Role))
                    user.Role = dto.Role;

                if (dto.IsActive.HasValue)
                    user.IsActive = dto.IsActive.Value;

                if (dto.Name != null)
                {
                    var nameParts = SplitName(dto.Name);
                    user.FirstName = nameParts.FirstName;
                    user.LastName = nameParts.LastName;
                }

                var client = await context.Clients
                    .FirstOrDefaultAsync(c => c.UserID == user.ID);

                if (client != null)
                {
                    if (dto.Phone != null)
                        client.Phone = dto.Phone;

                    if (dto.Email != null)
                        client.Email = dto.Email;
                }

                await context.SaveChangesAsync();

                return Ok(new { id = user.ID });
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            using (var context = new PhotoCenterContext())
            {
                var user = await context.Users.FindAsync(id);

                if (user == null)
                    return NotFound();

                var client = await context.Clients
                    .FirstOrDefaultAsync(c => c.UserID == id);

                if (client != null)
                    context.Clients.Remove(client);

                var employee = await context.Employees
                    .FirstOrDefaultAsync(e => e.UserID == id);

                if (employee != null)
                    context.Employees.Remove(employee);

                context.Users.Remove(user);

                await context.SaveChangesAsync();

                return Ok();
            }
        }

        private static string BuildFullName(
            string firstName,
            string lastName)
        {
            return string.Join(
                " ",
                new[] { firstName, lastName }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
            );
        }

        private static NameParts SplitName(string name)
        {
            var parts = name.Trim().Split(
                new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries
            );

            if (parts.Length == 1)
            {
                return new NameParts
                {
                    FirstName = parts[0],
                    LastName = ""
                };
            }

            return new NameParts
            {
                FirstName = parts[0],
                LastName = string.Join(" ", parts.Skip(1))
            };
        }
    }

    internal class NameParts
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
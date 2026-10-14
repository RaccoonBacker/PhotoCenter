using System;
using System.Linq;
using System.Web.Http;
using BCryptLibrary = BCrypt.Net.BCrypt;
using PhotoCenterWeb.Models;

namespace PhotoCenterWeb.Controllers
{
    [RoutePrefix("api/users")]
    [Authorize(Roles = "Admin")]
    public class UsersApiController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(
            string role = null,
            bool? isActive = null,
            int limit = 100)
        {
            using (var context = new PhotoCenterContext())
            {
                var query = context.Users.AsQueryable();

                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(u => u.Role == role);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(
                        u => u.IsActive == isActive.Value
                    );
                }

                var users = query
                    .OrderBy(u => u.ID)
                    .Take(limit)
                    .ToList();

                var userIds = users
                    .Select(u => u.ID)
                    .ToList();

                var clients = context.Clients
                    .Where(c => userIds.Contains(c.UserID))
                    .ToDictionary(c => c.UserID);

                var employees = context.Employees
                    .Where(e => userIds.Contains(e.UserID))
                    .ToDictionary(e => e.UserID);

                var result = users
                    .Select(u =>
                    {
                        Client client = null;
                        Employee employee = null;

                        clients.TryGetValue(
                            u.ID,
                            out client
                        );

                        employees.TryGetValue(
                            u.ID,
                            out employee
                        );

                        return new
                        {
                            id = u.ID,
                            login = u.Login,

                            firstName =
                                u.FirstName ?? "",

                            lastName =
                                u.LastName ?? "",

                            name =
                                BuildFullName(
                                    u.FirstName,
                                    u.LastName
                                ),

                            role = u.Role,

                            isActive =
                                u.IsActive,

                            phone =
                                client != null
                                    ? client.Phone ?? ""
                                    : "",

                            email =
                                client != null
                                    ? client.Email ?? ""
                                    : "",

                            clientId =
                                client != null
                                    ? (int?)client.ID
                                    : null,

                            employeeId =
                                employee != null
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
        public IHttpActionResult Create(
            [FromBody] UserCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest(
                    "Данные пользователя обязательны."
                );
            }

            if (string.IsNullOrWhiteSpace(dto.Login))
            {
                return BadRequest(
                    "Логин обязателен."
                );
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(
                    "Имя пользователя обязательно."
                );
            }

            if (string.IsNullOrWhiteSpace(dto.Role))
            {
                return BadRequest(
                    "Роль пользователя обязательна."
                );
            }

            if (
                dto.Role != "Client" &&
                dto.Role != "Employee" &&
                dto.Role != "Admin"
            )
            {
                return BadRequest(
                    "Недопустимая роль пользователя."
                );
            }

            using (var context = new PhotoCenterContext())
            {
                var loginExists = context.Users
                    .Any(u => u.Login == dto.Login);

                if (loginExists)
                {
                    return BadRequest(
                        "Пользователь с таким логином уже существует."
                    );
                }

                var nameParts =
                    SplitName(dto.Name);

                var user = new User
                {
                    Login = dto.Login.Trim(),

                    PasswordHash =
                        BCryptLibrary.HashPassword(
                            "qwerty1234"
                        ),

                    FirstName =
                        nameParts.FirstName,

                    LastName =
                        nameParts.LastName,

                    Role = dto.Role,

                    IsActive =
                        dto.IsActive ?? true
                };

                context.Users.Add(user);

                context.SaveChanges();

                if (dto.Role == "Client")
                {
                    var client = new Client
                    {
                        UserID = user.ID,

                        Phone = dto.Phone,

                        Email = dto.Email,

                        RegistrationDate =
                            DateTime.Now
                    };

                    context.Clients.Add(client);
                }
                else if (dto.Role == "Employee")
                {
                    var employee = new Employee
                    {
                        UserID = user.ID
                    };

                    context.Employees.Add(employee);
                }

                context.SaveChanges();

                return Ok(new
                {
                    id = user.ID
                });
            }
        }

        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult Update(
            int id,
            [FromBody] UserUpdateDto dto)
        {
            if (dto == null)
            {
                return BadRequest(
                    "Данные пользователя обязательны."
                );
            }

            using (var context = new PhotoCenterContext())
            {
                var user =
                    context.Users.Find(id);

                if (user == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrWhiteSpace(dto.Login))
                {
                    var loginExists =
                        context.Users.Any(
                            u =>
                                u.ID != id &&
                                u.Login == dto.Login
                        );

                    if (loginExists)
                    {
                        return BadRequest(
                            "Пользователь с таким логином уже существует."
                        );
                    }

                    user.Login =
                        dto.Login.Trim();
                }

                if (!string.IsNullOrWhiteSpace(dto.Role))
                {
                    if (
                        dto.Role != "Client" &&
                        dto.Role != "Employee" &&
                        dto.Role != "Admin"
                    )
                    {
                        return BadRequest(
                            "Недопустимая роль пользователя."
                        );
                    }

                    user.Role = dto.Role;
                }

                if (dto.IsActive.HasValue)
                {
                    user.IsActive =
                        dto.IsActive.Value;
                }

                if (dto.Name != null)
                {
                    var nameParts =
                        SplitName(dto.Name);

                    user.FirstName =
                        nameParts.FirstName;

                    user.LastName =
                        nameParts.LastName;
                }

                var client =
                    context.Clients.FirstOrDefault(
                        c => c.UserID == user.ID
                    );

                if (client != null)
                {
                    if (dto.Phone != null)
                    {
                        client.Phone =
                            dto.Phone;
                    }

                    if (dto.Email != null)
                    {
                        client.Email =
                            dto.Email;
                    }
                }

                context.SaveChanges();

                return Ok(new
                {
                    id = user.ID
                });
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult Delete(int id)
        {
            using (var context = new PhotoCenterContext())
            {
                var user =
                    context.Users.Find(id);

                if (user == null)
                {
                    return NotFound();
                }

                var client =
                    context.Clients.FirstOrDefault(
                        c => c.UserID == id
                    );

                if (client != null)
                {
                    context.Clients.Remove(client);
                }

                var employee =
                    context.Employees.FirstOrDefault(
                        e => e.UserID == id
                    );

                if (employee != null)
                {
                    context.Employees.Remove(employee);
                }

                context.Users.Remove(user);

                context.SaveChanges();

                return Ok();
            }
        }

        private static string BuildFullName(
            string firstName,
            string lastName)
        {
            return string.Join(
                " ",
                new[]
                {
                    firstName,
                    lastName
                }
                .Where(
                    x =>
                        !string.IsNullOrWhiteSpace(x)
                )
            );
        }

        private static NameParts SplitName(
            string name)
        {
            var parts =
                name.Trim().Split(
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

                LastName =
                    string.Join(
                        " ",
                        parts.Skip(1)
                    )
            };
        }
    }

    public class UserCreateDto
    {
        public string Login { get; set; }

        public string Name { get; set; }

        public string Role { get; set; }

        public bool? IsActive { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }
    }

    public class UserUpdateDto
    {
        public string Login { get; set; }

        public string Name { get; set; }

        public string Role { get; set; }

        public bool? IsActive { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }
    }

    internal class NameParts
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
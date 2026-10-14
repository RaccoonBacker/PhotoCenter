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
                        Client client;

                        if (clients.TryGetValue(u.ID, out client))
                        {
                            return new
                            {
                                id = u.ID,
                                clientId = client.ID,
                                employeeId = 0,
                                login = u.Login,
                                role = u.Role,
                                isActive = u.IsActive,
                                firstName = client.FirstName ?? "",
                                lastName = client.LastName ?? "",
                                name = BuildFullName(
                                    client.FirstName,
                                    client.LastName
                                ),
                                phone = client.Phone ?? "",
                                email = client.Email ?? ""
                            };
                        }

                        Employee employee;

                        if (employees.TryGetValue(u.ID, out employee))
                        {
                            return new
                            {
                                id = u.ID,
                                clientId = 0,
                                employeeId = employee.ID,
                                login = u.Login,
                                role = u.Role,
                                isActive = u.IsActive,
                                firstName = employee.FirstName ?? "",
                                lastName = employee.LastName ?? "",
                                name = BuildFullName(
                                    employee.FirstName,
                                    employee.LastName
                                ),
                                phone = "",
                                email = ""
                            };
                        }

                        return new
                        {
                            id = u.ID,
                            clientId = 0,
                            employeeId = 0,
                            login = u.Login,
                            role = u.Role,
                            isActive = u.IsActive,
                            firstName = "",
                            lastName = "",
                            name = "",
                            phone = "",
                            email = ""
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
                return BadRequest("Данные пользователя обязательны.");
            }

            if (string.IsNullOrWhiteSpace(dto.Login))
            {
                return BadRequest("Логин обязателен.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Имя пользователя обязательно.");
            }

            if (string.IsNullOrWhiteSpace(dto.Role))
            {
                return BadRequest("Роль пользователя обязательна.");
            }

            if (dto.Role != "Client" &&
                dto.Role != "Employee" &&
                dto.Role != "Admin")
            {
                return BadRequest("Недопустимая роль пользователя.");
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

                var nameParts = SplitName(dto.Name);

                var user = new User
                {
                    Login = dto.Login.Trim(),
                    PasswordHash = BCryptLibrary.HashPassword(
                        "qwerty1234"
                    ),
                    Role = dto.Role,
                    IsActive = dto.IsActive ?? true
                };

                context.Users.Add(user);
                context.SaveChanges();

                if (dto.Role == "Client")
                {
                    var client = new Client
                    {
                        UserID = user.ID,
                        FirstName = nameParts.FirstName,
                        LastName = nameParts.LastName,
                        Phone = dto.Phone,
                        Email = dto.Email,
                        RegistrationDate = DateTime.Now
                    };

                    context.Clients.Add(client);
                }
                else if (dto.Role == "Employee")
                {
                    var employee = new Employee
                    {
                        UserID = user.ID,
                        FirstName = nameParts.FirstName,
                        LastName = nameParts.LastName
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
                return BadRequest("Данные пользователя обязательны.");
            }

            using (var context = new PhotoCenterContext())
            {
                var user = context.Users.Find(id);

                if (user == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrWhiteSpace(dto.Login))
                {
                    var loginExists = context.Users
                        .Any(u =>
                            u.ID != id &&
                            u.Login == dto.Login);

                    if (loginExists)
                    {
                        return BadRequest(
                            "Пользователь с таким логином уже существует."
                        );
                    }

                    user.Login = dto.Login.Trim();
                }

                if (!string.IsNullOrWhiteSpace(dto.Role))
                {
                    if (dto.Role != "Client" &&
                        dto.Role != "Employee" &&
                        dto.Role != "Admin")
                    {
                        return BadRequest(
                            "Недопустимая роль пользователя."
                        );
                    }

                    user.Role = dto.Role;
                }

                if (dto.IsActive.HasValue)
                {
                    user.IsActive = dto.IsActive.Value;
                }

                /*
                 * Главное исправление:
                 *
                 * Старые пользователи могли существовать только
                 * в таблице Users без записи в Clients/Employees.
                 *
                 * При редактировании создаём отсутствующий профиль,
                 * а затем сохраняем в него имя.
                 */

                if (!string.IsNullOrWhiteSpace(dto.Name))
                {
                    var nameParts = SplitName(dto.Name);

                    if (user.Role == "Client")
                    {
                        var client = context.Clients
                            .FirstOrDefault(
                                c => c.UserID == user.ID
                            );

                        if (client == null)
                        {
                            client = new Client
                            {
                                UserID = user.ID,
                                FirstName = nameParts.FirstName,
                                LastName = nameParts.LastName,
                                Phone = dto.Phone,
                                Email = dto.Email,
                                RegistrationDate = DateTime.Now
                            };

                            context.Clients.Add(client);
                        }
                        else
                        {
                            client.FirstName =
                                nameParts.FirstName;

                            client.LastName =
                                nameParts.LastName;

                            if (dto.Phone != null)
                            {
                                client.Phone = dto.Phone;
                            }

                            if (dto.Email != null)
                            {
                                client.Email = dto.Email;
                            }
                        }
                    }
                    else if (user.Role == "Employee")
                    {
                        var employee = context.Employees
                            .FirstOrDefault(
                                e => e.UserID == user.ID
                            );

                        if (employee == null)
                        {
                            employee = new Employee
                            {
                                UserID = user.ID,
                                FirstName = nameParts.FirstName,
                                LastName = nameParts.LastName
                            };

                            context.Employees.Add(employee);
                        }
                        else
                        {
                            employee.FirstName =
                                nameParts.FirstName;

                            employee.LastName =
                                nameParts.LastName;
                        }
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
                var user = context.Users.Find(id);

                if (user == null)
                {
                    return NotFound();
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
                .Where(x => !string.IsNullOrWhiteSpace(x))
            );
        }

        private static NameParts SplitName(string name)
        {
            var parts = name
                .Trim()
                .Split(
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
                LastName = string.Join(
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
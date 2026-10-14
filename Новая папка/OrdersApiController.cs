using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using PhotoCenterWeb.Models;

namespace PhotoCenterWeb.Controllers
{
    [RoutePrefix("api/orders")]
    [Authorize(Roles = "Admin")]
    public class OrdersApiController : ApiController
    {
        private static readonly string[] AllowedStatuses =
        {
            "New",
            "InProgress",
            "Ready",
            "Completed",
            "Cancelled"
        };

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(
            string status = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int limit = 100)
        {
            using (var context = new PhotoCenterContext())
            {
                var query = context.Orders.AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(
                        o => o.Status == status
                    );
                }

                if (dateFrom.HasValue)
                {
                    query = query.Where(
                        o => o.CreatedDate >= dateFrom.Value
                    );
                }

                if (dateTo.HasValue)
                {
                    query = query.Where(
                        o => o.CreatedDate <= dateTo.Value
                    );
                }

                var orders = query
                    .OrderByDescending(
                        o => o.CreatedDate
                    )
                    .Take(limit)
                    .ToList();

                var clientIds = orders
                    .Select(o => o.ClientID)
                    .Distinct()
                    .ToList();

                var employeeIds = orders
                    .Where(o => o.EmployeeID.HasValue)
                    .Select(o => o.EmployeeID.Value)
                    .Distinct()
                    .ToList();

                var clients = context.Clients
                    .Where(c => clientIds.Contains(c.ID))
                    .ToDictionary(c => c.ID);

                var employees = context.Employees
                    .Where(e => employeeIds.Contains(e.ID))
                    .ToDictionary(e => e.ID);

                var clientUserIds = clients.Values
                    .Select(c => c.UserID)
                    .Distinct()
                    .ToList();

                var employeeUserIds = employees.Values
                    .Select(e => e.UserID)
                    .Distinct()
                    .ToList();

                var userIds = clientUserIds
                    .Concat(employeeUserIds)
                    .Distinct()
                    .ToList();

                var users = context.Users
                    .Where(u => userIds.Contains(u.ID))
                    .ToDictionary(u => u.ID);

                var result = orders
                    .Select(o =>
                    {
                        Client client = null;

                        clients.TryGetValue(
                            o.ClientID,
                            out client
                        );

                        Employee employee = null;

                        if (o.EmployeeID.HasValue)
                        {
                            employees.TryGetValue(
                                o.EmployeeID.Value,
                                out employee
                            );
                        }

                        string clientLogin = "";

                        if (client != null)
                        {
                            User clientUser;

                            if (users.TryGetValue(
                                client.UserID,
                                out clientUser))
                            {
                                clientLogin =
                                    clientUser.Login;
                            }
                        }

                        string clientName = "";

                        if (client != null && client.User != null)
                        {
                            clientName =
                                BuildFullName(
                                    client.User.FirstName,
                                    client.User.LastName
                                );
                        }

                        string employeeName = "";

                        if (employee != null && employee.User != null)
                        {
                            employeeName =
                                BuildFullName(
                                    employee.User.FirstName,
                                    employee.User.LastName
                                );
                        }

                        return new
                        {
                            id = o.ID,

                            clientId =
                                o.ClientID,

                            clientName =
                                clientName,

                            clientLogin =
                                clientLogin,

                            employeeId =
                                o.EmployeeID,

                            employeeName =
                                employeeName,

                            createdDate =
                                o.CreatedDate,

                            status =
                                o.Status,

                            totalPrice =
                                o.TotalPrice
                        };
                    })
                    .ToList();

                return Ok(result);
            }
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Create(
            [FromBody] OrderCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest(
                    "Данные заказа обязательны."
                );
            }

            if (dto.ClientID <= 0)
            {
                return BadRequest(
                    "Необходимо выбрать клиента."
                );
            }

            if (!dto.EmployeeID.HasValue ||
                dto.EmployeeID.Value <= 0)
            {
                return BadRequest(
                    "Необходимо выбрать исполнителя."
                );
            }

            if (string.IsNullOrWhiteSpace(dto.Status) ||
                !AllowedStatuses.Contains(dto.Status))
            {
                return BadRequest(
                    "Недопустимый статус заказа."
                );
            }

            if (dto.TotalPrice < 0)
            {
                return BadRequest(
                    "Сумма заказа не может быть отрицательной."
                );
            }

            using (var context = new PhotoCenterContext())
            {
                var clientExists = context.Clients
                    .Any(c => c.ID == dto.ClientID);

                if (!clientExists)
                {
                    return BadRequest(
                        "Выбранный клиент не существует."
                    );
                }

                var employeeExists = context.Employees
                    .Any(e => e.ID == dto.EmployeeID.Value);

                if (!employeeExists)
                {
                    return BadRequest(
                        "Выбранный исполнитель не существует."
                    );
                }

                var order = new Order
                {
                    ClientID = dto.ClientID,

                    EmployeeID =
                        dto.EmployeeID.Value,

                    CreatedDate =
                        dto.CreatedDate ?? DateTime.Now,

                    Status =
                        dto.Status,

                    TotalPrice =
                        dto.TotalPrice
                };

                context.Orders.Add(order);
                context.SaveChanges();

                return Ok(new
                {
                    id = order.ID
                });
            }
        }

        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult Update(
            int id,
            [FromBody] OrderUpdateDto dto)
        {
            if (dto == null)
            {
                return BadRequest(
                    "Данные заказа обязательны."
                );
            }

            using (var context = new PhotoCenterContext())
            {
                var order = context.Orders.Find(id);

                if (order == null)
                {
                    return NotFound();
                }

                if (dto.ClientID.HasValue)
                {
                    var clientExists = context.Clients
                        .Any(
                            c => c.ID == dto.ClientID.Value
                        );

                    if (!clientExists)
                    {
                        return BadRequest(
                            "Выбранный клиент не существует."
                        );
                    }

                    order.ClientID =
                        dto.ClientID.Value;
                }

                if (dto.EmployeeID.HasValue)
                {
                    var employeeExists =
                        context.Employees.Any(
                            e => e.ID ==
                                 dto.EmployeeID.Value
                        );

                    if (!employeeExists)
                    {
                        return BadRequest(
                            "Выбранный исполнитель не существует."
                        );
                    }

                    order.EmployeeID =
                        dto.EmployeeID.Value;
                }

                if (dto.CreatedDate.HasValue)
                {
                    order.CreatedDate =
                        dto.CreatedDate.Value;
                }

                if (dto.TotalPrice.HasValue)
                {
                    if (dto.TotalPrice.Value < 0)
                    {
                        return BadRequest(
                            "Сумма заказа не может быть отрицательной."
                        );
                    }

                    order.TotalPrice =
                        dto.TotalPrice.Value;
                }

                if (!string.IsNullOrWhiteSpace(
                    dto.Status))
                {
                    if (!AllowedStatuses.Contains(
                        dto.Status))
                    {
                        return BadRequest(
                            "Недопустимый статус заказа."
                        );
                    }

                    order.Status =
                        dto.Status;
                }

                context.SaveChanges();

                return Ok(new
                {
                    id = order.ID
                });
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult Delete(int id)
        {
            using (var context = new PhotoCenterContext())
            {
                var order = context.Orders.Find(id);

                if (order == null)
                {
                    return NotFound();
                }

                context.Orders.Remove(order);
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
                    lastName,
                    firstName
                }
                .Where(
                    x => !string.IsNullOrWhiteSpace(x)
                )
            );
        }
    }

    public class OrderCreateDto
    {
        public int ClientID { get; set; }

        public int? EmployeeID { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string Status { get; set; }

        public decimal TotalPrice { get; set; }
    }

    public class OrderUpdateDto
    {
        public int? ClientID { get; set; }

        public int? EmployeeID { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string Status { get; set; }

        public decimal? TotalPrice { get; set; }
    }
}
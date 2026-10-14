using System;
using System.Linq;
using System.Web.Http;
using PhotoCenterWeb.Models;

namespace PhotoCenterWeb.Controllers
{
    [RoutePrefix("api/analytics")]
    [Authorize(Roles = "Admin")]
    public class AnalyticsApiController : ApiController
    {
        [HttpGet]
        [Route("statuses")]
        public IHttpActionResult GetOrderStatuses()
        {
            using (var context = new PhotoCenterContext())
            {
                var data = context.Orders
                    .GroupBy(o => o.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .ToList();

                return Ok(data);
            }
        }

        [HttpGet]
        [Route("revenue")]
        public IHttpActionResult GetRevenueByMonth()
        {
            using (var context = new PhotoCenterContext())
            {
                var sixMonthsAgo =
                    DateTime.Now.AddMonths(-6);

                var data = context.Orders
                    .Where(o =>
                        o.CreatedDate >= sixMonthsAgo)
                    .ToList()
                    .GroupBy(o => new
                    {
                        o.CreatedDate.Year,
                        o.CreatedDate.Month
                    })
                    .Select(g => new
                    {
                        Month =
                            $"{g.Key.Month:D2}.{g.Key.Year}",

                        Revenue =
                            g.Sum(o => o.TotalPrice)
                    })
                    .OrderBy(x => x.Month)
                    .ToList();

                return Ok(data);
            }
        }

        [HttpGet]
        [Route("top-services")]
        public IHttpActionResult GetTopServices()
        {
            using (var context = new PhotoCenterContext())
            {
                var data = context.OrderDetails
                    .Where(od => od.ServiceID != null)
                    .GroupBy(od => od.ServiceID)
                    .Select(g => new
                    {
                        ServiceID = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList();

                var result = data
                    .Select(d => new
                    {
                        ServiceName =
                            context.Services
                                .Where(s =>
                                    s.ID == d.ServiceID)
                                .Select(s => s.Name)
                                .FirstOrDefault(),

                        Count = d.Count
                    })
                    .ToList();

                return Ok(result);
            }
        }
    }
}
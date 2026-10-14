using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<IHttpActionResult> GetOrderStatuses()
        {
            using (var context = new PhotoCenterContext())
            {
                var data = await context.Orders
                    .AsNoTracking()
                    .GroupBy(o => o.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                return Ok(data);
            }
        }

        [HttpGet]
        [Route("revenue")]
        public async Task<IHttpActionResult> GetRevenueByMonth()
        {
            using (var context = new PhotoCenterContext())
            {
                var sixMonthsAgo = DateTime.Now.AddMonths(-6);

                // В SQL только фильтрация и выборка нужных столбцов.
                var orders = await context.Orders
                    .AsNoTracking()
                    .Where(o => o.CreatedDate >= sixMonthsAgo)
                    .Select(o => new
                    {
                        o.CreatedDate,
                        o.TotalPrice
                    })
                    .ToListAsync();

                // GroupBy и Sum в памяти.
                var data = orders
                    .GroupBy(o => new
                    {
                        o.CreatedDate.Year,
                        o.CreatedDate.Month
                    })
                    .Select(g => new
                    {
                        Month = $"{g.Key.Month:D2}.{g.Key.Year}",
                        Revenue = g.Sum(o => o.TotalPrice)
                    })
                    .OrderBy(x => x.Month)
                    .ToList();

                return Ok(data);
            }
        }

        [HttpGet]
        [Route("top-services")]
        public async Task<IHttpActionResult> GetTopServices()
        {
            using (var context = new PhotoCenterContext())
            {
                var data = await context.OrderDetails
                    .AsNoTracking()
                    .Where(od => od.ServiceID != null)
                    .GroupBy(od => od.ServiceID)
                    .Select(g => new
                    {
                        ServiceID = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToListAsync();

                var serviceIds = data
                    .Select(d => d.ServiceID.Value)
                    .ToList();

                var serviceNames = await context.Services
                    .AsNoTracking()
                    .Where(s => serviceIds.Contains(s.ID))
                    .ToDictionaryAsync(s => s.ID, s => s.Name);

                var result = data
                    .Select(d => new
                    {
                        ServiceName = serviceNames.TryGetValue(
                            d.ServiceID.Value,
                            out var name
                        ) ? name : null,

                        d.Count
                    })
                    .ToList();

                return Ok(result);
            }
        }
    }
}
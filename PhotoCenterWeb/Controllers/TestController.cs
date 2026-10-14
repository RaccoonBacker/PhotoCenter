using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using PhotoCenterWeb.Models;

namespace PhotoCenterWeb.Controllers
{
    public class TestController : Controller
    {
        public async Task<ActionResult> Index()
        {
            using (var context = new PhotoCenterContext())
            {
                ViewBag.UsersCount = await context.Users
                    .AsNoTracking()
                    .CountAsync();

                ViewBag.ServicesCount = await context.Services
                    .AsNoTracking()
                    .CountAsync();

                ViewBag.OrdersCount = await context.Orders
                    .AsNoTracking()
                    .CountAsync();

                return View();
            }
        }
    }
}
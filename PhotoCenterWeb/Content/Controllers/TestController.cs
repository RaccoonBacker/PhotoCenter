using System.Linq;
using System.Web.Mvc;
using PhotoCenterWeb.Models;

namespace PhotoCenterWeb.Controllers
{
    public class TestController : Controller
    {
        public ActionResult Index()
        {
            using (var context = new PhotoCenterContext())
            {
                var users = context.Users.ToList();
                var services = context.Services.ToList();
                var orders = context.Orders.ToList();

                ViewBag.UsersCount = users.Count;
                ViewBag.ServicesCount = services.Count;
                ViewBag.OrdersCount = orders.Count;

                return View();
            }
        }
    }
}
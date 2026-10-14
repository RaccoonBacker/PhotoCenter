using System.IO;
using System.Web.Mvc;

namespace PhotoCenterWeb.Controllers
{
    public class ReactController : Controller
    {
        public ActionResult Index()
        {
            var path = Server.MapPath("~/Scripts/index.html");

            if (!System.IO.File.Exists(path))
            {
                return HttpNotFound("React build not found. Run npm run build.");
            }

            return File(path, "text/html");
        }
    }
}
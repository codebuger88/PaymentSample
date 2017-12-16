using System.Web.Mvc;

namespace ECPay.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Finished()
        {
            return View();
        }
    }
}
using System.Web.Mvc;

namespace CognitiveServicesDemo.Areas.ComputerVision.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Describe");
        }
    }
}
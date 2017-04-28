using System.Web.Mvc;

namespace CognitiveServicesDemo.Areas.Faces.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Detect");
        }
    }
}
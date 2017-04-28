using System.Configuration;
using System.Web.Mvc;
using Microsoft.ProjectOxford.Face;

namespace CognitiveServicesDemo.Areas.Faces.Controllers
{
    public abstract class FacesBaseController : Controller
    {
        [NonAction]
        protected FaceServiceClient GetFaceClient()
        {
            var apiKey = ConfigurationManager.AppSettings["CognitiveServicesKey"];

            return new FaceServiceClient(apiKey);
        }
    }
}
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.ProjectOxford.Vision;

namespace CognitiveServicesDemo.Areas.ComputerVision.Controllers
{
    public class HomeController : ImageUsingBaseController
    {
        protected VisionServiceClient VisionServiceClient { get; private set; }

        public HomeController()
        {
            var apiKey = ConfigurationManager.AppSettings["CognitiveServicesVisionApiKey"];
            var apiRoot = ConfigurationManager.AppSettings["CognitiveServicesVisionApiUrl"];
            VisionServiceClient = new VisionServiceClient(apiKey, apiRoot);
        }

        public async Task<ActionResult> Index()
        {
            ViewBag.Title = "Detect faces";

            if (Request.HttpMethod == "GET")
            {
                return View();
            }

            string imageResult = "";

            await RunOperationOnImage(async stream =>
            {
                var visualFeatures = new[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color,
                                             VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.ImageType, 
                                             VisualFeature.Tags };
                
                await VisionServiceClient.AnalyzeImageAsync(stream, visualFeatures);
            });

            return View((object)imageResult);
        }
    }
}
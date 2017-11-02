using System.Threading.Tasks;
using System.Web.Mvc;
using CognitiveServicesDemo.Areas.ComputerVision.Models;

namespace CognitiveServicesDemo.Areas.ComputerVision.Controllers
{
    public class TextController : ComputerVisionBaseController
    {
        public async Task<ActionResult> Index()
        {
            if (Request.HttpMethod == "GET")
            {
                return View();
            }
            
            var model = new RecognizeTextModel();
            
            await RunOperationOnImage(async stream =>
            {
                model.Results = await VisionServiceClient.RecognizeTextAsync(stream, detectOrientation: false);
            });

            model.ImageDump = GetInlineImageWithLines(model.Results);

            return View(model);
        }
    }
}
using System;
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

            await RunOperationOnImage(async stream => {
                var bytes = new byte[stream.Length];

                stream.Read(bytes, 0, bytes.Length);

                var base64 = Convert.ToBase64String(bytes);
                model.ImageDump = String.Format("data:image/png;base64,{0}", base64);
            });

            return View(model);
        }
    }
}
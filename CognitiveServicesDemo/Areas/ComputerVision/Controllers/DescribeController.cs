using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using CognitiveServicesDemo.Areas.ComputerVision.Models;
using Microsoft.ProjectOxford.Vision;

namespace CognitiveServicesDemo.Areas.ComputerVision.Controllers
{
    public class DescribeController : ComputerVisionBaseController
    {
        public async Task<ActionResult> Index()
        {
            if (Request.HttpMethod == "GET")
            {
                return View("Index");
            }

            var model = new DescribeImageModel();

            // What features to find on imahe
            var features = new[]
            {
                VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description,
                VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags
            };

            // Analyze image using copy of input stream
            await RunOperationOnImage(async stream =>
            {
                model.Result = await VisionServiceClient.AnalyzeImageAsync(stream, features);
            });

            // Convert image to base64 image string
            await RunOperationOnImage(async stream => {
                var bytes = new byte[stream.Length];

                await stream.ReadAsync(bytes, 0, bytes.Length);

                var base64 = Convert.ToBase64String(bytes);
                model.ImageDump = String.Format("data:image/png;base64,{0}", base64);
            });            
            
            return View(model);
        }
    }
}
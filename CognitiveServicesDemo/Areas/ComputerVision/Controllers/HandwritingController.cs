using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CognitiveServicesDemo.Areas.ComputerVision.Models;
using Microsoft.ProjectOxford.Vision.Contract;

namespace CognitiveServicesDemo.Areas.ComputerVision.Controllers
{
    public class HandwritingController : ComputerVisionBaseController
    {        
        public async Task<ActionResult> Index()
        {
            if (Request.HttpMethod == "GET")
            {
                return View();
            }

            var model = new HandwritingModel();
            HandwritingRecognitionOperation op = null;

            await RunOperationOnImage(async stream =>
            {
                op = await VisionServiceClient.CreateHandwritingRecognitionOperationAsync(stream);
            });

            while(true)
            {
                await Task.Delay(5000);

                var result = await VisionServiceClient.GetHandwritingRecognitionOperationResultAsync(op);
                if(result.Status == HandwritingRecognitionOperationStatus.NotStarted || 
                   result.Status == HandwritingRecognitionOperationStatus.Running)
                {
                    continue;
                }

                model.Result = result.RecognitionResult;
                break;
            }

            await RunOperationOnImage(async stream =>
            {
                var bytes = new byte[stream.Length];

                stream.Read(bytes, 0, bytes.Length);

                var base64 = Convert.ToBase64String(bytes);
                model.ImageDump = String.Format("data:image/png;base64,{0}", base64);
            });

            return View(model);
        }
    }
}
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CognitiveServicesDemo.Models;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

namespace CognitiveServicesDemo.Areas.ComputerVision.Controllers
{
    public abstract class ComputerVisionBaseController : ImageUsingBaseController
    {
        protected VisionServiceClient VisionServiceClient { get; private set; }

        public ComputerVisionBaseController()
        {
        var apiKey = ConfigurationManager.AppSettings["CognitiveServicesVisionApiKey"];
        var apiRoot = ConfigurationManager.AppSettings["CognitiveServicesVisionApiUrl"];
        VisionServiceClient = new VisionServiceClient(apiKey, apiRoot);
        }

        protected string GetInlineImageWithLines(HandwritingTextResult result)
        {
            ImageToProcess.Seek(0, SeekOrigin.Begin);

            using (var img = new Bitmap(ImageToProcess))
            // make copy, drawing on indexed pixel format image is not supported
            using (var nonIndexedImg = new Bitmap(img.Width, img.Height))
            using (var g = Graphics.FromImage(nonIndexedImg))
            using (var mem = new MemoryStream())
            {
                g.DrawImage(img, 0, 0, img.Width, img.Height);

                var i = 0;

                foreach (var line in result.Lines)
                {
                    if(i >= Settings.ImageSquareColors.Length)
                    {
                        i = 0;
                    }
                    var pen = new Pen(Settings.ImageSquareColors[i], 5);
                    var points = line.Polygon.Points.Select(pp => new System.Drawing.Point
                    {
                        X = pp.X,
                        Y = pp.Y
                    }).ToArray();

                    g.DrawPolygon(pen, points);
                    i++;
                }

                nonIndexedImg.Save(mem, ImageFormat.Png);

                var base64 = Convert.ToBase64String(mem.ToArray());
                return String.Format("data:image/png;base64,{0}", base64);
            }
        }

        protected string GetInlineImageWithLines(OcrResults result)
        {
            ImageToProcess.Seek(0, SeekOrigin.Begin);

            using (var img = new Bitmap(ImageToProcess))
            // make copy, drawing on indexed pixel format image is not supported
            using (var nonIndexedImg = new Bitmap(img.Width, img.Height))
            using (var g = Graphics.FromImage(nonIndexedImg))
            using (var mem = new MemoryStream())
            {
                g.DrawImage(img, 0, 0, img.Width, img.Height);

                var i = 0;

                foreach (var region in result.Regions)
                foreach (var line in region.Lines)
                {
                    var pen = new Pen(Settings.ImageSquareColors[i], 2);
                    g.DrawRectangle(pen, new System.Drawing.Rectangle(
                            line.Rectangle.Left,
                            line.Rectangle.Top,
                            line.Rectangle.Width,
                            line.Rectangle.Height
                        ));
                    i++;
                    if(i >= 10)
                    {
                        i = 0;
                    }
                }

                nonIndexedImg.Save(mem, ImageFormat.Png);

                var base64 = Convert.ToBase64String(mem.ToArray());
                return String.Format("data:image/png;base64,{0}", base64);
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            ViewBag.LeftMenu = "_ComputerVisionMenu";
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);

            if (filterContext.ExceptionHandled)
            {
                return;
            }

            var message = filterContext.Exception.Message;
            var code = "";

            if (filterContext.Exception is ClientException)
            {
                var faex = filterContext.Exception as ClientException;
                message = faex.Error.Message;
                code = faex.Error.Code;
            }

            filterContext.Result = new ViewResult
            {
                ViewName = "Error",
                ViewData = new ViewDataDictionary(filterContext.Controller.ViewData)
                {
                    Model = new ErrorModel { Code = code, Message = message }
                }
            };

            filterContext.ExceptionHandled = true;
        }
    }
}
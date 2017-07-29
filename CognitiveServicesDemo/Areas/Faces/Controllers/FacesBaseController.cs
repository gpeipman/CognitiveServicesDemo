using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using CognitiveServicesDemo.Models;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace CognitiveServicesDemo.Areas.Faces.Controllers
{
    public abstract class FacesBaseController : Controller
    {
        protected FaceServiceClient FaceClient { get; private set; }

        private Stream _imageToProcess = new MemoryStream();

        public FacesBaseController()
        { 
            var apiKey = ConfigurationManager.AppSettings["CognitiveServicesFaceApiKey"];
            var apiRoot = ConfigurationManager.AppSettings["CognitiveServicesFaceApiUrl"];
            FaceClient = new FaceServiceClient(apiKey, apiRoot);
        }

        protected void ResizeImage(Stream fromStream, Stream toStream)
        {
            var image = Image.FromStream(fromStream);

            if(image.Width <= 600)
            {
                fromStream.CopyTo(toStream);
                image.Dispose();
                return;
            }
            var scaleFactor = 600 / (double)image.Width;
            var newWidth = 600;
            var newHeight = (int)(image.Height * scaleFactor);
            var thumbnailBitmap = new Bitmap(newWidth, newHeight);

            var thumbnailGraph = Graphics.FromImage(thumbnailBitmap);
            thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
            thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
            thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
            thumbnailGraph.DrawImage(image, imageRectangle);

            thumbnailBitmap.Save(toStream, image.RawFormat);

            thumbnailGraph.Dispose();
            thumbnailBitmap.Dispose();
            image.Dispose();

            toStream.Seek(0, SeekOrigin.Begin);
        }

        protected string GetInlineImageWithFaces(IEnumerable<Face> faces)
        {
            _imageToProcess.Seek(0, SeekOrigin.Begin);

            using (var img = new Bitmap(_imageToProcess))
            // make copy, drawing on indexed pixel format image is not supported
            using (var nonIndexedImg = new Bitmap(img.Width, img.Height))
            using (var g = Graphics.FromImage(nonIndexedImg))
            using (var mem = new MemoryStream())
            {
                g.DrawImage(img, 0, 0, img.Width, img.Height);

                var pen = new Pen(Color.Red, 5);

                foreach (var face in faces)
                {
                    var faceRectangle = face.FaceRectangle;
                    var rectangle = new Rectangle(faceRectangle.Left,
                                                  faceRectangle.Top,
                                                  faceRectangle.Width,
                                                  faceRectangle.Height);

                    g.DrawRectangle(pen, rectangle);
                }

                nonIndexedImg.Save(mem, ImageFormat.Png);

                var base64 = Convert.ToBase64String(mem.ToArray());
                return String.Format("data:image/png;base64,{0}", base64);
            }
        }

        protected async Task RunOperationOnImage(Func<Stream, Task> func)
        {
            _imageToProcess.Seek(0, SeekOrigin.Begin);

            using (var temporaryStream = new MemoryStream())
            {
                _imageToProcess.CopyTo(temporaryStream);
                temporaryStream.Seek(0, SeekOrigin.Begin);
                await func(temporaryStream);
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if(Request.Files.Count > 0)
            {
                ResizeImage(Request.Files[0].InputStream, _imageToProcess);
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);

            if(filterContext.ExceptionHandled)
            {
                return;
            }

            var message = filterContext.Exception.Message;
            var code = "";

            if (filterContext.Exception is FaceAPIException)
            {
                var faex = filterContext.Exception as FaceAPIException;
                message = faex.ErrorMessage;
                code = faex.ErrorCode;
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(FaceClient != null)
            {
                FaceClient.Dispose();
                FaceClient = null;
            }

            if(_imageToProcess != null)
            {
                _imageToProcess.Dispose();
                _imageToProcess = null;
            }
        }
    }
}
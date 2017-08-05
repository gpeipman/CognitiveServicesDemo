using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CognitiveServicesDemo.Areas
{
    public abstract class ImageUsingBaseController : Controller
    {
        protected Stream ImageToProcess = new MemoryStream();

        protected void ResizeImage(Stream fromStream, Stream toStream)
        {
            var image = Image.FromStream(fromStream);

            if (image.Width <= 1200)
            {
                fromStream.Seek(0, SeekOrigin.Begin);
                fromStream.CopyTo(toStream);
                image.Dispose();
                return;
            }
            var scaleFactor = 1200 / (double)image.Width;
            var newWidth = 1200;
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

        protected async Task RunOperationOnImage(Func<Stream, Task> func)
        {
            ImageToProcess.Seek(0, SeekOrigin.Begin);

            using (var temporaryStream = new MemoryStream())
            {
                ImageToProcess.CopyTo(temporaryStream);
                temporaryStream.Seek(0, SeekOrigin.Begin);
                await func(temporaryStream);
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (Request.Files.Count > 0)
            {
                ResizeImage(Request.Files[0].InputStream, ImageToProcess);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (ImageToProcess != null)
            {
                ImageToProcess.Dispose();
                ImageToProcess = null;
            }
        }
    }
}
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.ProjectOxford.Face.Contract;

namespace CognitiveServicesDemo.Areas.Faces.Controllers
{
    public class DetectController : FacesBaseController
    {
        public async Task<ActionResult> Index()
        {
            if (Request.HttpMethod == "GET")
            {
                return View();
            }

            var imageResult = "";
            var file = Request.Files[0];
            Face[] faces;

            // input stream will be actually disposed by client
            using (var client = GetFaceClient())
            using (var analyzeCopyBuffer = new MemoryStream())
            {
                file.InputStream.CopyTo(analyzeCopyBuffer);
                file.InputStream.Seek(0, SeekOrigin.Begin);
                analyzeCopyBuffer.Seek(0, SeekOrigin.Begin);
                faces = await client.DetectAsync(analyzeCopyBuffer);
            }

            using (var img = new Bitmap(file.InputStream))
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
                imageResult = String.Format("data:image/png;base64,{0}", base64);
            }

            return View((object)imageResult);
        }
    }
}
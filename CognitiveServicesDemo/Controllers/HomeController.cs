using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using CognitiveServicesDemo.Extensions.CognitiveServices;

namespace CognitiveServicesDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        public async Task<ActionResult> DetectFaces()
        {
            if(Request.HttpMethod == "GET")
            {
                return View();
            }

            var imageResult = "";
            var file = Request.Files[0];
            IList<DetectedFace> faces = null;            

            using (var analyzeCopyBuffer = new MemoryStream()) // will be disposed by stream content
            {
                file.InputStream.CopyTo(analyzeCopyBuffer);
                file.InputStream.Seek(0, SeekOrigin.Begin);
                faces = await CognitiveServicesClient.DetectFaces(analyzeCopyBuffer);
            }
            

            using (var img = new Bitmap(file.InputStream))
            using (var nonIndexedImg = new Bitmap(img.Width, img.Height)) // drawing on indexed pixel format image not supported
            using (var g = Graphics.FromImage(nonIndexedImg))
            using (var mem = new MemoryStream())
            {
                g.DrawImage(img, 0, 0, img.Width, img.Height);

                var pen = new Pen(Color.Red, 5);

                foreach (var face in faces)
                {
                    var rectangle = face.FaceRectangle.ToRectangle();

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
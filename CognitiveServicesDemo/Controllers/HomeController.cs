using System;
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

            var file = Request.Files[0];
            var mem2 = new MemoryStream();
            var mem3 = new MemoryStream();
            file.InputStream.CopyTo(mem2);
            file.InputStream.Seek(0, SeekOrigin.Begin);
            file.InputStream.CopyTo(mem3);
            mem2.Seek(0, SeekOrigin.Begin);
            var result = await CognitiveServicesClient.DetectFaces(mem2);
            var imageResult = "";

            using (var img = new Bitmap(mem3))
            using (var nonIndexedImg = new Bitmap(img.Width, img.Height))
            using (var g = Graphics.FromImage(nonIndexedImg))
            using (var mem = new MemoryStream())
            {
                g.DrawImage(img, 0, 0, img.Width, img.Height);

                var pen = new Pen(Color.Red, 5);

                foreach (var face in result)
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
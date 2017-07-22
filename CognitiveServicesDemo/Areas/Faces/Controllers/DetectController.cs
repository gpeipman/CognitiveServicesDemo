using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using CognitiveServicesDemo.Areas.Faces.Models;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace CognitiveServicesDemo.Areas.Faces.Controllers
{
    public class DetectController : FacesBaseController
    {
        private readonly FaceServiceClient _client;

        public DetectController()
        {
            _client = GetFaceClient();
        }

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
            using (var analyzeCopyBuffer = new MemoryStream())
            {
                file.InputStream.CopyTo(analyzeCopyBuffer);
                file.InputStream.Seek(0, SeekOrigin.Begin);
                analyzeCopyBuffer.Seek(0, SeekOrigin.Begin);
                faces = await _client.DetectAsync(analyzeCopyBuffer);
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

        public async Task<ActionResult> Identify()
        {
            var personGroupId = Request["PersonGroupId"];
            var model = new IdentifyFacesModel();

            var groups = await _client.ListPersonGroupsAsync();
            model.PersonGroups = groups.Select(g => new SelectListItem
                                                {
                                                    Value = g.PersonGroupId,
                                                    Text = g.Name
                                                }).ToList();

            if (Request.HttpMethod == "GET")
            {                                   
                return View(model);
            }

            var file = Request.Files[0];
            Face[] faces;
            IdentifyResult[] results;

            try
            {
                // input stream will be actually disposed by client
                using (var analyzeCopyBuffer = new MemoryStream())
                {
                    file.InputStream.CopyTo(analyzeCopyBuffer);
                    file.InputStream.Seek(0, SeekOrigin.Begin);
                    analyzeCopyBuffer.Seek(0, SeekOrigin.Begin);
                    faces = await _client.DetectAsync(analyzeCopyBuffer);
                    var faceIds = faces.Select(f => f.FaceId).ToArray();

                    if(faceIds.Length == 0)
                    {
                        model.Error = "No faces detected";
                        return View(model);
                    }

                    results = await _client.IdentifyAsync(personGroupId, faceIds);
                }
            }
            catch(FaceAPIException faex)
            {
                model.Error = faex.ErrorMessage;
                return View(model);
            }
            catch(Exception ex)
            {
                model.Error = ex.Message;
                return View(model);
            }

            foreach(var result in results)
            {
                var identifiedFace = new IdentifiedFace();
                identifiedFace.Face = faces.FirstOrDefault(f => f.FaceId == result.FaceId);
                
                foreach(var candidate in result.Candidates)
                {
                    var person = await _client.GetPersonAsync(personGroupId, candidate.PersonId);

                    identifiedFace.PersonCandidates.Add(person.PersonId, person.Name);
                }

                identifiedFace.Color = Settings.ImageSquareColors[model.IdentifiedFaces.Count];
                model.IdentifiedFaces.Add(identifiedFace);
            }

            using (var img = new Bitmap(file.InputStream))
            // make copy, drawing on indexed pixel format image is not supported
            using (var nonIndexedImg = new Bitmap(img.Width, img.Height))
            using (var g = Graphics.FromImage(nonIndexedImg))
            using (var mem = new MemoryStream())
            {
                g.DrawImage(img, 0, 0, img.Width, img.Height);                

                foreach (var face in model.IdentifiedFaces)
                {
                    var faceRectangle = face.Face.FaceRectangle;
                    var pen = new Pen(face.Color, 5);
                    var rectangle = new Rectangle(faceRectangle.Left,
                                                  faceRectangle.Top,
                                                  faceRectangle.Width,
                                                  faceRectangle.Height);

                    g.DrawRectangle(pen, rectangle);
                }

                nonIndexedImg.Save(mem, ImageFormat.Png);

                var base64 = Convert.ToBase64String(mem.ToArray());
                model.ImageDump = String.Format("data:image/png;base64,{0}", base64);
            }

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                return;

            _client.Dispose();
        }
    }
}
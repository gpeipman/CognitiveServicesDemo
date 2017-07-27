using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using CognitiveServicesDemo.Areas.Faces.Models;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace CognitiveServicesDemo.Areas.Faces.Controllers
{
    public class DetectController : FacesBaseController
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.Title = "Detect faces";

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
                faces = await FaceClient.DetectAsync(analyzeCopyBuffer);
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

        public async Task<ActionResult> Landmarks()
        {
            ViewBag.Title = "Face landmarks";

            if (Request.HttpMethod == "GET")
            {
                return View("Index");
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
                faces = await FaceClient.DetectAsync(analyzeCopyBuffer, returnFaceLandmarks: true);
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
                    var props = typeof(FaceLandmarks).GetProperties();
                    foreach(var prop in props)
                    {
                        if(prop.PropertyType == typeof(FeatureCoordinate))
                        {
                            var coordinate = (FeatureCoordinate)prop.GetValue(face.FaceLandmarks);
                            var rect = new Rectangle((int)coordinate.X, (int)coordinate.Y, 2, 2);
                            g.DrawRectangle(pen, rect);
                        }
                    }
                }

                nonIndexedImg.Save(mem, ImageFormat.Png);

                var base64 = Convert.ToBase64String(mem.ToArray());
                imageResult = String.Format("data:image/png;base64,{0}", base64);
            }

            return View("Index",(object)imageResult);
        }

        public async Task<ActionResult> Identify()
        {
            var personGroupId = Request["PersonGroupId"];
            var model = new IdentifyFacesModel();

            var groups = await FaceClient.ListPersonGroupsAsync();
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
                    faces = await FaceClient.DetectAsync(analyzeCopyBuffer);
                    var faceIds = faces.Select(f => f.FaceId).ToArray();

                    if(faceIds.Length == 0)
                    {
                        model.Error = "No faces detected";
                        return View(model);
                    }

                    results = await FaceClient.IdentifyAsync(personGroupId, faceIds);
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
                    var person = await FaceClient.GetPersonAsync(personGroupId, candidate.PersonId);

                    identifiedFace.PersonCandidates.Add(person.PersonId, person.Name);
                }

                identifiedFace.Color = Settings.ImageSquareColors[model.IdentifiedFaces.Count];
                model.IdentifiedFaces.Add(identifiedFace);
            }

            Emotion[] emotionResults;

            using (var mem = new MemoryStream())
            {
                file.InputStream.CopyTo(mem);
                file.InputStream.Seek(0, SeekOrigin.Begin);
                mem.Seek(0, SeekOrigin.Begin);

                emotionResults = await EmotionClient.RecognizeAsync(mem, faces.Select(f => new Microsoft.ProjectOxford.Common.Rectangle
                {
                    Height = f.FaceRectangle.Height,
                    Width = f.FaceRectangle.Width,
                    Left = f.FaceRectangle.Left,
                    Top = f.FaceRectangle.Top
                }).ToArray());
            }

            foreach(var result in emotionResults)
            {
                var face = model.IdentifiedFaces.FirstOrDefault(f =>
                                    f.Face.FaceRectangle.Height == result.FaceRectangle.Height &&
                                    f.Face.FaceRectangle.Left == result.FaceRectangle.Left &&
                                    f.Face.FaceRectangle.Top == result.FaceRectangle.Top && 
                                    f.Face.FaceRectangle.Width == result.FaceRectangle.Width
                                );

                if(face != null)
                {
                    face.Emotions = new EmotionScores();
                    face.Emotions.Anger = result.Scores.Anger * 100;
                    face.Emotions.Contempt = result.Scores.Contempt * 100;
                    face.Emotions.Disgust = result.Scores.Disgust * 100;
                    face.Emotions.Fear = result.Scores.Fear * 100;
                    face.Emotions.Happiness = result.Scores.Happiness * 100;
                    face.Emotions.Neutral = result.Scores.Neutral * 100;
                    face.Emotions.Sadness = result.Scores.Sadness * 100;
                    face.Emotions.Surprise = result.Scores.Surprise * 100;
                }
            }

            file.InputStream.Seek(0, SeekOrigin.Begin);
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
    }
}
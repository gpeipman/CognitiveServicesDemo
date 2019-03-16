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
        public async Task<ActionResult> Index()
        {
            ViewBag.Title = "Detect faces";

            if (Request.HttpMethod == "GET")
            {
                return View();
            }

            string imageResult = "";

            await RunOperationOnImage(async stream =>
            {
                var faces = await FaceClient.DetectAsync(stream);
                imageResult = GetInlineImageWithFaces(faces);
            });

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
            Face[] faces = new Face[] { };

            await RunOperationOnImage(async stream =>
            {
                faces = await FaceClient.DetectAsync(stream, returnFaceLandmarks: true);
            });

            ImageToProcess.Seek(0, SeekOrigin.Begin);
            using (var img = new Bitmap(ImageToProcess))
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

            Face[] faces = new Face[] { };
            Guid[] faceIds = new Guid[] { };
            IdentifyResult[] results = new IdentifyResult[] { };
            
            await RunOperationOnImage(async stream =>
            {
                faces = await FaceClient.DetectAsync(stream);
                faceIds = faces.Select(f => f.FaceId).ToArray();

                if (faceIds.Count() > 0)
                {
                    results = await FaceClient.IdentifyAsync(personGroupId, faceIds);
                }
            });

            if (faceIds.Length == 0)
            {
                model.Error = "No faces detected";
                return View(model);
            }

            foreach (var result in results)
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

            model.ImageDump = GetInlineImageWithFaces(model.IdentifiedFaces.Select(f => f.Face));
            return View(model);
        }

        public async Task<ActionResult> Emotions()
        {
            var personGroupId = Request["PersonGroupId"];
            var model = new IdentifyFacesModel();

            var groups = await FaceClient.ListPersonGroupsAsync();
            model.PersonGroups = groups.Select(g => new SelectListItem
            {
                Value = g.PersonGroupId,
                Text = g.Name
            }).ToList();
            model.PersonGroups.Insert(0, new SelectListItem { Text = "", Value = "" });


            if (Request.HttpMethod == "GET")
            {
                return View(model);
            }

            Face[] faces = new Face[] { };
            Guid[] faceIds = new Guid[] { };
            IdentifyResult[] results = new IdentifyResult[] { };

            await RunOperationOnImage(async stream =>
            {
                var emotionsType = new[] { FaceAttributeType.Emotion };
                faces = await FaceClient.DetectAsync(stream, returnFaceAttributes: emotionsType);
                faceIds = faces.Select(f => f.FaceId).ToArray();

                if (faceIds.Length > 0 && !string.IsNullOrEmpty(personGroupId))
                {
                    results = await FaceClient.IdentifyAsync(personGroupId, faceIds);
                }
            });

            if (faceIds.Length == 0)
            {
                model.Error = "No faces detected";
                return View(model);
            }

            if (!string.IsNullOrEmpty(personGroupId))
            {
                foreach (var result in results)
                {
                    var identifiedFace = new IdentifiedFace();
                    identifiedFace.Face = faces.FirstOrDefault(f => f.FaceId == result.FaceId);

                    foreach (var candidate in result.Candidates)
                    {
                        var person = await FaceClient.GetPersonAsync(personGroupId, candidate.PersonId);

                        identifiedFace.PersonCandidates.Add(person.PersonId, person.Name);
                    }

                    model.IdentifiedFaces.Add(identifiedFace);
                    identifiedFace.Color = Settings.ImageSquareColors[model.IdentifiedFaces.Count];
                }
            }
            else
            {               
                foreach(var face in faces)
                {
                    var identifiedFace = new IdentifiedFace { Face = face };
                    model.IdentifiedFaces.Add(identifiedFace);

                    identifiedFace.Color = Settings.ImageSquareColors[model.IdentifiedFaces.Count];
                }
            }


            model.ImageDump = GetInlineImageWithIdentifiedFaces(model.IdentifiedFaces);

            return View(model);
        }

        public async Task<ActionResult> Attributes()
        {
            var personGroupId = Request["PersonGroupId"];
            var model = new IdentifyFacesModel();

            var groups = await FaceClient.ListPersonGroupsAsync();
            model.PersonGroups = groups.Select(g => new SelectListItem
            {
                Value = g.PersonGroupId,
                Text = g.Name
            }).ToList();
            model.PersonGroups.Insert(0, new SelectListItem { Text = "", Value = "" });

            if (Request.HttpMethod == "GET")
            {
                return View(model);
            }

            Face[] faces = new Face[] { };
            Guid[] faceIds = new Guid[] { };
            IdentifyResult[] results = new IdentifyResult[] { };
            var faceAttributeTypes = new[] {
                FaceAttributeType.Accessories, FaceAttributeType.Age, FaceAttributeType.Blur,
                FaceAttributeType.Exposure, FaceAttributeType.FacialHair, FaceAttributeType.Gender,
                FaceAttributeType.Glasses, FaceAttributeType.Hair, FaceAttributeType.HeadPose,
                FaceAttributeType.Makeup, FaceAttributeType.Noise, FaceAttributeType.Occlusion,
                FaceAttributeType.Smile
            };

            await RunOperationOnImage(async stream =>
            {
                faces = await FaceClient.DetectAsync(stream, returnFaceAttributes: faceAttributeTypes);
                faceIds = faces.Select(f => f.FaceId).ToArray();

                if (faceIds.Length > 0 && !string.IsNullOrEmpty(personGroupId))
                {
                    results = await FaceClient.IdentifyAsync(personGroupId, faceIds);
                }
            });

            if (faceIds.Length == 0)
            {
                model.Error = "No faces detected";
                return View(model);
            }

            if (!string.IsNullOrEmpty(personGroupId))
            {
                foreach (var result in results)
                {
                    var identifiedFace = new IdentifiedFace();
                    identifiedFace.Face = faces.FirstOrDefault(f => f.FaceId == result.FaceId);

                    foreach (var candidate in result.Candidates)
                    {
                        var person = await FaceClient.GetPersonAsync(personGroupId, candidate.PersonId);

                        identifiedFace.PersonCandidates.Add(person.PersonId, person.Name);
                    }

                    identifiedFace.Color = Settings.ImageSquareColors[model.IdentifiedFaces.Count];
                    model.IdentifiedFaces.Add(identifiedFace);
                }
            }
            else
            {
                foreach (var face in faces)
                {
                    var identifiedFace = new IdentifiedFace { Face = face };
                    model.IdentifiedFaces.Add(identifiedFace);

                    identifiedFace.Color = Settings.ImageSquareColors[model.IdentifiedFaces.Count];
                }
            }

            model.ImageDump = GetInlineImageWithIdentifiedFaces(model.IdentifiedFaces);

            return View(model);
        }
    }
}
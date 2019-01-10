using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace CognitiveServicesDemo.Areas.Faces.Controllers
{
    public class PeopleController : FacesBaseController
    {
        public async Task<ActionResult> Index(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return HttpNotFound("Person group ID is missing");
            }

            var model = await FaceClient.ListPersonsAsync(id);
            ViewBag.PersonGroupId = id;

            return View(model);
        }

        public async Task<ActionResult> Details(string id, Guid? personId)
        {
            if(string.IsNullOrEmpty(id))
            {
                return HttpNotFound();
            }

            if(personId == null)
            {
                return HttpNotFound();
            }

            var model = await FaceClient.GetPersonAsync(id, personId.Value);
            ViewBag.PersonGroupId = id;

            return View(model);
        }

        public ActionResult Create(string id)
        {
            ViewBag.PersonGroupId = id;

            return View("Edit", new Person());
        }

        [HttpPost]
        public async Task<ActionResult> Create(Person person)
        {
            return await Edit(person);
        }

        public async Task<ActionResult> Edit(string id, Guid personId)
        {
            ViewBag.PersonGroupId = id;

            var model = await FaceClient.GetPersonAsync(id, personId);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Person person)
        {
            var personGroupId = Request.Form["PersonGroupId"];
            if(string.IsNullOrEmpty(personGroupId))
            {
                return HttpNotFound("PersonGroupId is missing");
            }

            if(!ModelState.IsValid)
            {
                ViewBag.PersonGroupId = personGroupId;
                return View(person);
            }

            try
            {
                if (person.PersonId == Guid.Empty)
                {
                    await FaceClient.CreatePersonAsync(personGroupId, person.Name, person.UserData);
                }
                else
                {
                    await FaceClient.UpdatePersonAsync(personGroupId, person.PersonId, person.Name, person.UserData);
                }

                return RedirectToAction("Index", new { id = personGroupId });
            }
            catch (FaceAPIException fex)
            {
                ModelState.AddModelError(string.Empty, fex.ErrorMessage);
            }

            return View(person);
        }

        [HttpGet]
        public ActionResult AddFace(string id, string personId)
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddFace()
        {
            var id = Request["id"];
            var personId = Guid.Parse(Request["personId"]);

            try
            {
                Request.Files[0].InputStream.Seek(0,SeekOrigin.Begin);
                await FaceClient.AddPersonFaceAsync(id, personId, Request.Files[0].InputStream);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }

            return RedirectToAction("Index", new { id = id });
        }
        
        [HttpGet]
        public async Task<ActionResult> Delete(string id, string personId)
        {
            var personGuid = Guid.Parse(Request["personId"]);
            await FaceClient.DeletePersonAsync(id, personGuid);
            return RedirectToAction("Index", new { id = id });
        }
    }
}

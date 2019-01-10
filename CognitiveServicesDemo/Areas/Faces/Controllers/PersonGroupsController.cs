using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using CognitiveServicesDemo.Areas.Faces.Models;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace CognitiveServicesDemo.Areas.Faces.Controllers
{
    public class PersonGroupsController : FacesBaseController
    {
        public async Task<ActionResult> Index()
        {
            var groups = await FaceClient.ListPersonGroupsAsync();

            return View(groups);
        }

        public async Task<ActionResult> Details(string id)
        {
            if(string.IsNullOrEmpty(id))
            {

            }

            var model = new PersonGroupDetailsModel();
            model.PersonGroup = await FaceClient.GetPersonGroupAsync(id);

            try
            {
                model.TrainingStatus = await FaceClient.GetPersonGroupTrainingStatusAsync(id);
            }
            catch (FaceAPIException fex)
            {
                ModelState.AddModelError(string.Empty, fex.ErrorMessage);
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new PersonGroup());
        }

        [HttpPost]
        public async Task<ActionResult> Create(PersonGroup model)
        {
            try
            {
                await FaceClient.CreatePersonGroupAsync(model.PersonGroupId, model.Name, model.UserData);
            }
            catch (FaceAPIException fex)
            {
                ModelState.AddModelError(string.Empty, fex.ErrorMessage);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Edit(string id)
        {
            var model = await FaceClient.GetPersonGroupAsync(id);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(PersonGroup model)
        {
            await FaceClient.UpdatePersonGroupAsync(model.PersonGroupId, model.Name, model.UserData);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Train(string id)
        {
            await FaceClient.TrainPersonGroupAsync(id);

            return RedirectToAction("Details", new { id = id });
        }
        
        [HttpGet]
        public async Task<ActionResult> Delete(string id)
        {
            await FaceClient.DeletePersonGroupAsync(id);
            return RedirectToAction("Index", new { id = id });
        }
    }
}

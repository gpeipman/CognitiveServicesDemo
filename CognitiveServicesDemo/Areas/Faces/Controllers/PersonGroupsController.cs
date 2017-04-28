using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace CognitiveServicesDemo.Areas.Faces.Controllers
{
    public class PersonGroupsController : FacesBaseController
    {
        public async Task<ActionResult> Index()
        {
            using (var client = GetFaceClient())
            {
                var groups = await client.ListPersonGroupsAsync();

                return View(groups);
            }
        }

        public async Task<ActionResult> Details(string id)
        {
            using (var client = GetFaceClient())
            {
                var model = await client.GetPersonGroupAsync(id);

                return View(model);
            }
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
                using (var client = GetFaceClient())
                {
                    await client.CreatePersonGroupAsync(model.PersonGroupId, model.Name, model.UserData);
                }
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
    }
}
using System.Configuration;
using System.Web.Mvc;
using CognitiveServicesDemo.Models;
using Microsoft.ProjectOxford.Vision;

namespace CognitiveServicesDemo.Areas.ComputerVision.Controllers
{
    public abstract class ComputerVisionBaseController : ImageUsingBaseController
    {
        protected VisionServiceClient VisionServiceClient { get; private set; }

        public ComputerVisionBaseController()
        {
            var apiKey = ConfigurationManager.AppSettings["CognitiveServicesVisionApiKey"];
            var apiRoot = ConfigurationManager.AppSettings["CognitiveServicesVisionApiUrl"];
            VisionServiceClient = new VisionServiceClient(apiKey, apiRoot);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            ViewBag.LeftMenu = "_ComputerVisionMenu";
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);

            if (filterContext.ExceptionHandled)
            {
                return;
            }

            var message = filterContext.Exception.Message;
            var code = "";

            if (filterContext.Exception is ClientException)
            {
                var faex = filterContext.Exception as ClientException;
                message = faex.Error.Message;
                code = faex.Error.Code;
            }

            filterContext.Result = new ViewResult
            {
                ViewName = "Error",
                ViewData = new ViewDataDictionary(filterContext.Controller.ViewData)
                {
                    Model = new ErrorModel { Code = code, Message = message }
                }
            };

            filterContext.ExceptionHandled = true;
        }
    }
}
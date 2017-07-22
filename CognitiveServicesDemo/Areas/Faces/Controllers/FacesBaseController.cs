using System.Configuration;
using System.Web.Mvc;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Face;

namespace CognitiveServicesDemo.Areas.Faces.Controllers
{
    public abstract class FacesBaseController : Controller
    {
        protected FaceServiceClient FaceClient { get; private set; }
        protected EmotionServiceClient EmotionClient { get; private set; }

        public FacesBaseController()
        { 
            var apiKey = ConfigurationManager.AppSettings["CognitiveServicesFaceApiKey"];
            var apiRoot = ConfigurationManager.AppSettings["CognitiveServicesFaceApiUrl"];
            FaceClient = new FaceServiceClient(apiKey, apiRoot);

            apiKey = ConfigurationManager.AppSettings["CognitiveServicesEmotionApiKey"];
            apiRoot = ConfigurationManager.AppSettings["CognitiveServicesEmotionApiUrl"];
            EmotionClient = new EmotionServiceClient(apiKey, apiRoot);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(FaceClient != null)
            {
                FaceClient.Dispose();
                FaceClient = null;
            }
        }
    }
}
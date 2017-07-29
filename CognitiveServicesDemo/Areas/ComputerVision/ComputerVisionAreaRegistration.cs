using System.Web.Mvc;

namespace CognitiveServicesDemo.Areas.ComputerVision
{
    public class ComputerVisionAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ComputerVision";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ComputerVision_default",
                "ComputerVision/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
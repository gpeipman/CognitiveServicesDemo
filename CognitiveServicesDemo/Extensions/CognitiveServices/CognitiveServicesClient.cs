using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CognitiveServicesDemo.Extensions.CognitiveServices
{
    public static class CognitiveServicesClient
    {
        private static readonly string ApiKey = ConfigurationManager.AppSettings["CognitiveServicesKey"];
        private static readonly string ApiUrl = ConfigurationManager.AppSettings["CongnitiveServicesApiUrl"];

        public static async Task<List<DetectedFace>> DetectFaces(Stream image)
        {
            using (var client = new HttpClient())
            {
                var content = new StreamContent(image);
                var url = ApiUrl + "/face/v1.0/detect?returnFaceId=true&returnFaceLandmarks=true";

                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "ae68ba18c736445ead14f46905279610");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var httpResponse = await client.PostAsync(url, content);
                
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    var responseBody = await httpResponse.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<DetectedFace>>(responseBody);
                }
            }

            return null;
        }
    }
}
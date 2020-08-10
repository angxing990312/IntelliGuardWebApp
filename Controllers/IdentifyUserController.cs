
using System;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


using System.IO;

using System.Data;

using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;

using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;

namespace maddweb.Controllers
{


    public class IdentifyUserController : Controller
    {


        static string faceApiKey = "0d7af552cce6469999a42e0383d1edd9";
        static string faceApiEndPoint = "https://southeastasia.api.cognitive.microsoft.com/";


        public static IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }

        public IActionResult Index()
        {


            return View();
        }





        [HttpPost]
        [Route("api/IdentifyUser/{filename}")]
        public async Task<IActionResult> IdentifyUser(string filename)
        {
            var personGroupId = "test";
            string personname = "";
            Person a = (dynamic)null;
            string fullpath = $"{Directory.GetCurrentDirectory()}{@"\wwwroot\photos\" + filename}";
            IFaceClient faceClient = Authenticate(faceApiEndPoint, faceApiKey);
            using (Stream fs = System.IO.File.OpenRead(fullpath))
            {


                var faces = await faceClient.Face.DetectWithStreamAsync(fs);

                if (faces.Count == 0)
                {
                    TempData["Message"] = "No Person in the picture";
                }
                else if (faces.Count > 1)
                {
                    TempData["Message"] = "More than one  Person in the picture";
                }
                else
                {

                    

                    var faceIds = faces.Select(face => face.FaceId).ToArray();
                    Debug.WriteLine(" face: {0}", faceIds[0]);
                    var results = await faceClient.Face.IdentifyAsync(faceIds.OfType<Guid>().ToList(), personGroupId);
                    foreach (var identifyResult in results)
                    {
                        Debug.WriteLine("Result of face: {0}", identifyResult.FaceId);

                        if (identifyResult.Candidates.Count == 0)
                        {
                            Debug.WriteLine("no one identified");
                        }
                        else
                        {

                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var confidence = identifyResult.Candidates[0].Confidence;
                            var person = await faceClient.PersonGroupPerson.GetAsync(personGroupId, candidateId);
                            personname = person.Name;
                            a = person;
                            Debug.WriteLine(person.Name);
                            Console.WriteLine(personname);

                        }
                    }
                }

            }
            return Ok(a);
        }





        [HttpPost]
        [Route("api/makerequest/{filename}")]
        public async Task<IActionResult> MakeRequest(string filename)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", faceApiKey);

            var uri = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0/identify?" + queryString;

            HttpResponseMessage response;

            string fullpath = $"{Directory.GetCurrentDirectory()}{@"\wwwroot\images\" + filename}";

            // Request body
            byte[] byteData = Encoding.UTF8.GetBytes(fullpath);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
            }
            return Ok("yes");

        }
    }
}



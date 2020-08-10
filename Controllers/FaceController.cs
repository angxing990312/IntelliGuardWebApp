using maddweb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace maddweb.Controllers
{
    public class FaceController : Controller
    {
        //private AppDbContext _dbContext;

      //  public FaceController(AppDbContext dbContext)
       // {
       //     _dbContext = dbContext;
      //  }

        public static Entry a = new Entry
        {
            UserID = 1,
            Temperature = 36,
            Photo = "",
            EntryTime = new DateTime(2019, 11, 12, 12, 12, 12)

        };
        public static Entry record = new Entry();
        public IActionResult Form()
        {

            return View();
        }


        public IActionResult Form1()
        {

            return View();
        }


        public IActionResult SelectLocation()
        {

            List<string> locations = new List<string>() { "North Agora", "South Agora" };
            ViewData["locations"] = locations;
            return View();
        }


        //north 
        public IActionResult Getrecords()
        {

           // DbSet<Entry> dbs = _dbContext.Entry;
           // var model1 = dbs.Where(e => e.Location.Equals("north_agora")).FirstOrDefault();

            List<Entry> model = DBUtl.GetList<Entry>("Select * From Entry Where location = 'north_agora' ");
            record = model.LastOrDefault();
            if (record.EntryTime == DateTime.Now)
            {
                record = new Entry
                {
                    UserID = 0,
                    Temperature = 0,
                    Photo = null,
                    EntryTime = DateTime.Now
                }
                ;

                ViewData["name"] = "";

            }

            else
            {
                a = record;
                string sql = string.Format("Select UserName From MaddUser  Where UserID = {0}", a.UserID);
                DataTable dt1 = DBUtl.GetTable(sql);

                ViewData["name"] = dt1.Rows[0]["UserName"].ToString().Trim();

            }

            return View(record);

        }

        public IActionResult Getrecords1()
        {

            // DbSet<Entry> dbs = _dbContext.Entry;
            // var model1 = dbs.Where(e => e.Location.Equals("north_agora")).FirstOrDefault();

            List<Entry> model = DBUtl.GetList<Entry>("Select * From Entry Where location = 'south_agora' ");
            record = model.LastOrDefault();
            if (record.EntryTime == DateTime.Now)
            {
                record = new Entry
                {
                    UserID = 0,
                    Temperature = 0,
                    Photo = null,
                    EntryTime = DateTime.Now
                }
                ;

                ViewData["name"] = "";

            }

            else
            {
                a = record;
                string sql = string.Format("Select UserName From MaddUser  Where UserID = {0}", a.UserID);
                DataTable dt1 = DBUtl.GetTable(sql);

                ViewData["name"] = dt1.Rows[0]["UserName"].ToString().Trim();

            }

            return View(record);

        }





        [HttpPost]
        public IActionResult SelectLocation(string Location)
        {

            TempData["location"] = Location;

            if (Location.Equals("north_agora"))
            {
                return RedirectToAction("Form");
            }

            else
            {
                return RedirectToAction("Form1");
            }
           

        }



        public IActionResult pi()
        {
            DataTable dt = DBUtl.GetTable("select TOP 1* from Entry  inner join MaddUser on MaddUser.UserID=Entry.UserID order by EntryID DESC");

            ViewData["name"] = dt.Rows[0]["UserName"].ToString().Trim();
            ViewData["Temperature"] = dt.Rows[0]["Temperature"].ToString().Trim();
            ViewData["location"] = "RPIC";
            DateTime time = Convert.ToDateTime(dt.Rows[0]["EntryTime"].ToString().Trim());
            string output = $"{time:yyyy MMM dd HH:mm:ss}";
            ViewData["Time"] = output;

            DataTable dt1 = DBUtl.GetTable("Select UserName From MaddUser on MaddUser Where UserID = {UserID}");

            ViewData["name"] = dt.Rows[0]["UserName"].ToString().Trim();




            return View();
        }
        static class test
        {
            public static string p;
        }

        public IActionResult Index()
        {
            return View("Index");
        }

        public async Task<string> FaceLogin(IFormFile upimage)
        {
            string outcome = "no";
            string fullpath = Path.Combine(_env.WebRootPath, @"webcam\login.jpg");
            using (FileStream fs = new FileStream(fullpath, FileMode.Create))
            {
                upimage.CopyTo(fs);
                fs.Close();
            }
            var personGroupId = "test";

            using (Stream fs = System.IO.File.OpenRead(fullpath))
            {
                var faces = await faceClient.Face.DetectWithStreamAsync(fs);
                if (faces.Count == 0)
                {
                    TempData["Message"] = "No person in the Webcam";
                }
                else if (faces.Count > 1)
                {
                    TempData["Message"] = "One person at a time please";
                }
                else
                {
                    var faceIds = faces.Select(face => face.FaceId).ToArray();
                    var results = await faceClient.Face.IdentifyAsync(faceIds.OfType<Guid>().ToList(), personGroupId);
                    foreach (var identifyResult in results)
                    {
                        Debug.WriteLine("Result of face: {0}", identifyResult.FaceId);
                        if (identifyResult.Candidates.Count == 0)
                        {
                            Debug.WriteLine("No one identified");
                            TempData["Message"] = "No One Identified";
                        }
                        else
                        {
                            // Get top 1 among all candidates returned
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var confidence = identifyResult.Candidates[0].Confidence;
                            var person = await faceClient.PersonGroupPerson.GetAsync(personGroupId, candidateId);
                            test.p = person.Name;
                            Debug.WriteLine("Identified as {0} ({1})", person.Name, confidence);

                            TempData["Message2"] = $"Identified as {person.Name} ({confidence})";
                            outcome = "yes";
                        }
                    }
                }
            }

            return outcome;

        }



        public string FaceLoginOrg(IFormFile upimage)
        {
            string fullpath = Path.Combine(_env.WebRootPath, "login/user.jpg");
            using (FileStream fs = new FileStream(fullpath, FileMode.Create))
            {
                upimage.CopyTo(fs);
                fs.Close();
            }

            string imagePath = @"/login/user.jpg";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", FACEAPIKEY);
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";
            string uri = FACEAPIENDPOINT + "/detect?" + requestParameters;
            var fileInfo = _env.WebRootFileProvider.GetFileInfo(imagePath);
            var byteData = GetImageAsByteArray(fileInfo.PhysicalPath);
            string contentStringFace = string.Empty;
            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                var response = client.PostAsync(uri, content).Result;

                // Get the JSON response.
                contentStringFace = response.Content.ReadAsStringAsync().Result;
            }

            var expConverter = new ExpandoObjectConverter();
            dynamic faceList = JsonConvert.DeserializeObject<List<ExpandoObject>>(contentStringFace, expConverter);
            if (faceList.Count == 0)
            {
                TempData["json"] = "No Face detected";
            }
            else
            {
                TempData["json"] = JsonPrettyPrint(contentStringFace);
            }

            return contentStringFace;

        }




        public IActionResult Next()
        {

            if (!AuthenticateUser(test.p,
                                  out ClaimsPrincipal principal))
            {
                ViewData["Message"] = "Incorrect User ID or Password";
                return RedirectToAction("login", "Account");
            }
            else
            {
                HttpContext.SignInAsync(
                   CookieAuthenticationDefaults.AuthenticationScheme,
                   principal);

                if (TempData["returnUrl"] != null)
                {
                    string returnUrl = TempData["returnUrl"].ToString();
                    if (Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                }
                TempData["AfterLogin"] = "succeed";

                return RedirectToAction("Index", "Admin");
            }
        }

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            byte[] bytes = binaryReader.ReadBytes((int)fileStream.Length);
            binaryReader.Close();
            fileStream.Close();
            return bytes;
        }


        static string JsonPrettyPrint(string json)
        {
            string INDENT_STRING = "    ";
            int indentation = 0;
            int quoteCount = 0;
            var result =
                from ch in json
                let quotes = ch == '"' ? quoteCount++ : quoteCount
                let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, indentation)) : null
                let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, ++indentation)) : ch.ToString()
                let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, --indentation)) + ch : ch.ToString()
                select lineBreak == null ? openChar.Length > 1 ? openChar : closeChar : lineBreak;
            return String.Concat(result);
        }

        public FaceController(IWebHostEnvironment environment,
                        IConfiguration config)
        {
            _env = environment;
            FACEAPIKEY = config.GetSection("FaceConfig").GetValue<string>("SubscriptionKey");
            FACEAPIENDPOINT = config.GetSection("FaceConfig").GetValue<string>("EndPoint");
            faceClient = new FaceClient(
               new ApiKeyServiceClientCredentials(FACEAPIKEY),
               new System.Net.Http.DelegatingHandler[] { });
            //faceClient.Endpoint = FACEAPIENDPOINT;
            faceClient.Endpoint = "https://southeastasia.api.cognitive.microsoft.com";

        }





        private IWebHostEnvironment _env;

        //const string faceApiKey = "0d7af552cce6469999a42e0383d1edd9";
        //const faceApiEndPoint = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0";

        private readonly string FACEAPIKEY;
        private readonly string FACEAPIENDPOINT;
        private readonly IFaceClient faceClient;

        private bool AuthenticateUser(string uid,
                                    out ClaimsPrincipal principal)
        {
            principal = null;

            string sql = @"SELECT * FROM [dbo].[MaddUser] where UserId='{0}';";

            DataTable ds = DBUtl.GetTable(sql, uid);
            string name = ds.Rows[0]["UserName"].ToString();
            string role = ds.Rows[0]["Role"].ToString();
            if (ds.Rows.Count == 1)
            {
                principal =
                   new ClaimsPrincipal(
                      new ClaimsIdentity(
                         new Claim[] {
                        new Claim(ClaimTypes.NameIdentifier, uid),
                        new Claim(ClaimTypes.Name,name),
                        new Claim(ClaimTypes.Role,role)

                         },
                         CookieAuthenticationDefaults.AuthenticationScheme));
                return true;
            }
            return false;
        }

    }
}

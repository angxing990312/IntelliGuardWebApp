using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using maddweb.Models;
using System.Web.Http;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using FromBodyAttribute = System.Web.Http.FromBodyAttribute;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace maddweb.Controllers
{
	public class MaddUserController : Controller
	{
		//const string faceApiKey = "0d7af552cce6469999a42e0383d1edd9";
		//const faceApiEndPoint = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0";
		private IWebHostEnvironment _env;
		private readonly string FACEAPIKEY;
		private readonly string FACEAPIENDPOINT;
		private readonly IFaceClient faceClient;
		const string RECOGNITION_MODEL2 = RecognitionModel.Recognition02;
		const string RECOGNITION_MODEL1 = RecognitionModel.Recognition01;
		public MaddUserController(IWebHostEnvironment environment,
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
		const int PersonCount = 10000;
		const int CallLimitPerSecond = 10;

		static Queue<DateTime> _timeStampQueue = new Queue<DateTime>(CallLimitPerSecond);
		public static async Task WaitCallLimitPerSecondAsync()
		{
			Monitor.Enter(_timeStampQueue);
			try
			{
				if (_timeStampQueue.Count >= CallLimitPerSecond)
				{
					TimeSpan timeInterval = DateTime.UtcNow - _timeStampQueue.Peek();
					if (timeInterval < TimeSpan.FromSeconds(1))
					{
						await Task.Delay(TimeSpan.FromSeconds(1) - timeInterval);
					}
					_timeStampQueue.Dequeue();
				}
				_timeStampQueue.Enqueue(DateTime.UtcNow);
			}
			finally
			{
				Monitor.Exit(_timeStampQueue);
			}
		}

		public IActionResult index()
		{

			return View();
		}

		[Microsoft.AspNetCore.Authorization.AllowAnonymous]
		[HttpGet]
		[Route("api/MaddUser/{name}/{pass}")]
		public IActionResult NormalLogin(string name, string pass)
		{
			List<MaddUser> dt = DBUtl.GetList<MaddUser>("SELECT * from MaddUser where UserName='{0}' and UserPw='{1}'", name, pass);
			IActionResult response = Unauthorized();

			if (dt.Count == 1)
			{
				MaddUser user = new MaddUser();
				user = dt[0];
				response = Ok(user);
			}
			return response;
		}




		//api portion

		[HttpGet]
		[Route("api/Madduser/info")]
		public IActionResult UserInfo()
		{
			List<MaddUser> dbList = DBUtl.GetList<MaddUser>("Select * FROM MaddUser");
			return Json(dbList);
		}

		[HttpPost]
		[Route("api/multifacesignup")]
		public async Task<IActionResult> MultipleFace([FromUri]int id, [FromUri] string[] PathInURL)
		{
			IQueryCollection qry = HttpContext.Request.Query;
			PersistedFace face = null;
			string sid = qry["id"].ToString();
			string p = qry["PathInURL"].ToString();
			string[] path = p.Split(",");
			for (int i = 0; i < path.Length; i++)
			{
				path[i] = Path.Combine(_env.WebRootPath, "photos/" + path[i]);


			}
			String[] fullpath = path.ToArray();
			if (sid == null || sid.Equals("") || fullpath.Length == 0)
			{

				return Json("no input");

			}
			else
			{
				Dictionary<string, string[]> personDictionary = new Dictionary<string, string[]>();

				personDictionary.Add(sid, (fullpath));

				foreach (var groupedFace in personDictionary.Keys)
				{
					await Task.Delay(250);
					Person person = await faceClient.PersonGroupPerson.CreateAsync("test", sid);

					if (person == null)
					{
						return Json("no person");
					}
					else
					{
						foreach (var similarImage in personDictionary[groupedFace])
						{

							//face = await faceClient.PersonGroupPerson.AddFaceFromUrlAsync(personGroupId, person.PersonId,
							//$"{similarImage}");
							await WaitCallLimitPerSecondAsync();
							using (System.IO.Stream stream = System.IO.File.OpenRead(similarImage))
							{
								face = await faceClient.PersonGroupPerson.AddFaceFromStreamAsync("test", person.PersonId, stream);
								if (face != null)
								{
									//train
									await faceClient.PersonGroup.TrainAsync("test");

									// Wait until the training is completed.
									while (true)
									{
										await Task.Delay(1000);
										var trainingStatus = await faceClient.PersonGroup.GetTrainingStatusAsync("test");
										Console.WriteLine($"Training status: {trainingStatus.Status}.");
										if (trainingStatus.Status == TrainingStatusType.Succeeded)
										{

											break;

										}
									}

								}
								else
								{
									return Json("No face");
								}

							}

						}


					}
				}
				return Ok("succeed");
			}
		}






		[HttpPost]
		[Route("api/{id}/{location}")]
		public async Task<IActionResult> UserFace(int id, string location)
		{
			//create person 
			Person p = new Person();
			string personName = $"{id}";
			p = await faceClient.PersonGroupPerson.CreateAsync("test", personName);
			Guid personId = p.PersonId;

			// update UPhotoPath with person id 
			string sql = $"update MaddUser set UPhotoPath='{personId}' where UserID={id}";
			int row = DBUtl.ExecSQL(sql);
			string fp = Path.Combine(_env.WebRootPath, "photos/" + location);


			if (row == 1)
			{
				try
				{

					var client = new HttpClient();

					// Request headers
					client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "0d7af552cce6469999a42e0383d1edd9");
					var uri = $"https://southeastasia.api.cognitive.microsoft.com/face/v1.0/persongroups/test/persons/{personId}/persistedFaces";

					HttpResponseMessage response;

					using (var content = new ByteArrayContent(ImageToBinary(fp)))
					{
						content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
						// call microsoft add face API 
						response = await client.PostAsync(uri, content);
					}
					await faceClient.PersonGroup.TrainAsync("test");
					while (true)
					{
						await Task.Delay(1000);
						var trainingStatus = await faceClient.PersonGroup.GetTrainingStatusAsync("test");
						Console.WriteLine($"Training status: {trainingStatus.Status}.");
						if (trainingStatus.Status == TrainingStatusType.Succeeded) { break; }
					}
					
				}
				catch (Exception ex)
				{
					Debug.Print(ex.StackTrace);
					return BadRequest(ex.StackTrace);
				}
				
			}
			return Ok("succeed");

		}

		[HttpPost]
		[Route("api/UploadOnePhoto")]
		public IActionResult UploadPhoto(IFormFile photo)
		{
			try
			{
				string fext = Path.GetExtension(photo.FileName);
				string uname = Guid.NewGuid().ToString();
				string fname = uname + fext;
				string fullpath = Path.Combine(_env.WebRootPath, "photos/" + fname);
				using (FileStream fs = new FileStream(fullpath, FileMode.Create))
				{
					photo.CopyTo(fs);
					fs.Close();
				}
				var outcome = new
				{
					filename = fname,
					Result = true
				};
				return Ok(outcome);
			}
			catch
			{
				return BadRequest();
			}
		}


		[HttpGet]
		public IActionResult MobileSignUp()
		{
			return View();
		}
		[HttpPost]
		public IActionResult MobileSignUp(MaddUser user)
		{
			return View();
		}


		// Quan xing multiple upload 
		[HttpPost]
		[Route("api/MaddUser/UploadMultipleFiles")]
		public async Task<IActionResult> Posts(List<IFormFile> files)
		{
			try
			{
				var result = new List<FileUploadResult>();
				string fname = "";
				foreach (var file in files)
				{
					string fext = Path.GetExtension(file.FileName);
					string uname = Guid.NewGuid().ToString();
					 fname = uname + fext;
					var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/photos", fname);
					var stream = new FileStream(path, FileMode.Create);
					file.CopyToAsync(stream);
					result.Add(new FileUploadResult()
					{
						Name = fname,
						Length = file.Length
					});
				}

				return Ok(result);
			}
			catch
			{
				return BadRequest();
			}

		}

		
		

		public static byte[] ImageToBinary(string imagePath)
		{
			FileStream fS = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
			byte[] b = new byte[fS.Length];
			fS.Read(b, 0, (int)fS.Length);
			fS.Close();
			return b;
		}


	}
}

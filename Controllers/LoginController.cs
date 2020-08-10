using maddweb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;


namespace maddweb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : Controller
    {

        private IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string name, string pass)
        {
            MaddUser login= new MaddUser();
            login.UserName = name;
            login.UserPw = pass;
            IActionResult response = Unauthorized()
;
            var user = AuthenticateUser(login);

            if (user != null)
            {
                var tokenStr = GenerateJSONWebToken(user);
                response = Ok(new { token = tokenStr });
            }
            return response;
        }
        private MaddUser AuthenticateUser(MaddUser login)
        {
            MaddUser user = null;
            DataTable dt = DBUtl.GetTable($"SELECT * FROM MaddUser WHERE UserName='{login.UserName}' and UserPw='{login.UserPw}'");
            if (dt.Rows.Count == 1)
            {
                string id = dt.Rows[0]["UserID"].ToString();
                int userid = Int32.Parse(id);
                string name = dt.Rows[0]["UserName"].ToString();
                string pw= dt.Rows[0]["UserPw"].ToString();
               
                string path= dt.Rows[0]["UPhotoPath"].ToString();
                string contact= dt.Rows[0]["UserContact"].ToString();
                int contactnumber = Int32.Parse(contact);
                string role= dt.Rows[0]["Role"].ToString();

                user = new MaddUser(userid, name, pw, path, contactnumber, role);

                return user;
            }
            else
            {
                return user;
            }
        }

        private string GenerateJSONWebToken(MaddUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      
                 };

            var token = new JwtSecurityToken(issuer:_config["Jwt:Issuer"],
              audience:_config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            var encodetoken= new JwtSecurityTokenHandler().WriteToken(token);
            return encodetoken;
        }

        [Authorize]
        [HttpPost("Post")]
        public string Post()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var name = claim[0].Value;
            return "Hi, user " + name;

        }
        [Authorize]
        [HttpGet("GetValue")]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }
       
        /*
      [Authorize]
      public IActionResult Logoff(string returnUrl = null)
      {
          HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
          if (Url.IsLocalUrl(returnUrl))
              return Redirect(returnUrl);
          return RedirectToAction("index", "MaddUser");
      }

      [AllowAnonymous]
      public IActionResult Login(string returnUrl = null)
      {
          TempData["ReturnUrl"] = returnUrl;
          return View();
      }
      */
        /*
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(UserLogin user)
        {
            if (!AuthenticateUser(user.UserID, user.Password,
                                  out ClaimsPrincipal principal))
            {
                ViewData["Message"] = "Incorrect User ID or Password";
                return View("Login");
            }

            //Successful Login
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

                //Return to User Details page
                return RedirectToAction("UserPage", "Madd");
            }
        }

        [AllowAnonymous]
        public IActionResult Forbidden()
        {
            return View();
        }

        private bool AuthenticateUser(string uid, string pw,
                                         out ClaimsPrincipal principal)
        {
            principal = null;


            string sql = @"SELECT * FROM UserCredentials
                         WHERE email ='{0}' AND pwd ='{1}'";


            // string select = String.Format(sql, uid, pw);
            DataTable ds = DBUtl.GetTable(sql, uid, pw);
            if (ds.Rows.Count == 1)
            {
                principal =
                   new ClaimsPrincipal(
                      new ClaimsIdentity(
                         new Claim[] {
                        new Claim(ClaimTypes.NameIdentifier, uid),
                        new Claim(ClaimTypes.Name, ds.Rows[0]["email"].ToString()),
                        new Claim(ClaimTypes.Role, ds.Rows[0]["Role"].ToString())
                         },
                         CookieAuthenticationDefaults.AuthenticationScheme));
                return true;
            }

            
            return false;
        }



        [AllowAnonymous]
        [HttpPost]
        public IActionResult FaceLogin()
        {

            //Read the email 
            IFormCollection form = HttpContext.Request.Form;
            string email = form["emailUID"];
			//
			if (email.Equals(""))
			{
				return RedirectToAction("Forbidden", "Account");
			}


			//SQL to retrieve user details based on email 
			string SQL = @"SELECT * FROM userCredentials WHERE email = '{0}'";
            DataTable dt = DBUtl.GetTable(SQL, email);

            var mail = dt.Rows[0]["email"].ToString();
            var pwd = dt.Rows[0]["pwd"].ToString();

			


			if (!AuthenticateUser(mail, pwd,
                                  out ClaimsPrincipal principal))
            {
                ViewData["Message"] = "Incorrect User ID or Password";
                return View("Login");
            }

            //Successful Login
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

                //Return to User Details page
                return RedirectToAction("UserPage", "Madd");
            }
        }*/

    }
}
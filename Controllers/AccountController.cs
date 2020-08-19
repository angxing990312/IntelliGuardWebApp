using maddweb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace maddweb.Controllers
{
   
    //Ang xing is so clever and smart
    public class AccountController : Controller
    {
        
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
       
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(MaddUser user)
        {
            if (!AuthenticateUser(user.UserName, user.UserPw,
                                  out ClaimsPrincipal principal))
            {
                ViewData["Message"] = "Incorrect User ID or Password"+DBUtl.DB_Message;
                return View("login");
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
                DataTable dt = DBUtl.GetTable("SELECT * from MaddUser where UserName='{0}'", user.UserName);
                //Return to User Details page
                if (dt.Rows[0]["Role"].ToString().Trim().Equals("Staff")) {
                 
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("index", "MaddUser");
                }
            }
        }

        [AllowAnonymous]
        public IActionResult Forbidden()
        {
            return View();
        }
     

        private bool AuthenticateUser(string name, string pw,
                                         out ClaimsPrincipal principal)
        {
            principal = null;


            string sql = @"SELECT * FROM MaddUser
                         WHERE UserName ='{0}' AND UserPw ='{1}'";


            // string select = String.Format(sql, uid, pw);
            DataTable ds = DBUtl.GetTable(sql, name, pw);
            if (ds.Rows.Count == 1)
            {
                principal =
                   new ClaimsPrincipal(
                      new ClaimsIdentity(
                         new Claim[] {
                        new Claim(ClaimTypes.NameIdentifier, ds.Rows[0]["UserID"].ToString()),
                        new Claim(ClaimTypes.Name, ds.Rows[0]["UserName"].ToString()),
                        new Claim(ClaimTypes.Role, ds.Rows[0]["Role"].ToString())
                         },
                         CookieAuthenticationDefaults.AuthenticationScheme));
                return true;
            }

            
            return false;
        }

        public IActionResult FaceLogin()
        {
            return View();
        }

        [HttpPost]
        [Route("api/Data/Users")]
        public IActionResult Post([FromBody]MaddUser usr)
        {
            if (usr == null)
            {
                return BadRequest("Incorrect Input");
            }

            string sqlInsert = $"insert into MaddUser(UserName, UserPw, FullName, UPhotoPath, UserContact, Role, nric) values ('{usr.UserName}','{usr.UserPw}', '{usr.FullName}','{usr.UPhotoPath}',{usr.UserContact},'Student', '{usr.nric}');";
            if (DBUtl.ExecSQL(sqlInsert) == 1)
            {
                List<MaddUser> obj = DBUtl.GetList<MaddUser>($"select UserID from MaddUser where UserName='{usr.UserName}'");
                int id = obj[0].UserID;
                return Ok(id);
            }
                
            else
                return BadRequest(new { Message = DBUtl.DB_Message });
        }
    }
}
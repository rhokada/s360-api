using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
   [Authorize]
   [ApiController]
   [Route("[controller]")]
   public class UsersController : ControllerBase
   {
      private IUserService _userService;
      private ConnectionStrings _ConnectionStrings;
      private ReCaptchaService _reCaptchaService;

      public UsersController(
         IUserService userService,
         IOptions<ConnectionStrings> ConnectionStrings,
         ReCaptchaService reCaptchaService)
      {
         _reCaptchaService = reCaptchaService;
         _userService = userService;
         _ConnectionStrings = ConnectionStrings.Value;
      }

      private ContentResult OkDyn(dynamic obj)
      {
         string ret = JsonConvert.SerializeObject(obj);
         return Content(ret, "application/json");
      }


      [AllowAnonymous]
      [HttpPost("authenticate")]
      public IActionResult Authenticate([FromBody] AuthenticateModel model)
      {
         /*if (!_reCaptchaService.ValidaReCaptcha(model.recaptcha))
            return BadRequest(new { message = "ReCaptcha inválido" });*/

         var user = _userService.Authenticate(model.Username, model.Password, model.AppId);

         if (user == null)
            return BadRequest(new { message = "E-mail ou senha inválidos. Favor conferir." });

         return Ok(user);
      }

 /*     [AllowAnonymous]
      [HttpPost("authenticateSGP")]
      public IActionResult AuthenticateSGP([FromBody] AuthenticateModelSGP model)
      {
         var user = _userService.AuthenticateSGP(model.Username, model.Password);

         if (user == null)
            return BadRequest(new { message = "Username or password is incorrect" });

         return Ok(user);
      }
 */
      [HttpGet]
      public IActionResult GetAll()
      {
         var users = _userService.GetAll();
         using (var con = new SqlConnection(_ConnectionStrings.Default.ToString()))
         {
            try
            {
               con.Open();
               var query = "SELECT TOP 10 * FROM Customer";

               return Ok(con.Query(query).ToList());
            }
            catch (Exception ex)
            {
               throw ex;
            }
            finally
            {
               con.Close();
            }
         }


      }
   }
}

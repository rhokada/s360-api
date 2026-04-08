using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using Microsoft.Data.SqlClient;
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


      [AllowAnonymous]
      [HttpPost("NewTempPassword")]
      public IActionResult NewTempPassword([FromBody] NewTempPasswordModel model)
      {
          /*if (!_reCaptchaService.ValidaReCaptcha(model.recaptcha))
                return BadRequest(new { message = "ReCaptcha inválido" });*/

          var user = _userService.NewTempPassword(model.Username, model.AppId);

          if (user == null)
              return BadRequest(new { message = "E-mail inválidos. Favor conferir." });

          return Ok(user);
      }

      [AllowAnonymous]
      [HttpPost("ChangePassword")]
      public IActionResult ChangePassword ([FromBody] ChangePasswordModel changepassword)
      {
            /*if (!_reCaptchaService.ValidaReCaptcha(model.recaptcha))
               return BadRequest(new { message = "ReCaptcha inválido" });*/
            changepassword.userId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
            var user = _userService.ChangePassword(changepassword.userId, changepassword.Password, changepassword.NewPassword, changepassword.AppId);

          if (user.Email == null)
              return BadRequest(new { message = "E-mail ou senha inválidos. Favor conferir." });

          return Ok(user);
      }

      [HttpPost("GetPermissions")]
      public IActionResult GetPermissions()
      {
            var userId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
            var ret = _userService.GetPermissions(userId);

            return OkDyn(ret);
        }
    }
}

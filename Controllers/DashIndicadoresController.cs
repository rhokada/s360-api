using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class DashIndicadoresController : ControllerBase
    {
        private readonly IDashIndicadoresService _dashIndicadoresService;

        public DashIndicadoresController(IDashIndicadoresService dashIndicadoresService)
        {
            _dashIndicadoresService = dashIndicadoresService;
        }

        private static readonly JsonSerializerSettings _camelCase = new()
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        };

        private ContentResult OkDyn(dynamic obj)
        {
            string ret = JsonConvert.SerializeObject(obj, _camelCase);
            return Content(ret, "application/json");
        }

        [HttpGet("Select")]
        public IActionResult Select()
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = int.Parse(User.FindFirst("SubjectId")?.Value ?? "0");
            return OkDyn(_dashIndicadoresService.Select(userId, tokenUsuario));
        }
    }
}

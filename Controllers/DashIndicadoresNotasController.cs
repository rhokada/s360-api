using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class DashIndicadoresNotasController : ControllerBase
    {
        private readonly IDashIndicadoresNotasService _service;

        public DashIndicadoresNotasController(IDashIndicadoresNotasService service)
        {
            _service = service;
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
            return OkDyn(_service.Select(userId, tokenUsuario));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApi.Models;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AdmRolePermissionController : ControllerBase
    {
        private readonly IAdmRolePermissionService _admRolePermissionService;

        public AdmRolePermissionController(IAdmRolePermissionService admRolePermissionService)
        {
            _admRolePermissionService = admRolePermissionService;
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

        [HttpGet("Select/{admRoleId}")]
        public IActionResult Select(int admRoleId)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admRolePermissionService.Select(admRoleId, tokenUsuario));
        }

        [HttpPost("Upsert")]
        public IActionResult Upsert([FromBody] AdmRolePermissionUpsertModel model)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admRolePermissionService.Upsert(model, tokenUsuario));
        }
    }
}

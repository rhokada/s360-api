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
    public class AdmRoleController : ControllerBase
    {
        private readonly IAdmRoleService _admRoleService;
        private readonly IAdmRolePermissionService _admRolePermissionService;

        public AdmRoleController(IAdmRoleService admRoleService, IAdmRolePermissionService admRolePermissionService)
        {
            _admRoleService           = admRoleService;
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

        [HttpGet("Select")]
        public IActionResult Select([FromQuery] AdmRoleFilterModel filtro)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admRoleService.Select(filtro, tokenUsuario));
        }

        [HttpGet("Permissions/{admRoleId}")]
        public IActionResult GetPermissions(int admRoleId)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admRolePermissionService.Select(admRoleId, tokenUsuario));
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] AdmRoleCreateModel model)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admRoleService.Create(model, tokenUsuario));
        }

        [HttpPut("Update")]
        public IActionResult Update([FromBody] AdmRoleUpdateModel model)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admRoleService.Update(model, tokenUsuario));
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admRoleService.Delete(id, tokenUsuario));
        }
    }
}

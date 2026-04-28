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
    public class AdmRoleUserController : ControllerBase
    {
        private readonly IAdmRoleUserService _admRoleUserService;

        public AdmRoleUserController(IAdmRoleUserService admRoleUserService)
        {
            _admRoleUserService = admRoleUserService;
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
        public IActionResult Select([FromQuery] AdmRoleUserFilterModel filtro)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admRoleUserService.Select(filtro, tokenUsuario));
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] AdmRoleUserCreateModel model)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admRoleUserService.Create(model, tokenUsuario));
        }

        [HttpDelete("Delete/{admRoleUserId}")]
        public IActionResult Delete(int admRoleUserId)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admRoleUserService.Delete(admRoleUserId, null, null, tokenUsuario));
        }

        [HttpDelete("DeleteByRoleUser")]
        public IActionResult DeleteByRoleUser([FromQuery] int admRoleId, [FromQuery] int userId)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admRoleUserService.Delete(null, admRoleId, userId, tokenUsuario));
        }
    }
}

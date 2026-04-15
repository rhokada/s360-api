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
    public class AdmCompanyDeptController : ControllerBase
    {
        private readonly IAdmCompanyDeptService _admCompanyDeptService;

        public AdmCompanyDeptController(IAdmCompanyDeptService admCompanyDeptService)
        {
            _admCompanyDeptService = admCompanyDeptService;
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
        public IActionResult Select([FromQuery] AdmCompanyDeptFilterModel filtro)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admCompanyDeptService.Select(filtro, tokenUsuario));
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] AdmCompanyDeptCreateModel model)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admCompanyDeptService.Create(model, tokenUsuario));
        }

        [HttpPut("Update")]
        public IActionResult Update([FromBody] AdmCompanyDeptUpdateModel model)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admCompanyDeptService.Update(model, tokenUsuario));
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admCompanyDeptService.Delete(id, tokenUsuario));
        }
    }
}

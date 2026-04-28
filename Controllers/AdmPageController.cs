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
    public class AdmPageController : ControllerBase
    {
        private readonly IAdmPageService _admPageService;

        public AdmPageController(IAdmPageService admPageService)
        {
            _admPageService = admPageService;
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
        public IActionResult Select([FromQuery] AdmPageFilterModel filtro)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admPageService.Select(filtro, tokenUsuario));
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] AdmPageCreateModel model)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admPageService.Create(model, tokenUsuario));
        }

        [HttpPut("Update")]
        public IActionResult Update([FromBody] AdmPageUpdateModel model)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admPageService.Update(model, tokenUsuario));
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admPageService.Delete(id, tokenUsuario));
        }
    }
}

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
    public class AdmAddressController : ControllerBase
    {
        private readonly IAdmAddressService _admAddressService;

        public AdmAddressController(IAdmAddressService admAddressService)
        {
            _admAddressService = admAddressService;
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
        public IActionResult Select([FromQuery] AdmAddressFilterModel filtro)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admAddressService.Select(filtro, tokenUsuario));
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] AdmAddressCreateModel model)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admAddressService.Create(model, tokenUsuario));
        }

        [HttpPut("Update")]
        public IActionResult Update([FromBody] AdmAddressUpdateModel model)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admAddressService.Update(model, tokenUsuario));
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_admAddressService.Delete(id, tokenUsuario));
        }
    }
}

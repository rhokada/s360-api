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
    public class AiPromptController : ControllerBase
    {
        private readonly IAiPromptService _aiPromptService;

        public AiPromptController(IAiPromptService aiPromptService)
        {
            _aiPromptService = aiPromptService;
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
            return OkDyn(_aiPromptService.Select(tokenUsuario));
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] AiPromptCreateModel model)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_aiPromptService.Create(model, tokenUsuario));
        }

        [HttpPut("Update")]
        public IActionResult Update([FromBody] AiPromptUpdateModel model)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_aiPromptService.Update(model, tokenUsuario));
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_aiPromptService.Delete(id, tokenUsuario));
        }
    }
}

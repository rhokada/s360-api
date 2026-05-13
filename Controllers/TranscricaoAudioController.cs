using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class TranscricaoAudioController : ControllerBase
    {
        private readonly ITranscricaoAudioService _transcricaoAudioService;

        public TranscricaoAudioController(ITranscricaoAudioService transcricaoAudioService)
        {
            _transcricaoAudioService = transcricaoAudioService;
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
            return OkDyn(_transcricaoAudioService.Select(tokenUsuario));
        }
    }
}

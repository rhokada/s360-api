using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class DataImportProLogController : ControllerBase
    {
        private readonly IDataImportProLogService _service;

        private static readonly JsonSerializerSettings _camelCase = new()
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        };

        public DataImportProLogController(IDataImportProLogService service)
        {
            _service = service;
        }

        private ContentResult OkDyn(dynamic obj)
        {
            string ret = JsonConvert.SerializeObject(obj, _camelCase);
            return Content(ret, "application/json");
        }

        [HttpGet("Select")]
        public IActionResult Select([FromQuery] DataImportProLogFilterModel filtro)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_service.Select(filtro, tokenUsuario));
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            string tokenUsuario = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return OkDyn(_service.Delete(id, tokenUsuario));
        }

        [HttpPost("ImportarPlanilha")]
        public async Task<IActionResult> ImportarPlanilha(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Nenhum arquivo recebido." });

            if (!file.FileName.EndsWith(".xlsx", System.StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Formato inválido. Envie um arquivo .xlsx." });

            int? userId = null;
            var subjectId = User.FindFirst("SubjectId")?.Value;
            if (!string.IsNullOrEmpty(subjectId))
                userId = int.TryParse(subjectId, out var uid) ? uid : null;

            using var stream = file.OpenReadStream();
            var resultado = await _service.ImportarAsync(stream, file.FileName, userId);
            return Ok(resultado);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ScoreSellerController : ControllerBase
    {
        private readonly IScoreSellerService _service;

        private static readonly JsonSerializerSettings _camelCase = new()
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        };

        public ScoreSellerController(IScoreSellerService service)
        {
            _service = service;
        }

        private ContentResult OkDyn(dynamic obj)
        {
            string ret = JsonConvert.SerializeObject(obj, _camelCase);
            return Content(ret, "application/json");
        }

        [HttpPost("Processar")]
        public async Task<IActionResult> Processar([FromBody] ScoreSellerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.SellerCode))
                return BadRequest(new { message = "SellerCode é obrigatório." });

            if (request.ContractId <= 0)
                return BadRequest(new { message = "ContractId inválido." });

            try
            {
                var inserted = await _service.ProcessarAsync(request.SellerCode, request.ContractId);
                return OkDyn(new { success = true, mensagensInseridas = inserted });
            }
            catch (InvalidOperationException ex)
            {
                return OkDyn(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}

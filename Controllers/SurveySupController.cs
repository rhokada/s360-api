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
    public class SurveySupController : ControllerBase
    {
        private readonly ISurveySupService _surveySupService;

        public SurveySupController(ISurveySupService surveySupService)
        {
            _surveySupService = surveySupService;
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
        public IActionResult Select([FromQuery] SurveySupFilterModel filtro)
        {
            return OkDyn(_surveySupService.Select(filtro));
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] SurveySupCreateModel model)
        {
            return OkDyn(_surveySupService.Create(model));
        }

        [HttpPut("Update")]
        public IActionResult Update([FromBody] SurveySupUpdateModel model)
        {
            return OkDyn(_surveySupService.Update(model));
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            return OkDyn(_surveySupService.Delete(id));
        }
    }
}

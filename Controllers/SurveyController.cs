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
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
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
        public IActionResult Select([FromQuery] SurveyFilterModel filtro)
        {
            return OkDyn(_surveyService.Select(filtro));
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] SurveyCreateModel model)
        {
            return OkDyn(_surveyService.Create(model));
        }

        [HttpPut("Update")]
        public IActionResult Update([FromBody] SurveyUpdateModel model)
        {
            return OkDyn(_surveyService.Update(model));
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            return OkDyn(_surveyService.Delete(id));
        }
    }
}

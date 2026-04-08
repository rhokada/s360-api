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
    public class SurveyQuestionController : ControllerBase
    {
        private readonly ISurveyQuestionService _surveyQuestionService;

        public SurveyQuestionController(ISurveyQuestionService surveyQuestionService)
        {
            _surveyQuestionService = surveyQuestionService;
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
        public IActionResult Select([FromQuery] SurveyQuestionFilterModel filtro)
        {
            return OkDyn(_surveyQuestionService.Select(filtro));
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] SurveyQuestionCreateModel model)
        {
            return OkDyn(_surveyQuestionService.Create(model));
        }

        [HttpPut("Update")]
        public IActionResult Update([FromBody] SurveyQuestionUpdateModel model)
        {
            return OkDyn(_surveyQuestionService.Update(model));
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            return OkDyn(_surveyQuestionService.Delete(id));
        }
    }
}

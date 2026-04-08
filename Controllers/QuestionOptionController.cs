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
    public class QuestionOptionController : ControllerBase
    {
        private readonly IQuestionOptionService _questionOptionService;

        public QuestionOptionController(IQuestionOptionService questionOptionService)
        {
            _questionOptionService = questionOptionService;
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
        public IActionResult Select([FromQuery] QuestionOptionFilterModel filtro)
        {
            return OkDyn(_questionOptionService.Select(filtro));
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] QuestionOptionCreateModel model)
        {
            return OkDyn(_questionOptionService.Create(model));
        }

        [HttpPut("Update")]
        public IActionResult Update([FromBody] QuestionOptionUpdateModel model)
        {
            return OkDyn(_questionOptionService.Update(model));
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            return OkDyn(_questionOptionService.Delete(id));
        }
    }
}

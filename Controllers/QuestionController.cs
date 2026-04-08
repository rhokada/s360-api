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
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
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
        public IActionResult Select([FromQuery] QuestionFilterModel filtro)
        {
            return OkDyn(_questionService.Select(filtro));
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] QuestionCreateModel model)
        {
            return OkDyn(_questionService.Create(model));
        }

        [HttpPut("Update")]
        public IActionResult Update([FromBody] QuestionUpdateModel model)
        {
            return OkDyn(_questionService.Update(model));
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            return OkDyn(_questionService.Delete(id));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AdmHomeController : ControllerBase
    {
        private readonly IAdmHomeService _service;

        private static readonly JsonSerializerSettings _camelCase = new()
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        };

        public AdmHomeController(IAdmHomeService service)
        {
            _service = service;
        }

        private ContentResult OkDyn(dynamic obj)
        {
            string ret = JsonConvert.SerializeObject(obj, _camelCase);
            return Content(ret, "application/json");
        }

        [HttpGet("HomeData")]
        public async Task<IActionResult> HomeData()
        {
            int userId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);

            try
            {
                var data = await _service.GetHomeDataAsync(userId);
                return OkDyn(data);
            }
            catch (InvalidOperationException ex)
            {
                return OkDyn(new { erro = ex.Message });
            }
        }
    }
}

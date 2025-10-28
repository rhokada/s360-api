using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SgpController : ControllerBase
    {

        private ISgpService _sgpService;

        private ConnectionStrings _ConnectionStrings;

        public SgpController(ISgpService sgpService, IOptions<ConnectionStrings> ConnectionStrings)
        {
            _sgpService = sgpService;
            _ConnectionStrings = ConnectionStrings.Value;
        }

        private ContentResult OkDyn(dynamic obj)
        {
            string ret = JsonConvert.SerializeObject(obj);
            return Content(ret, "application/json");
        }



        [HttpGet("getPromotionList")]
        public IActionResult GetPromotionList()
        {
            var ret = _sgpService.GetPromotionList(Convert.ToInt32(User.FindFirst("CompanyId")?.Value));

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("getStatusParticipacaoList")]
        public IActionResult GetStatusParticipacaoList()
        {
            var ret = _sgpService.GetStatusParticipacaoList();

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("getStatusCupomList")]
        public IActionResult GetStatusCupomList()
        {
            var ret = _sgpService.GetStatusCupomList();

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }


        [HttpGet("getStatusEntregaList")]
        public IActionResult GetStatusEntregaList()
        {
            var ret = _sgpService.GetStatusEntregaList();

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpPost("promotionListPurchaseValueValidation")]
        public IActionResult PromotionListPurchaseValueValidation([FromBody] PromotionValidationSgpModel campos)
        {
            var ret = _sgpService.PromotionListPurchaseValueValidation(campos);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("promotionValidationCustomerData​")]
        public IActionResult PromotionValidationCustomerData(int PromotionValidationId)
        {
            var ret = _sgpService.PromotionValidationCustomerData(PromotionValidationId);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("promotionValidationPurchaseData")]
        public IActionResult PromotionValidationPurchaseData(int PromotionValidationId)
        {
            var ret = _sgpService.PromotionValidationPurchaseData(PromotionValidationId);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }


        [HttpGet("promotionValidationPurchaseProducts")]
        public IActionResult PromotionValidationPurchaseProducts(int PromotionValidationId)
        {
            var ret = _sgpService.PromotionValidationPurchaseProducts(PromotionValidationId);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("promotionValidationPurchaseProductsSEFAZ")]
        public IActionResult PromotionValidationPurchaseProductsSEFAZ(int PromotionValidationId)
        {
            var ret = _sgpService.PromotionValidationPurchaseProductsSEFAZ(PromotionValidationId);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("promotionValidationStatus")]
        public IActionResult PromotionValidationStatus()
        {
            var ret = _sgpService.PromotionValidationStatus();

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpPost("promotionPurchaseValueValidation")]
        public IActionResult PromotionPurchaseValueValidation([FromBody] PromotionValidationSgpModel campos)
        {
            var ret = _sgpService.PromotionPurchaseValueValidation(Convert.ToInt32(User.FindFirst("SubjectId")?.Value), campos);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpPost("promotionListPrizesToDeliver")]
        public IActionResult PromotionListPrizesToDeliver([FromBody] PromotionValidationSgpModel campos)
        {
            var ret = _sgpService.PromotionListPrizesToDeliver(campos);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("ServicesCompany")]
        public IActionResult ServicesCompany()
        {
            var ret = _sgpService.ServicesCompany(Convert.ToInt32(User.FindFirst("SubjectId")?.Value));

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("ConsCustomer")]
        public IActionResult ConsCustomer(string cpf, int promotionId)
        {
            var ret = _sgpService.ConsCustomer(cpf, promotionId);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("ConsCustomerParticipations")]
        public IActionResult ConsCustomerParticipations(int customerId)
        {
            var ret = _sgpService.ConsCustomerParticipations(customerId);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("PurchaseReceiptProducts​")]
        public IActionResult PurchaseReceiptProducts​(int purchasereceiptid)
        {
            var ret = _sgpService.PurchaseReceiptProducts​(purchasereceiptid);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("PurchaseReceiptValidationData​")]
        public IActionResult PurchaseReceiptValidationData​(int purchasereceiptid)
        {
            var ret = _sgpService.PurchaseReceiptValidationData​(purchasereceiptid);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("PurchaseReceiptProductsSEFAZ​")]
        public IActionResult PurchaseReceiptProductsSEFAZ​(int purchasereceiptid)
        {
            var ret = _sgpService.PurchaseReceiptProductsSEFAZ​(purchasereceiptid);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("ConsultaCupons")]
        public IActionResult ConsultaCupons(int purchasereceiptid, string proc)
        {
            var ret = _sgpService.ConsultaCupons(proc, purchasereceiptid, Convert.ToInt32(User.FindFirst("SubjectId")?.Value));

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("ConsultaFiltros")]
        public IActionResult ConsultaFiltros(string proc)
        {
            var ret = _sgpService.ConsultaFiltros(proc);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpPost("ValidationCuponsList")]
        public IActionResult ValidationCuponsList([FromBody] PromotionValidationSgpModel campos)
        {
            var ret = _sgpService.ValidationCuponsList(campos, Convert.ToInt32(User.FindFirst("SubjectId")?.Value));

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("AppCheckPurchaseReceiptAccessCode")]
        public IActionResult AppCheckPurchaseReceiptAccessCode(string accessCode)
        {
            var ret = _sgpService.AppCheckPurchaseReceiptAccessCode(accessCode);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("ListStateLinkSEFAZ")]
        public IActionResult ListStateLinkSEFAZ(string accessCode)
        {
            var ret = _sgpService.ListStateLinkSEFAZ(accessCode);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpPost("PurchaseReceiptValidationFinish")]
        public IActionResult PurchaseReceiptValidationFinish([FromBody] PromotionValidationSgpModel campos)
        {
            var ret = _sgpService.PurchaseReceiptValidationFinish(campos, Convert.ToInt32(User.FindFirst("SubjectId")?.Value));

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }

        [HttpGet("ListPromotionProducts")]
        public IActionResult ListPromotionProducts(int PromotionId)
        {
            var ret = _sgpService.ListPromotionProducts(PromotionId);

            if (ret == null)
            {
                return Unauthorized();
            }

            return OkDyn(ret);
        }
        //ValidationCuponsList
        //dynamic AppCheckPurchaseReceiptAccessCode(string accessCode);
        //dynamic ListStateLinkSEFAZ(string accessCode);
        //dynamic PurchaseReceiptValidationFinish(PromotionValidationSg
        //dynamic ListPromotionProducts(int PromotionId);
    }
}

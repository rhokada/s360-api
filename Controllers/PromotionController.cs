using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services;
using WebApi.Services.Interfaces;
using System.Collections.Generic;


namespace WebApi.Controllers
{
   [Authorize]
   [ApiController]
   [Route("[controller]")]
   public class PromotionController : ControllerBase
   {
      private IPromotionService _promotionService;
      private ConnectionStrings _ConnectionStrings;
      private ICustomerService _customerService;
      private ReCaptchaService _reCaptchaService;


      public PromotionController(
           IOptions<ConnectionStrings> ConnectionStrings,
           IPromotionService promotionService,
           ICustomerService customerService,
           ReCaptchaService reCaptchaService)

      {
         _reCaptchaService = reCaptchaService;
         _ConnectionStrings = ConnectionStrings.Value;
         _promotionService = promotionService;
         _customerService = customerService;
      }

      private ContentResult OkDyn(dynamic obj)
      {
         var ret = JsonConvert.SerializeObject(obj);
         return Content(ret, "application/json");
      }

      [AllowAnonymous]
      [HttpGet("products")]
      public IActionResult GetProducts(int promotionGroupId)
      {
         var ret = _promotionService.GetProducts(promotionGroupId);
         return OkDyn(ret);
      }

      [AllowAnonymous]
      [HttpPost("contactForm")]
      public IActionResult SendContactForm([FromBody] ContactForm contact)
      {
         if (!_reCaptchaService.ValidaReCaptcha(contact.recaptcha))
            return BadRequest(new { result = new { errors = new string[] { "ReCaptcha inválido", "" } } });
         return OkDyn(_promotionService.SendContactForm(contact));
      }

      [AllowAnonymous]
      [HttpGet("sysParameters")]
      public IActionResult GetSysParameters()
      {
         var ret = _promotionService.GetSysParameters();
         return Ok(ret);
      }

      [AllowAnonymous]
      [HttpGet("promotionParameters")]
      public IActionResult GetPromotionParameters(int promotionId)
      {
         var ret = _promotionService.GetPromotionParameters(promotionId);
         return OkDyn(ret);
      }

      [AllowAnonymous]
      [HttpGet("promotionFaq")]
      public IActionResult GetPromotionFaq(int promotionGroupId)
      {
         var ret = _promotionService.GetPromotionFaq(promotionGroupId);
         return Ok(ret);
      }

      [AllowAnonymous]
      [HttpGet("promotionWinners")]
      public IActionResult GetPromotionWinners(int promotionId)
      {
         var ret = _promotionService.GetPromotionWinners(promotionId);
         return OkDyn(ret);
      }

      [HttpGet("customerPriceProducts")]
      public IActionResult CustomerPriceProducts(int promotionId)
      {
         int customerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
         var ret = _promotionService.CustomerPriceProducts(promotionId, customerId);
         return OkDyn(ret);
      }

      [AllowAnonymous]
      [HttpGet("promotionCompanies")]
      public IActionResult GetPromotionCompanies(int promotionId)
      {
         var ret = _promotionService.GetPromotionCompanies(promotionId);
         return OkDyn(ret);
      }

      [AllowAnonymous]
      [HttpGet("promotionCompaniesUF")]
      public IActionResult GetPromotionCompaniesUF(int promotionId)
      {
         var ret = _promotionService.GetPromotionCompaniesUF(promotionId);
         return OkDyn(ret);
      }

      [AllowAnonymous]
      [HttpGet("promotionCompaniesUfCities")]
      public IActionResult GetPromotionCompaniesUfCities(int promotionId, string uf)
      {
         var ret = _promotionService.GetPromotionCompaniesUfCities(promotionId, uf);
         return OkDyn(ret);
      }

      [AllowAnonymous]
      [HttpGet("promotionPopupBanner")]
      public IActionResult GetPromotionPopupBanner(int promotionId)
      {
         var ret = _promotionService.GetPromotionPopupBanner(promotionId);
         return OkDyn(ret);
      }

      [AllowAnonymous]
      [HttpPost("whatsAppCoupom")]
      public IActionResult WhatsAppCoupom([FromBody] WhatsAppCoupomModel whatsAppCoupomModel)
      {
         string base64Image = _promotionService.WhatsAppImage(whatsAppCoupomModel);

         var purchaseReceipt = new PurchaseReceipt()
         {
            CustomerId = whatsAppCoupomModel.customerId,
            PromotionId = whatsAppCoupomModel.promotionId,
            Points = whatsAppCoupomModel.QR,
            AccessCode = "",
            PurchaseNumber = base64Image,
            PurchaseReceiptStatusId = whatsAppCoupomModel.MensagemId
         };

         return OkDyn(_customerService.ReadPurchaseReceipt(purchaseReceipt));
      }

        //[AllowAnonymous]
        //[HttpPost("whatsAppProducts")]
        //public IActionResult WhatsAppProducts([FromBody] WhatsAppProductsModel whatsAppProductsModel)
        //{
        //    var products = new products()
        //    {
        //        CustomerId = whatsAppProductsModel.customerId,
        //        PromotionId = whatsAppProductsModel.promotionId,
        //        PurchaseReceiptId = whatsAppProductsModel.purchasereceiptId,
        //        CaptureDateTime = whatsAppProductsModel.CaptureDateTime,
        //        QrCodeId = whatsAppProductsModel.QrCodeId,
        //        ValueProducts = whatsAppProductsModel.ValueProdutcs,
        //        QttProducts = whatsAppProductsModel.QttProdutcs
        //    };

        //    return OkDyn(_customerService.SaveQrCaptured(products));
        //}

      [AllowAnonymous]
      [HttpPost("WhatsAppSaveQrCaptured")]
      public IActionResult SaveQrCaptured([FromBody] QrCaptured[] qrCaptureds)
      {
            List<dynamic> ret = new List<dynamic>();
            foreach (var qrCaptured in qrCaptureds)
            {
                ret.Add(_customerService.SaveQrCaptured(qrCaptured));
            }
            return OkDyn(ret);
      }

      //[AllowAnonymous]
      //[HttpGet("WhatsAppGetCustomer")]
      //public IActionResult WhatsAppGetCustomer(string appId, string WaCell)
      //{
      //    var ret = _promotionService.WhatsAppGetCustomer(appId, WaCell);
      //    return OkDyn(ret);
      //}

      [AllowAnonymous]
      [HttpGet("WhatsAppValidateInputs")]
      public IActionResult WhatsAppValidateInputs(string appId, string Input, string Field)
      {
            var ret = _promotionService.WhatsAppValidateInputs(appId, Input, Field);
            return OkDyn(ret);
      }

      [AllowAnonymous]
      [HttpPost("WhatsAppPurchaseReceiptFinish")]
      public IActionResult PurchaseReceiptFinish([FromBody] PurchaseReceipt purchaseReceipt)
      {
            return OkDyn(_customerService.PurchaseReceiptFinish(purchaseReceipt));
      }

      [AllowAnonymous]
      [HttpPost("WhatsAppCheckPurchaseRepeat")]
      public IActionResult CheckPurchaseRepeat([FromBody] PurchaseReceipt purchaseReceipt)
      {
             return OkDyn(_customerService.CheckPurchaseRepeat(purchaseReceipt));
      }

      [AllowAnonymous]
      [HttpPost("WhatsAppCreateCustomer")]
      public IActionResult Create([FromBody] Customer customer)
      {
            return OkDyn(_customerService.CreateCustomer(customer));
      }

      [AllowAnonymous]
      [HttpGet("WhatsAppCustomerResults")]
      public IActionResult WhatsAppCustomerResults(int customerId, string Promo)
      {
            var ret = _promotionService.WhatsAppCustomerResults(customerId, Promo);
            return OkDyn(ret);
      }


    }
}

using Google.Apis.Vision.v1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
   [Authorize]
   [ApiController]
   [Route("[controller]")]
   public class CustomerController : ControllerBase
   {
      private ICustomerService _customerService;

      private ConnectionStrings _ConnectionStrings;
      private ReCaptchaService _reCaptchaService;

      public CustomerController(
         ICustomerService customerService,
         IOptions<ConnectionStrings> ConnectionStrings,
         ReCaptchaService reCaptchaService)
      {
         _reCaptchaService = reCaptchaService;
         _customerService = customerService;
         _ConnectionStrings = ConnectionStrings.Value;
      }

      private ContentResult OkDyn(dynamic obj)
      {
         string ret = JsonConvert.SerializeObject(obj);
         return Content(ret, "application/json");
      }


      [AllowAnonymous]
      [HttpPost("validation")]
      public IActionResult Validation([FromBody] ValidationModel model)
      {
         if (model.Type == "DocumentNumber")
         {
            return OkDyn(_customerService.CheckExistDocumentNumber(model.Value, model.AppId));
         }
         else if (model.Type == "CheckCell")
         {
            return OkDyn(_customerService.CheckExistCell(model.Value, model.AppId));
         }
         else
         {
            return OkDyn(_customerService.CheckExistEmail(model.Value, model.AppId));
         }
      }

      [AllowAnonymous]
      [HttpGet("Wa_CheckExistCell")]
      public IActionResult Validation(string Cell, string AppId)
      {
            return OkDyn(_customerService.Wa_CheckExistCell(Cell, AppId));
      }

      [AllowAnonymous]
      [HttpPost("create")]
      public IActionResult Create([FromBody] Customer customer)
      {
         if (!_reCaptchaService.ValidaReCaptcha(customer.recaptcha))
            return BadRequest(new { result = new { errors = new string[] { "ReCaptcha inválido", "" } } });
         return OkDyn(_customerService.CreateCustomer(customer));
      }

      [HttpPost("complement")]
      public IActionResult Complement([FromBody] CustomerComplement customer)
      {
         customer.CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
         return OkDyn(_customerService.SaveCustomerComplement(customer));
      }


      [HttpGet("currentUser")]
      public IActionResult currentUser()
      {
         var ret = _customerService.GetCurrentCustomer(Convert.ToInt32(User.FindFirst("SubjectId")?.Value));

         if (ret == null)
         {
            return Unauthorized();
         }

         return Ok(ret);
      }

      [HttpGet("checkParticipation")]
      public IActionResult CheckParticipation()
      {
         var ret = _customerService.PromotionInstantPrizeCheckParticipation(Convert.ToInt32(User.FindFirst("SubjectId")?.Value));

         if (ret == null)
         {
            return Unauthorized();
         }

         return OkDyn(ret);
      }

      //


      [HttpPost("update")]
      public IActionResult Update([FromBody] Customer customer)
      {
         customer.CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
         return OkDyn(_customerService.UpdateCustomer(customer));
      }

      [HttpPost("ocrpuchasereceipt")]
      public IActionResult ocrPuchaseReceipt([FromBody] PurchaseReceipt purchaseReceipt)
      {
         purchaseReceipt.CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
         return OkDyn(_customerService.ReadPurchaseReceipt(purchaseReceipt));
      }

      [HttpPost("SavePuchasereceiptImage")]
      public IActionResult SavePuchasereceiptImage([FromBody] PurchaseReceipt purchaseReceipt)
      {
         purchaseReceipt.CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
         return OkDyn(_customerService.SavePuchasereceiptImage(purchaseReceipt));
      }

        [HttpGet("participations")]
      public IActionResult GetParticipations(int promotionId)
      {
         var ret = _customerService.GetParticipations(Convert.ToInt32(User.FindFirst("SubjectId")?.Value), promotionId);
         return OkDyn(ret);
      }

      [HttpGet("InstantPrizeParticipations")]
      public IActionResult GetInstantPrizeParticipations(int promotionId)
      {
        var ret = _customerService.GetInstantPrizeParticipations(Convert.ToInt32(User.FindFirst("SubjectId")?.Value), promotionId);
        return OkDyn(ret);
      }

      [HttpGet("participationsNew")]  //teste okada
      public IActionResult GetParticipationsNew()
      {
         var ret = _customerService.GetParticipationsNew(Convert.ToInt32(User.FindFirst("SubjectId")?.Value));
         return OkDyn(ret);
      }

      [HttpPost("SaveCodeCaptured_PrePurchaseReceipt")]
      public IActionResult SaveCodeCaptured_PrePurchaseReceipt([FromBody] PurchaseReceipt purchaseReceipt)
      {
          purchaseReceipt.CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
          return OkDyn(_customerService.SaveCodeCaptured_PrePurchaseReceipt(purchaseReceipt));
      }

        [HttpPost("checkPurchaseRepeat")]
      public IActionResult CheckPurchaseRepeat([FromBody] PurchaseReceipt purchaseReceipt)
      {
         purchaseReceipt.CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
         return OkDyn(_customerService.CheckPurchaseRepeat(purchaseReceipt));
      }

      [HttpPost("saveQrCaptured")]
      public IActionResult SaveQrCaptured([FromBody] QrCaptured[] qrCaptureds)
      {
         List<dynamic> ret = new List<dynamic>();
         foreach (var qrCaptured in qrCaptureds)
         {
            qrCaptured.CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
            ret.Add(_customerService.SaveQrCaptured(qrCaptured));
         }
         return OkDyn(ret);
      }

      [HttpPost("purchaseReceiptFinish")]
      public IActionResult PurchaseReceiptFinish([FromBody] PurchaseReceipt purchaseReceipt)
      {
         if (!_reCaptchaService.ValidaReCaptcha(purchaseReceipt.recaptcha))
            return OkDyn(new { message = "ReCaptcha inválido" });

         purchaseReceipt.CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
         return OkDyn(_customerService.PurchaseReceiptFinish(purchaseReceipt));
      }


      [AllowAnonymous]
      [HttpPost("recoveryPasswordGenerated")]
      public IActionResult RecoveryPasswordGenerated([FromBody] PasswordRecovery passwordRecovery)
      {
         if (!_reCaptchaService.ValidaReCaptcha(passwordRecovery.recaptcha))
            return OkDyn(new { message = "ReCaptcha inválido" });
         return OkDyn(_customerService.RecoveryPasswordGenerated(passwordRecovery.cpf, passwordRecovery.appId, passwordRecovery.promotionId));
      }

      [AllowAnonymous]
      [HttpPost("recoveryPassword")]
      public IActionResult RecoveryPassword([FromBody] RecoveryPasswordModel model)
      {
         return OkDyn(_customerService.RecoveryPassword(model.guid, model.Password, model.appId));
      }

      [AllowAnonymous]
      [HttpPost("CustomerUnsubscribe")]
      public IActionResult CustomerUnsubscribe([FromBody] CustomerUnsubscribeModel model)
      {
        return OkDyn(_customerService.CustomerUnsubscribe(model.guid, model.appId, model.CustomerId, model.Documentnumber));
      }

      [AllowAnonymous]
      [HttpPost("BoletoRegistro")]
      public IActionResult BoletoRegistro([FromBody] BoletoRegistroModel model)
      {
          return OkDyn(_customerService.BoletoRegistro(model.promotionGroupId, model.cnpj_emissor, model.cpf_cliente, model.nr_boleto, model.vl_boleto, model.dt_vcto, model.dt_pagto, model.email, model.customername));
      }

        [AllowAnonymous]
      [HttpGet("instagram")]
      public async System.Threading.Tasks.Task<IActionResult> InstagramAsync(string code)
      {
         HttpClient teste = new HttpClient();
         IList<KeyValuePair<string, string>> nameValueCollection = new List<KeyValuePair<string, string>> {
                { new KeyValuePair<string, string>("client_id", "916334663096177") },
                { new KeyValuePair<string, string>("client_secret", "29f10a0eadece9d6acafec2568c44309") },
                { new KeyValuePair<string, string>("grant_type", "authorization_code") },
                { new KeyValuePair<string, string>("redirect_uri", "https://diadosnamoradosurbano.com.br/dev") },
                { new KeyValuePair<string, string>("code", code) },
            };

         var ret = await teste.PostAsync("https://api.instagram.com/oauth/access_token", new FormUrlEncodedContent(nameValueCollection));
         if (ret.IsSuccessStatusCode)
         {
            var user = JsonConvert.DeserializeObject<dynamic>(ret.Content.ReadAsStringAsync().Result);
            var getIns = "https://graph.instagram.com/" + user.user_id + "?fields=id,username&access_token=" + user.access_token;
            var res = await teste.GetAsync(getIns);
            return OkDyn(JsonConvert.DeserializeObject<dynamic>(res.Content.ReadAsStringAsync().Result));
         }
         return OkDyn(ret.Content.ReadAsStringAsync().Result);
      }

    //  [HttpGet("updatePassword")]
    //  public IActionResult UpdatePassword(string oldPassword, string newPassword)
    //  {
    //     int CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
    //     return OkDyn(_customerService.UpdatePassword(oldPassword, newPassword, CustomerId));
    //  }

      [HttpPost("updatePassword")]
      public IActionResult UpdatePassword([FromBody] UpdatePasswordModel model)
      {
         int CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
         return OkDyn(_customerService.UpdatePassword(model.Password, model.newPassword, CustomerId));
      }

      [HttpPost("customerLoyaltyPoints")]
      public IActionResult CustomerLoyaltyPoints([FromBody] CustomerLoyaltyPointsFilter customerLoyaltyPointsFilter)
      {
         int CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
         return OkDyn(_customerService.ConsCustomerLoyaltyPoints(CustomerId, customerLoyaltyPointsFilter));
      }

      [HttpGet("customerLoyaltyPointsLastBalance")]
      public IActionResult ConsCustomerLoyaltyPointsLastBalance(int promotionId)
      {
         int CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
         return OkDyn(_customerService.ConsCustomerLoyaltyPointsLastBalance(CustomerId, promotionId));
      }

      [HttpGet("loyaltyPrizeCoupons")]
      public IActionResult LoyaltyPrizeCoupons(int promotionId)
      {
         int CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
         return OkDyn(_customerService.LoyaltyPrizeCoupons(CustomerId, promotionId));
      }

      [HttpGet("loyaltyCustomerPrizeCoupons")]
      public IActionResult LoyaltyCustomerPrizeCoupons(int promotionId)
      {
         int CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
         return OkDyn(_customerService.LoyaltyCustomerPrizeCoupons(CustomerId, promotionId));
      }

      [HttpGet("loyaltyCustomerExchangePrizeCoupons")]
      public IActionResult LoyaltyCustomerExchangePrizeCoupons(int promotionId, string cupom)
      {
         int CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
         return OkDyn(_customerService.LoyaltyCustomerExchangePrizeCoupons(CustomerId, promotionId, cupom));
      }

      [HttpGet("loyaltyPrizeCouponDetail")]
      public IActionResult LoyaltyPrizeCouponDetail(int promotionId, int loyaltyprizecouponid)
      {
         int CustomerId = Convert.ToInt32(User.FindFirst("SubjectId")?.Value);
         return OkDyn(_customerService.LoyaltyPrizeCouponDetail(CustomerId, promotionId, loyaltyprizecouponid));
      }

   }
}

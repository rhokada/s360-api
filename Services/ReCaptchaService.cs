using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PromoClicks.Common.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{

   public class ReCaptchaService
   {
      IWebHostEnvironment _env;

      private readonly IOptions<RecaptchaConfig> _recaptchaConfig;

      public ReCaptchaService(
         IOptions<RecaptchaConfig> recaptchaConfig)
      {
         _recaptchaConfig = recaptchaConfig;
      }

      public Boolean ValidaReCaptcha(string recaptcha)
      {
         var client = new HttpClient();
         string SECRET_KEY = _recaptchaConfig.Value.SecretTestKey;
         var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.google.com/recaptcha/api/siteverify?response={recaptcha}&secret={SECRET_KEY}");
         var response = client.SendAsync(request).Result;
         response.EnsureSuccessStatusCode();
         var objResponse = JsonConvert.DeserializeObject<ReCaptchaResponse>(response.Content.ReadAsStringAsync().Result);
         return objResponse.success;
      }
   }
}
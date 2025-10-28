using System;

namespace WebApi.Models
{
   public class ReCaptchaResponse
   {

      public bool success { get; set; }
      public DateTime challenge_ts { get; set; }
      public string hostname { get; set; }

   }
}
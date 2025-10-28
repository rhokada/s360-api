using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Helpers
{
   public class RecaptchaConfig
   {
      public string SecretKey { get; set; }
      public string SecretTestKey { get; set; }
   }
}

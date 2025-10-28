using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Entities
{
   public class PasswordRecovery
   {
      public int promotionId { get; set; }
      public string cpf { get; set; }
      public string appId { get; set; }
      [NotMapped]
      public string recaptcha { get; set; }
   }
}
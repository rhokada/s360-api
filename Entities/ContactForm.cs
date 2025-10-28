using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Entities
{
   public class ContactForm
   {
      public int? ContactFormId { get; set; }
      public string Name { get; set; }
      public string DocumentNumber { get; set; }
      public string Phone { get; set; }
      public string Email { get; set; }
      public string Subject { get; set; }
      public string Message { get; set; }
      public DateTime SentDateTime { get; set; }
      public int? PromotionId { get; set; }
      public string ImageName { get; set; }
      [NotMapped]
      public string recaptcha { get; set; }
   }
}
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Entities
{
   public class SupportRequest
   {
      public string appId { get; set; }
      public string Origin { get; set; }
      public int SupportRequestTypeId { get; set; }
      public string UserEmail { get; set; }
      public string UserName { get; set; }
      public string ToSupport { get; set; }
      public string ToSupportName { get; set; }
      public string From { get; set; }
      public string FromName { get; set; }
//      public int? Userid { get; set; }
      public string? DddCell { get; set; }
      public string? NrCell { get; set; }
      public string Subject { get; set; }
      public string? Message { get; set; }
//      public string Status { get; set; }
//      public string Priority { get; set; }
//      public string? ImageName { get; set; }
//      [NotMapped]
//      public string recaptcha { get; set; }
   }
}
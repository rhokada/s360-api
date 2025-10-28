using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
   public class ValidationModel
   {
      [Required]
      public string Value { get; set; }

      [Required]
      public string AppId { get; set; }

      [Required]
      public string Type { get; set; }
   }
}
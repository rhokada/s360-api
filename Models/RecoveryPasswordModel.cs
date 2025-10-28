using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
   public class RecoveryPasswordModel
   {
      [Required]
      public string guid { get; set; }

      [Required]
      public string appId { get; set; }

      [Required]
      public string Password { get; set; }
   }
}
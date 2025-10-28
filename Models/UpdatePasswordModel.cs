using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
   public class UpdatePasswordModel
   {
      [Required]
      public string Password { get; set; }

      [Required]
      public string newPassword { get; set; }
   }
}
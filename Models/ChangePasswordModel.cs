using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
   public class ChangePasswordModel
   {
      [Required]
      public string Username { get; set; }

      [Required]
      public string Password { get; set; }
      public string NewPassword { get; set; }

      [Required]
      public string AppId { get; set; }
     // [Required]
      //public string recaptcha { get; set; }
   }
}
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
   public class AuthenticateModelSGP
   {
      [Required]
      public string Username { get; set; }

      [Required]
      public string Password { get; set; }

      [Required]
      public string AppId { get; set; }

   }
}
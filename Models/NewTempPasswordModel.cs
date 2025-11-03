using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
   public class NewTempPasswordModel
   {
      [Required]
      public string Username { get; set; }

      [Required]
      public string AppId { get; set; }

   }
}
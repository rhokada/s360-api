using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
   public class AppSupSellersListModel
   {
      [Required]
      public string CodSup { get; set; }
   }
}
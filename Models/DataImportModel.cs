using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Entities
{
   public class DataImport
   {
      public int UserId { get; set; }
      public string AppUuid { get; set; }
      public string Structure { get; set; }
      public string Data { get; set; }
   }
}
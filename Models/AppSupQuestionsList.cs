using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
   public class AppSupQuestionsListModel
    {
      [Required]
      public string SurveyTypeCd { get; set; }
   }
}
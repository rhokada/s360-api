using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface ISurveyQuestionService
    {
        dynamic Select(SurveyQuestionFilterModel filtro);
        dynamic Create(SurveyQuestionCreateModel model);
        dynamic Update(SurveyQuestionUpdateModel model);
        dynamic Delete(int surveyQuestionId);
    }
}

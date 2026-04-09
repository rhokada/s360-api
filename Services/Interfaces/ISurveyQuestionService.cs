using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface ISurveyQuestionService
    {
        dynamic Select(SurveyQuestionFilterModel filtro, string tokenUsuario);
        dynamic Create(SurveyQuestionCreateModel model, string tokenUsuario);
        dynamic Update(SurveyQuestionUpdateModel model, string tokenUsuario);
        dynamic Delete(int surveyQuestionId, string tokenUsuario);
    }
}

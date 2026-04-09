using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface ISurveyService
    {
        dynamic Select(SurveyFilterModel filtro, string tokenUsuario);
        dynamic Create(SurveyCreateModel model, string tokenUsuario);
        dynamic Update(SurveyUpdateModel model, string tokenUsuario);
        dynamic Delete(int surveyId, string tokenUsuario);
    }
}

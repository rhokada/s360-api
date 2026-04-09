using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface ISurveySupService
    {
        dynamic Select(SurveySupFilterModel filtro, string tokenUsuario);
        dynamic Create(SurveySupCreateModel model, string tokenUsuario);
        dynamic Update(SurveySupUpdateModel model, string tokenUsuario);
        dynamic Delete(int surveySupId, string tokenUsuario);
    }
}

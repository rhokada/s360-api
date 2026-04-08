using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface ISurveySupService
    {
        dynamic Select(SurveySupFilterModel filtro);
        dynamic Create(SurveySupCreateModel model);
        dynamic Update(SurveySupUpdateModel model);
        dynamic Delete(int surveySupId);
    }
}

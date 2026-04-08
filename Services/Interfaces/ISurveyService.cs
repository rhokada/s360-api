using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface ISurveyService
    {
        dynamic Select(SurveyFilterModel filtro);
        dynamic Create(SurveyCreateModel model);
        dynamic Update(SurveyUpdateModel model);
        dynamic Delete(int surveyId);
    }
}

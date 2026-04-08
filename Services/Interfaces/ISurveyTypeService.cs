using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface ISurveyTypeService
    {
        dynamic Select(SurveyTypeFilterModel filtro);
        dynamic Create(SurveyTypeCreateModel model);
        dynamic Update(SurveyTypeUpdateModel model);
        dynamic Delete(int surveyTypeId);
    }
}

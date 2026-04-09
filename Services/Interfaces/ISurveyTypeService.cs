using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface ISurveyTypeService
    {
        dynamic Select(SurveyTypeFilterModel filtro, string tokenUsuario);
        dynamic Create(SurveyTypeCreateModel model, string tokenUsuario);
        dynamic Update(SurveyTypeUpdateModel model, string tokenUsuario);
        dynamic Delete(int surveyTypeId, string tokenUsuario);
    }
}

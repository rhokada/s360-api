using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IQuestionOptionService
    {
        dynamic Select(QuestionOptionFilterModel filtro, string tokenUsuario);
        dynamic Create(QuestionOptionCreateModel model, string tokenUsuario);
        dynamic Update(QuestionOptionUpdateModel model, string tokenUsuario);
        dynamic Delete(int questionOptionId, string tokenUsuario);
    }
}

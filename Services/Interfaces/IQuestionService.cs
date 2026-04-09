using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IQuestionService
    {
        dynamic Select(QuestionFilterModel filtro, string tokenUsuario);
        dynamic Create(QuestionCreateModel model, string tokenUsuario);
        dynamic Update(QuestionUpdateModel model, string tokenUsuario);
        dynamic Delete(int questionId, string tokenUsuario);
    }
}

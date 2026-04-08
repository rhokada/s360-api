using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IQuestionService
    {
        dynamic Select(QuestionFilterModel filtro);
        dynamic Create(QuestionCreateModel model);
        dynamic Update(QuestionUpdateModel model);
        dynamic Delete(int questionId);
    }
}

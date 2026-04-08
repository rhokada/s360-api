using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IQuestionOptionService
    {
        dynamic Select(QuestionOptionFilterModel filtro);
        dynamic Create(QuestionOptionCreateModel model);
        dynamic Update(QuestionOptionUpdateModel model);
        dynamic Delete(int questionOptionId);
    }
}

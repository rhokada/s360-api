using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAiPromptService
    {
        dynamic Select(string tokenUsuario);
        dynamic Create(AiPromptCreateModel model, string tokenUsuario);
        dynamic Update(AiPromptUpdateModel model, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
    }
}

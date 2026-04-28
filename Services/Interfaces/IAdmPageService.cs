using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAdmPageService
    {
        dynamic Select(AdmPageFilterModel filtro, string tokenUsuario);
        dynamic Create(AdmPageCreateModel model, string tokenUsuario);
        dynamic Update(AdmPageUpdateModel model, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
    }
}

using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAdmUserService
    {
        dynamic Select(AdmUserFilterModel filtro, string tokenUsuario);
        dynamic Create(AdmUserCreateModel model, string tokenUsuario);
        dynamic Update(AdmUserUpdateModel model, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
    }
}

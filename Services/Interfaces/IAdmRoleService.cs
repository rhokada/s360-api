using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAdmRoleService
    {
        dynamic Select(AdmRoleFilterModel filtro, string tokenUsuario);
        dynamic Create(AdmRoleCreateModel model, string tokenUsuario);
        dynamic Update(AdmRoleUpdateModel model, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
    }
}

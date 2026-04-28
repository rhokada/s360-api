using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAdmRoleUserService
    {
        dynamic Select(AdmRoleUserFilterModel filtro, string tokenUsuario);
        dynamic Create(AdmRoleUserCreateModel model, string tokenUsuario);
        dynamic Delete(int? admRoleUserId, int? admRoleId, int? userId, string tokenUsuario);
    }
}

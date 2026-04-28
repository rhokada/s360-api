using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAdmRolePermissionService
    {
        dynamic Select(int admRoleId, string tokenUsuario);
        dynamic Upsert(AdmRolePermissionUpsertModel model, string tokenUsuario);
    }
}

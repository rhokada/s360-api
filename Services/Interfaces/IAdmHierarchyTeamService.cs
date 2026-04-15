using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAdmHierarchyTeamService
    {
        dynamic Select(AdmHierarchyTeamFilterModel filtro, string tokenUsuario);
        dynamic Create(AdmHierarchyTeamCreateModel model, string tokenUsuario);
        dynamic Update(AdmHierarchyTeamUpdateModel model, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
    }
}

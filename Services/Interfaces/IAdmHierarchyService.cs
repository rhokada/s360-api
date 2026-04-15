using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAdmHierarchyService
    {
        dynamic Select(AdmHierarchyFilterModel filtro, string tokenUsuario);
        dynamic Create(AdmHierarchyCreateModel model, string tokenUsuario);
        dynamic Update(AdmHierarchyUpdateModel model, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
    }
}

using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAdmCompanyDeptService
    {
        dynamic Select(AdmCompanyDeptFilterModel filtro, string tokenUsuario);
        dynamic Create(AdmCompanyDeptCreateModel model, string tokenUsuario);
        dynamic Update(AdmCompanyDeptUpdateModel model, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
    }
}

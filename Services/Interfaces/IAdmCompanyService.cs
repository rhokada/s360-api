using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAdmCompanyService
    {
        dynamic Select(AdmCompanyFilterModel filtro, string tokenUsuario);
        dynamic Create(AdmCompanyCreateModel model, string tokenUsuario);
        dynamic Update(AdmCompanyUpdateModel model, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
    }
}

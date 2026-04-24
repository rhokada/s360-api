using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAdmCustomerService
    {
        dynamic Select(AdmCustomerFilterModel filtro, string tokenUsuario);
        dynamic Create(AdmCustomerCreateModel model, string tokenUsuario);
        dynamic Update(AdmCustomerUpdateModel model, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
    }
}

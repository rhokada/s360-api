using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAdmCustomerSellerService
    {
        dynamic Select(AdmCustomerSellerFilterModel filtro, string tokenUsuario);
        dynamic Create(AdmCustomerSellerCreateModel model, string tokenUsuario);
        dynamic Update(AdmCustomerSellerUpdateModel model, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
    }
}

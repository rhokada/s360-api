using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAdmAddressService
    {
        dynamic Select(AdmAddressFilterModel filtro, string tokenUsuario);
        dynamic Create(AdmAddressCreateModel model, string tokenUsuario);
        dynamic Update(AdmAddressUpdateModel model, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
    }
}

using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAdmDeptUserService
    {
        dynamic Select(AdmDeptUserFilterModel filtro, string tokenUsuario);
        dynamic Create(AdmDeptUserCreateModel model, string tokenUsuario);
        dynamic Update(AdmDeptUserUpdateModel model, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
    }
}

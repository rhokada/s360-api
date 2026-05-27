using System.Threading.Tasks;

namespace WebApi.Services.Interfaces
{
    public interface IAdmHomeService
    {
        Task<dynamic> GetHomeDataAsync(int userId);
    }
}

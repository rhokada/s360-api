using System.Threading.Tasks;

namespace WebApi.Services.Interfaces
{
    public interface IScoreSellerService
    {
        Task<int> ProcessarAsync(string sellerCode, int contractId);
    }
}

using System.IO;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IDataImportSellersService
    {
        dynamic Select(DataImportSellersFilterModel filtro, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
        Task<ImportacaoSellersResultado> ImportarAsync(Stream stream, string fileName, int? userId);
    }
}

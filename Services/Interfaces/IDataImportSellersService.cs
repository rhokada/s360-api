using System.IO;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IDataImportSallersService
    {
        dynamic Select(DataImportSallersFilterModel filtro, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
        Task<ImportacaoSallersResultado> ImportarAsync(Stream stream, string fileName, int? userId);
    }
}

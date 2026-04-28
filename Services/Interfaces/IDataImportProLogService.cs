using System.IO;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IDataImportProLogService
    {
        dynamic Select(DataImportProLogFilterModel filtro, string tokenUsuario);
        dynamic Delete(int id, string tokenUsuario);
        Task<ImportacaoResultado> ImportarAsync(Stream stream, string fileName, int? userId);
    }
}

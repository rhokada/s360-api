using System.Collections.Generic;
using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IDashIndicadoresNotasService
    {
        List<DashNotasRow> Select(int userId, string tokenUsuario);
    }
}

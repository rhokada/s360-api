using System.Collections.Generic;
using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IDashIndicadoresService
    {
        List<DashIndicadoresRow> Select(int userId, string tokenUsuario);
    }
}

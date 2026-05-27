using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Threading.Tasks;
using WebApi.Helpers;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class AdmHomeService : IAdmHomeService
    {
        private readonly ConnectionStrings _connectionStrings;

        public AdmHomeService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public async Task<dynamic> GetHomeDataAsync(int userId)
        {
            using var con = new SqlConnection(_connectionStrings.Default);
            await con.OpenAsync();

            var contractId = await con.QueryFirstOrDefaultAsync<int?>(
                "SELECT TOP 1 ContractId FROM [User] WHERE UserId = @UserId",
                new { UserId = userId }
            );

            if (contractId == null)
                throw new InvalidOperationException("ContractId não encontrado para o usuário.");

            // FOR JSON PATH pode retornar várias linhas para JSONs grandes — concatenar todas
            var parts = await con.QueryAsync<string>(
                "SP_Adm_HomeData",
                new { ContractId = contractId },
                commandType: CommandType.StoredProcedure
            );

            var jsonStr = string.Concat(parts);

            if (string.IsNullOrWhiteSpace(jsonStr))
                return new { };

            return JObject.Parse(jsonStr);
        }
    }
}

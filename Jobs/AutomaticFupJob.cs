using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using System.Threading.Tasks;
using WebApi.Helpers;

namespace WebApi.Jobs
{
    public class AutomaticFupJob
    {
        private readonly ConnectionStrings _connectionStrings;

        public AutomaticFupJob(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public async Task ExecutarAsync()
        {
            using var con = new SqlConnection(_connectionStrings.Default);
            con.Open();
            try
            {
                con.Execute("sp_Process_AutomaticFup", commandType: CommandType.StoredProcedure, commandTimeout: 1800);
            }
            catch
            {
                throw;
            }
            finally
            {
                con.Close();
            }
        }
    }
}

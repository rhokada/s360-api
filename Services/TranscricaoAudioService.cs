using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using WebApi.Helpers;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class TranscricaoAudioService : ITranscricaoAudioService
    {
        private readonly ConnectionStrings _connectionStrings;

        public TranscricaoAudioService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public dynamic Select(string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_TranscricaoAudio", new
                    {
                        TypeRequest   = "SELECT",
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }
    }
}

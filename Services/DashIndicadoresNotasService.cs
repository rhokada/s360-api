using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class DashIndicadoresNotasService : IDashIndicadoresNotasService
    {
        private readonly ConnectionStrings _connectionStrings;

        public DashIndicadoresNotasService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public List<DashNotasRow> Select(int userId, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var raw = con.Query("SP_dash_TimeLineNotesByUser", new
                    {
                        UserId        = userId,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure);

                    return raw.Select(r =>
                    {
                        var d = (IDictionary<string, object>)r;
                        return new DashNotasRow
                        {
                            SubmittedAnswerDetailId = Convert.ToInt32(d["SubmittedAnswerDetailId"] ?? 0),
                            IdItem                  = d["id_item"]?.ToString(),
                            TipoItem                = d["tipo_item"]?.ToString(),
                            ComData                 = d["comdata"]?.ToString(),
                            Texto                   = d["texto"]?.ToString(),
                            AudioArquivo            = d["audio_arquivo"]?.ToString(),
                            ImagemArquivo           = d["imagem_arquivo"]?.ToString(),
                        };
                    }).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }
    }
}

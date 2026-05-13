using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class AiPromptService : IAiPromptService
    {
        private readonly ConnectionStrings _connectionStrings;

        public AiPromptService(IOptions<ConnectionStrings> connectionStrings)
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
                    return con.Query("SP_Adm_AiPrompt", new
                    {
                        TypeRequest   = "SELECT",
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }

        public dynamic Create(AiPromptCreateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_AiPrompt", new
                    {
                        TypeRequest   = "INSERT",
                        model.AiProcessCd,
                        model.Context,
                        model.Prompt,
                        model.Engine,
                        model.Log,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }

        public dynamic Update(AiPromptUpdateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_AiPrompt", new
                    {
                        TypeRequest   = "UPDATE",
                        model.AiPromptId,
                        model.AiProcessCd,
                        model.Context,
                        model.Prompt,
                        model.Engine,
                        model.Log,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }

        public dynamic Delete(int id, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_AiPrompt", new
                    {
                        TypeRequest   = "DELETE",
                        AiPromptId    = id,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }
    }
}

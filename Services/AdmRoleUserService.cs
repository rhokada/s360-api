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
    public class AdmRoleUserService : IAdmRoleUserService
    {
        private readonly ConnectionStrings _connectionStrings;

        public AdmRoleUserService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public dynamic Select(AdmRoleUserFilterModel filtro, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_RoleUser", new
                    {
                        TypeRequest = "SELECT",
                        filtro.AdmRoleId,
                        filtro.UserId,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }

        public dynamic Create(AdmRoleUserCreateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_RoleUser", new
                    {
                        TypeRequest = "INSERT",
                        model.AdmRoleId,
                        model.UserId,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }

        public dynamic Delete(int? admRoleUserId, int? admRoleId, int? userId, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_RoleUser", new
                    {
                        TypeRequest    = "DELETE",
                        AdmRoleUserId  = admRoleUserId,
                        AdmRoleId      = admRoleId,
                        UserId         = userId,
                        token_usuario  = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }
    }
}

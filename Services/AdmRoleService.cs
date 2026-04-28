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
    public class AdmRoleService : IAdmRoleService
    {
        private readonly ConnectionStrings _connectionStrings;

        public AdmRoleService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public dynamic Select(AdmRoleFilterModel filtro, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_Role", new
                    {
                        TypeRequest = "SELECT",
                        filtro.AdmRoleId,
                        filtro.AdmRoleCd,
                        filtro.AdmRoleName,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }

        public dynamic Create(AdmRoleCreateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_Role", new
                    {
                        TypeRequest = "INSERT",
                        model.AdmRoleCd,
                        model.AdmRoleName,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }

        public dynamic Update(AdmRoleUpdateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_Role", new
                    {
                        TypeRequest = "UPDATE",
                        model.AdmRoleId,
                        model.AdmRoleCd,
                        model.AdmRoleName,
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
                    return con.Query("SP_Adm_Role", new
                    {
                        TypeRequest = "DELETE",
                        AdmRoleId   = id,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }
    }
}

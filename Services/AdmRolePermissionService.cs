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
    public class AdmRolePermissionService : IAdmRolePermissionService
    {
        private readonly ConnectionStrings _connectionStrings;

        public AdmRolePermissionService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public dynamic Select(int admRoleId, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_RolePermission", new
                    {
                        TypeRequest = "SELECT",
                        AdmRoleId   = admRoleId,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }

        public dynamic Upsert(AdmRolePermissionUpsertModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_RolePermission", new
                    {
                        TypeRequest = "UPSERT",
                        model.AdmRoleId,
                        model.AdmPageId,
                        model.Read,
                        model.Create,
                        model.Delete,
                        model.Alter,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }
    }
}

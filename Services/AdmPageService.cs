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
    public class AdmPageService : IAdmPageService
    {
        private readonly ConnectionStrings _connectionStrings;

        public AdmPageService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public dynamic Select(AdmPageFilterModel filtro, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_Page", new
                    {
                        TypeRequest = "SELECT",
                        filtro.AdmPageId,
                        filtro.Slug,
                        filtro.Menu,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }

        public dynamic Create(AdmPageCreateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_Page", new
                    {
                        TypeRequest = "INSERT",
                        model.Slug,
                        model.Menu,
                        model.Icon,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }

        public dynamic Update(AdmPageUpdateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    return con.Query("SP_Adm_Page", new
                    {
                        TypeRequest = "UPDATE",
                        model.AdmPageId,
                        model.Slug,
                        model.Menu,
                        model.Icon,
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
                    return con.Query("SP_Adm_Page", new
                    {
                        TypeRequest = "DELETE",
                        AdmPageId   = id,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }
    }
}

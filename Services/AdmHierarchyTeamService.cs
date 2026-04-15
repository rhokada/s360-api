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
    public class AdmHierarchyTeamService : IAdmHierarchyTeamService
    {
        private readonly ConnectionStrings _connectionStrings;

        public AdmHierarchyTeamService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public dynamic Select(AdmHierarchyTeamFilterModel filtro, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_sl_HierarchyTeam", new
                    {
                        filtro.HierarchyTeamId,
                        filtro.HierarchyId,
                        filtro.UserId,
                        filtro.BossId,
                        filtro.Active,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                    return ret;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public dynamic Create(AdmHierarchyTeamCreateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_in_HierarchyTeam", new
                    {
                        model.HierarchyId,
                        model.UserId,
                        model.BossId,
                        model.Active,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                    return ret;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public dynamic Update(AdmHierarchyTeamUpdateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_up_HierarchyTeam", new
                    {
                        model.HierarchyTeamId,
                        model.HierarchyId,
                        model.UserId,
                        model.BossId,
                        model.Active,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                    return ret;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public dynamic Delete(int id, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_dl_HierarchyTeam", new
                    {
                        HierarchyTeamId = id,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure).ToList();
                    return ret;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    con.Close();
                }
            }
        }
    }
}

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
    public class AdmCompanyService : IAdmCompanyService
    {
        private readonly ConnectionStrings _connectionStrings;

        public AdmCompanyService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public dynamic Select(AdmCompanyFilterModel filtro, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_sl_Company", new
                    {
                        filtro.CompanyId,
                        filtro.Name,
                        filtro.TaxID,
                        filtro.GroupCompanyId,
                        filtro.ParentCompanyId,
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

        public dynamic Create(AdmCompanyCreateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_in_Company", new
                    {
                        model.AddressId,
                        model.GroupCompanyId,
                        model.Name,
                        model.TaxID,
                        model.LogoUrl,
                        model.ParentCompanyId,
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

        public dynamic Update(AdmCompanyUpdateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_up_Company", new
                    {
                        model.CompanyId,
                        model.AddressId,
                        model.GroupCompanyId,
                        model.Name,
                        model.TaxID,
                        model.LogoUrl,
                        model.ParentCompanyId,
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
                    var ret = con.Query("SP_Adm_dl_Company", new
                    {
                        CompanyId = id,
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

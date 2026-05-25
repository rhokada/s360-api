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
    public class AdmCustomerService : IAdmCustomerService
    {
        private readonly ConnectionStrings _connectionStrings;

        public AdmCustomerService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public dynamic Select(AdmCustomerFilterModel filtro, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_Customer", new
                    {
                        TypeRequest   = "SELECT",
                        filtro.CustomerId,
                        filtro.CompanyId,
                        filtro.Name,
                        filtro.CustomerCode,
                        filtro.CNPJ,
                        filtro.City,
                        filtro.State,
                        filtro.ToBeConfirmed,
                        filtro.SellerFilter,
                        PageNumber    = filtro.PageNumber ?? 1,
                        PageSize      = filtro.PageSize  ?? 20,
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

        public dynamic Create(AdmCustomerCreateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_Customer", new
                    {
                        TypeRequest = "INSERT",
                        model.CompanyId,
                        model.ToBeConfirmed,
                        model.CustomerCode,
                        model.Name,
                        model.CNPJ,
                        model.Street,
                        model.Street2,
                        model.Neighborhood,
                        model.City,
                        model.State,
                        model.ZipCode,
                        model.OriginCd,
                        model.DataImportId,
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

        public dynamic Update(AdmCustomerUpdateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_Customer", new
                    {
                        TypeRequest = "UPDATE",
                        model.CustomerId,
                        model.CompanyId,
                        model.ToBeConfirmed,
                        model.CustomerCode,
                        model.Name,
                        model.CNPJ,
                        model.Street,
                        model.Street2,
                        model.Neighborhood,
                        model.City,
                        model.State,
                        model.ZipCode,
                        model.OriginCd,
                        model.DataImportId,
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
                    var ret = con.Query("SP_Adm_Customer", new
                    {
                        TypeRequest = "DELETE",
                        CustomerId = id,
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

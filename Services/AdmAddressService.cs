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
    public class AdmAddressService : IAdmAddressService
    {
        private readonly ConnectionStrings _connectionStrings;

        public AdmAddressService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public dynamic Select(AdmAddressFilterModel filtro, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_sl_Address", new
                    {
                        filtro.AddressId,
                        filtro.Street,
                        filtro.Neighborhood,
                        filtro.City,
                        filtro.State,
                        filtro.ZipCode,
                        filtro.Country,
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

        public dynamic Create(AdmAddressCreateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_in_Address", new
                    {
                        model.Street,
                        model.Street2,
                        model.Neighborhood,
                        model.City,
                        model.State,
                        model.ZipCode,
                        model.Country,
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

        public dynamic Update(AdmAddressUpdateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_Adm_up_Address", new
                    {
                        model.AddressId,
                        model.Street,
                        model.Street2,
                        model.Neighborhood,
                        model.City,
                        model.State,
                        model.ZipCode,
                        model.Country,
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
                    var ret = con.Query("SP_Adm_dl_Address", new
                    {
                        AddressId = id,
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

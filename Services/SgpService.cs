using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Services
{
    public interface ISgpService
    {
        dynamic GetPromotionList(int companyId);
        dynamic GetStatusParticipacaoList();
        dynamic GetStatusCupomList();
        dynamic GetStatusEntregaList();
        dynamic PromotionListPurchaseValueValidation(PromotionValidationSgpModel campos);
        dynamic PromotionValidationCustomerData(int PromotionValidationId);
        dynamic PromotionValidationPurchaseData(int PromotionValidationId);
        dynamic PromotionValidationPurchaseProducts(int PromotionValidationId);
        dynamic PromotionValidationPurchaseProductsSEFAZ(int PromotionValidationId);
        dynamic PromotionValidationStatus();
        dynamic PromotionPurchaseValueValidation(int userId, PromotionValidationSgpModel model);
        dynamic PromotionListPrizesToDeliver(PromotionValidationSgpModel campos);
        dynamic ServicesCompany(int userId);
        dynamic ConsCustomer(string cpf, int promotionId);
        dynamic ConsCustomerParticipations(int customerId);
        dynamic PurchaseReceiptProducts​(int purchasereceiptid);
        dynamic PurchaseReceiptValidationData​(int purchasereceiptid);
        dynamic PurchaseReceiptProductsSEFAZ​(int purchasereceiptid);
        dynamic ConsultaCupons(string nomeProc, int purchasereceiptid, int userId);
        dynamic ConsultaFiltros(string nomeProc);
        dynamic ValidationCuponsList(PromotionValidationSgpModel campos, int userId);
        dynamic AppCheckPurchaseReceiptAccessCode(string accessCode);
        dynamic ListStateLinkSEFAZ(string accessCode);
        dynamic PurchaseReceiptValidationFinish(PromotionValidationSgpModel model, int userId);
        dynamic ListPromotionProducts(int PromotionId);
    }
    public class SgpService : ISgpService
    {
        private readonly AppSettings _appSettings;

        private readonly ConnectionStrings _connectionStrings;

        public SgpService(IOptions<AppSettings> appSettings, IOptions<ConnectionStrings> connectionStrings)
        {
            _appSettings = appSettings.Value;
            _connectionStrings = connectionStrings.Value;
        }
        public dynamic GetPromotionList(int companyId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "select promotionId as 'Promotionid', name as 'Promocao' from promotion where companyid = @companyId";

                    var ret = con.Query(query, new { companyId = companyId }, commandType: CommandType.Text).ToList();

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

        public dynamic GetStatusParticipacaoList()
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SELECT validationshortdescription 'StatusParticipacao' FROM dbo.ValidationRegisterStatus";

                    var ret = con.Query(query, new { }, commandType: CommandType.Text).ToList();

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

        public dynamic GetStatusCupomList()
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "select purchasereceiptvalidationstatusshortdescription 'StatusCupom' from PurchaseReceiptvalidationStatus";

                    var ret = con.Query(query, new { }, commandType: CommandType.Text).ToList();

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

        public dynamic GetStatusEntregaList()
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "select PrizeStatusShortDescription 'Entrega' from prizestatus";

                    var ret = con.Query(query, new { }, commandType: CommandType.Text).ToList();

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

        public dynamic PromotionListPurchaseValueValidation(PromotionValidationSgpModel campos)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_PromotionListPurchaseValueValidation";

                    var ret = con.Query(query, new
                    {
                        @promotionId = campos.PromotionId,
                        @dateStart = campos.DateStart,
                        @dateEnd = campos.DateEnd,
                        @ValidationShortDescription = campos.ValidationShortDescription,
                        @PurchaseReceiptValidationStatusShortDescription = campos.PurchaseReceiptValidationStatusShortDescription,
                        @PrizeStatusShortDescription = campos.PrizeStatusShortDescription
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

        public dynamic PromotionValidationCustomerData(int PromotionValidationId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_CustomerData";

                    var ret = con.Query(query, new
                    {
                        PromotionValidationId
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

        public dynamic PromotionValidationPurchaseData(int PromotionValidationId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_PurchaseData";

                    var ret = con.Query(query, new
                    {
                        PromotionValidationId
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

        public dynamic PromotionValidationPurchaseProducts(int PromotionValidationId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_PurchaseProducts";

                    var ret = con.Query(query, new
                    {
                        PromotionValidationId
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

        public dynamic PromotionValidationPurchaseProductsSEFAZ(int PromotionValidationId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_PurchaseProductsSEFAZ";

                    var ret = con.Query(query, new
                    {
                        PromotionValidationId
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

        public dynamic PromotionValidationStatus()
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = @"select PurchaseReceiptValidationStatusId 'Id', PurchaseReceiptValidationStatusShortDescription 'Status' 
                                  from PurchaseReceiptvalidationStatus 
                                 where statustype = 'M'";

                    var ret = con.Query(query, new { }, commandType: CommandType.Text).ToList();

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


        public dynamic PromotionPurchaseValueValidation(int userId, PromotionValidationSgpModel model)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_PromotionPurchaseValueValidation";

                    var ret = con.Query(query, new
                    {
                        userId,
                        @profileId = model.ProfileId,
                        @PromotionValidationId = model.PromotionValidationId,
                        @InputPurchaseReceiptValidationStatusId = model.InputPurchaseReceiptValidationStatusId,
                        @analysis = model.Analysis

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

        public dynamic PromotionListPrizesToDeliver(PromotionValidationSgpModel campos)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_PromotionListPrizesToDeliver";

                    var ret = con.Query(query, new
                    {
                        @promotionId = campos.PromotionId,
                        @dateStart = campos.DateStart,
                        @dateEnd = campos.DateEnd,
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

        public dynamic ServicesCompany(int userId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_ServicesCompany";

                    var ret = con.Query(query, new
                    {
                        @UserId = userId
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

        public dynamic ConsCustomer(string cpf, int promotionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_ConsCustomer";

                    var ret = con.Query(query, new
                    {
                        cpf,
                        promotionId
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

        public dynamic ConsCustomerParticipations(int customerId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_ConsCustomerParticipations";

                    var ret = con.Query(query, new
                    {
                        customerId
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

        public dynamic PurchaseReceiptProducts​(int purchasereceiptid)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_PurchaseReceiptProducts";

                    var ret = con.Query(query, new
                    {
                        purchasereceiptid
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

        public dynamic PurchaseReceiptValidationData​(int purchasereceiptid)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_PurchaseReceiptValidationData";

                    var ret = con.Query(query, new
                    {
                        purchasereceiptid
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

        public dynamic PurchaseReceiptProductsSEFAZ​(int purchasereceiptid)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_PurchaseReceiptProductsSEFAZ";

                    var ret = con.Query(query, new
                    {
                        purchasereceiptid
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

        public dynamic ConsultaCupons(string nomeProc, int purchasereceiptid, int userId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = nomeProc;

                    var ret = con.Query(query, new
                    {
                        purchasereceiptid,
                        userId
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

        public dynamic ConsultaFiltros(string nomeProc)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = nomeProc;

                    var ret = con.Query(query, new
                    {
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

        public dynamic ValidationCuponsList(PromotionValidationSgpModel campos, int userId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_ValidationCuponsList";

                    var ret = con.Query(query, new
                    {
                        userId = userId,
                        promotionId = campos.PromotionId,
                        PurchaseReceiptValidationStatusId = campos.InputPurchaseReceiptValidationStatusId,
                        dt_ini = campos.DateStart,
                        dt_fim = campos.DateEnd,
                        Serie = campos.Serie
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
        //


        public dynamic AppCheckPurchaseReceiptAccessCode(string accessCode)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_AppCheckPurchaseReceiptAccessCode";

                    var ret = con.Query(query, new
                    {
                        @chaveacesso = accessCode
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

        public dynamic ListStateLinkSEFAZ(string accessCode)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_ListStateLinkSEFAZ";

                    var ret = con.Query(query, new
                    {
                        @AccessCode = accessCode
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

        public dynamic PurchaseReceiptValidationFinish(PromotionValidationSgpModel model, int userId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_PurchaseReceiptValidationFinish";

                    var ret = con.Query(query, new
                    {
                        @UserId = userId,
                        @PurchaseReceiptId = model.PurchaseReceiptId,
                        @PurchaseReceiptValidationStatusId = model.InputPurchaseReceiptValidationStatusId,
                        @JSonProductSefaz = model.JSonProductSefaz,
                        @accesscode = model.AccessCode,
                        @notes = model.Notes
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

        public dynamic ListPromotionProducts(int PromotionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SGP_ListPromotionProducts";

                    var ret = con.Query(query, new
                    {
                        promotionid = PromotionId,
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

        //SP_SGP_ListPromotionProducts
        //SP_SGP_PurchaseReceiptValidationFinish 




    }
}

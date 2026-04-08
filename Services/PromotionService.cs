using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{

    public class PromotionService : IPromotionService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications

        private readonly ConnectionStrings _connectionStrings;

        IWebHostEnvironment _env;

        private readonly IOptions<ConfigEmailBase> _configEmail;
        private readonly IOptions<StorageConfig> _storageConfig;

        public PromotionService(IOptions<ConnectionStrings> connectionStrings, IOptions<ConfigEmailBase> configEmail, IOptions<StorageConfig> storageConfig, IWebHostEnvironment env)
        {
            _connectionStrings = connectionStrings.Value;
            _env = env;
            _configEmail = configEmail;
            _storageConfig = storageConfig;
        }

        public dynamic GetProducts(int promotionGroupId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_PromotionProducts";

                    var ret = con.Query<dynamic>(query, new
                    {
                        PromotionGroupId = promotionGroupId
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

        public List<PromotionFaq> GetPromotionFaq(int promotionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SELECT * FROM PromotionFaq Where PromotionId = @PromotionId order by Seq";

                    var ret = con.Query<PromotionFaq>(query, new { PromotionId = promotionId }, commandType: CommandType.Text).ToList();

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

        public List<SysParameters> GetSysParameters()
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SELECT * FROM SysParameters ";

                    var ret = con.Query<SysParameters>(query, new { }, commandType: CommandType.Text).ToList();

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

        public dynamic SendContactForm(ContactForm contactForm)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {

                if (contactForm.ImageName != null && contactForm.ImageName.Trim() != "")
                {
                    //var uploads = Path.Combine(this._env.ContentRootPath, "upload", "ContactForm");
                    //if (!Directory.Exists(uploads))
                    //{
                    //    Directory.CreateDirectory(uploads);
                    //}

                    Guid guidId = Guid.NewGuid();
                    string nomeArquivo = guidId.ToString() + ".jpg";
                    //File.WriteAllBytes(Path.Combine(uploads, nomeArquivo), Convert.FromBase64String(contactForm.ImageName));

                    string imageRet = StorageHelpers.upload(contactForm.ImageName, contactForm.PromotionId + "-" + nomeArquivo, _storageConfig.Value, _storageConfig.Value.ImageContactForm).Result;
                    contactForm.ImageName = imageRet;
                }

                try
                {
                    con.Open();
                    var query = "SP_WebPromo_SaveContactForm";

                    var ret = con.Query<dynamic>(query, contactForm, commandType: CommandType.StoredProcedure).ToList();

                    var resProc = ret.First();

                    // Usa o Titulo da procedure no lugar do assunto fixo
                    string titulo = Convert.ToString(resProc.Msg) ?? "Registro de Contato";

                    string body = "" +
                                   "<b>Nome:</b>  " + contactForm.Name + " <br> " +
                                   "<b>CPF:</b> " + contactForm.DocumentNumber + " <br> " +
                                   "<b>Telefone:</b> " + contactForm.Phone + " <br> " +
                                   "<b>E-mail:</b> " + contactForm.Email + " <br> " +
                                   "<b>Assunto:</b> " + contactForm.Subject + " <br> " +
                                   ((contactForm.ImageName != null && contactForm.ImageName.Trim() != "") ? "<b>Anexo</b> <a href=\"" + contactForm.ImageName + "\">abrir anexo</a> <br>" : "") +
                                   "<b>Mensagem:</b> <br> " + contactForm.Message.Replace("\n", "<br>");


                    var retPromoConf = con.Query<dynamic>("SP_WebPromo_PromotionParameters", new { promotionId = contactForm.PromotionId }, commandType: CommandType.StoredProcedure).ToList();

                    if (retPromoConf.Count() > 0)
                    {
                        var confEmail = this._configEmail.Value;

                        var res = retPromoConf.First();
                        confEmail.FromEmail = Convert.ToString(res.SysEmailFrom);
                        confEmail.FromText = Convert.ToString(res.SysEmailFromText);
                        confEmail.ContactFormToEmail = Convert.ToString(res.ContactFormToEmail);
                        Mail.Send(confEmail.ContactFormToEmail, "Contato", contactForm.Email, contactForm.Name, titulo, body, confEmail);
                    }

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

        public dynamic GetPromotionParameters(int promotionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_PromotionParameters";

                    var ret = con.Query(query, new
                    {
                        promotionId = promotionId
                    }
                    , commandType: CommandType.StoredProcedure).ToList();

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

        public dynamic GetPromotionWinners(int promotionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_ConsCustomersWinners";

                    var ret = con.Query<dynamic>(query, new { PromotionId = promotionId }, commandType: CommandType.StoredProcedure).ToList();

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

        public dynamic CustomerPriceProducts(int promotionId, int customerId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_CustomerPriceProducts";

                    var ret = con.Query(query, new
                    {
                        QtdMeses = 7,
                        PromotionId = promotionId,
                        CustomerId = customerId
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

        public dynamic GetPromotionCompanies(int promotionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_PromotionCompanies";

                    var ret = con.Query(query, new
                    {
                        PromotionId = promotionId,
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

        public dynamic GetPromotionCompaniesUF(int promotionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_PromotionCompaniesUF";

                    var ret = con.Query(query, new
                    {
                        PromotionId = promotionId,
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

        public dynamic GetPromotionCompaniesUfCities(int promotionId, string uf)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_PromotionCompaniesUfCities";

                    var ret = con.Query(query, new
                    {
                        PromotionId = promotionId,
                        Uf = uf
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

        public dynamic GetPromotionPopupBanner(int promotionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_PopupBanner_Img";

                    var ret = con.Query(query, new
                    {
                        PromotionId = promotionId
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

        public string WhatsAppImage(WhatsAppCoupomModel whatsAppCoupomModel)
        {
            var net = new System.Net.WebClient();
            var data = net.DownloadData(whatsAppCoupomModel.UrlImage);
            var content = new System.IO.MemoryStream(data);

            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                content.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            return Convert.ToBase64String(bytes);
        }

        //public dynamic WhatsAppGetCustomer(string appId, string WaCell)
        //{
        //    using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
        //    {
        //        try
        //        {
        //            con.Open();
        //            var query = "SP_Wa_CheckExistCell";

        //            var ret = con.Query(query, new
        //            {
        //                WaCell = WaCell,
        //                appId = appId
        //            }
        //            , commandType: CommandType.StoredProcedure).ToList();

        //            return ret;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //        finally
        //        {
        //            con.Close();
        //        }
        //    }
        //}
        public dynamic WhatsAppValidateInputs(string appId, string Input, string Field)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_Wa_ValidateInputs";

                    var ret = con.Query(query, new
                    {
                        Input = Input,
                        Field = Field,
                        appId = appId
                    }
                    , commandType: CommandType.StoredProcedure).ToList();

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

        public dynamic WhatsAppCustomerResults(int customerId, string Promo)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_Wa_CustomerResults";

                    var ret = con.Query(query, new
                    {
                        customerId = customerId,
                        Promo = Promo
                    }
                    , commandType: CommandType.StoredProcedure).ToList();

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
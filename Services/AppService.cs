using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PromoClicks.Common.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{

    public class AppService : IAppService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications

        private readonly ConnectionStrings _connectionStrings;


        IWebHostEnvironment _env;

        private readonly IOptions<ConfigEmailBase> _configEmail;
        private readonly IOptions<StorageConfig> _storageConfig;

        public AppService(IOptions<ConnectionStrings> connectionStrings, IOptions<ConfigEmailBase> configEmail, IOptions<StorageConfig> storageConfig, IWebHostEnvironment env)
        {
            _connectionStrings = connectionStrings.Value;
            _env = env;
            _configEmail = configEmail;
            _storageConfig = storageConfig;
        }

        //private ContentResult OkAzure(dynamic data)
        //{
        //    var ret = JsonConverter.SerializeObject();
        //}

        public IEnumerable<Customer> GetAll()
        {
            throw new NotImplementedException();
        }

        public dynamic AppUserHomeData(int UserId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_UserHomeData";
                    var ret = con.Query(query, new
                    {
                        UserId = UserId
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

        public dynamic AppSupSellersList(int UserId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SupSellersList";
                    var ret = con.Query(query, new
                    {
                        SupUserId = UserId
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

        public dynamic AppSupCustomersList(int UserId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SupCustomersList";
                    var ret = con.Query(query, new
                    {
                        SupUserId = UserId
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

        public dynamic AppSupQuestionsList(int UserId) //, string SurveyTypeCd)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SupQuestionsList";
                    var ret = con.Query(query, new
                    {
                        SupUserId = UserId
                        //,SurveyTypeCd = SurveyTypeCd
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


        public dynamic AppSaveSupportRequest(SupportRequest supportrequest)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {

 /*               if (contactForm.ImageName != null && contactForm.ImageName.Trim() != "")
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
 */
                try
                {
                    con.Open();
                    var query = "SP_SaveSupportRequest";

                    var ret = con.Query<dynamic>(query, supportrequest, commandType: CommandType.StoredProcedure).ToList();

                    var resProc = ret.First();

                    // Usa o Titulo da procedure no lugar do assunto fixo
                    string titulo = Convert.ToString(resProc.Subject) ?? "Registro de Suporte";

                    string body = "" +
                                   "<b>Nome:</b>  " + supportrequest.UserName + " <br> " +
                                   "<b>Telefone:</b> (" + supportrequest.DddCell + ") " + supportrequest.NrCell + " <br> " +
                                   "<b>E-mail:</b> " + supportrequest.UserEmail + " <br> " +
                                   "<b>Assunto:</b> " + supportrequest.Subject + " <br> " +
                               //    ((contactForm.ImageName != null && contactForm.ImageName.Trim() != "") ? "<b>Anexo</b> <a href=\"" + contactForm.ImageName + "\">abrir anexo</a> <br>" : "") +
//                                   "<b>Mensagem:</b> <br> " + supportrequest.Message.Replace("\n", "<br>");
                                  "<b>Mensagem:</b> <br> " + resProc.Message.Replace("\n", "<br>");

                    var confEmail = this._configEmail.Value;

                    confEmail.FromEmail = Convert.ToString(resProc.From);
                    confEmail.FromText = Convert.ToString(resProc.FromName);

                    Mail.Send(resProc.ToSupport, resProc.ToSupportName, supportrequest.UserEmail, supportrequest.UserName, titulo, body, confEmail);

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

       

        public dynamic AppSupportRequestTypeList ()
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SupportRequestTypeList";

                    var ret = con.Query<dynamic>(query, new { }, commandType: CommandType.StoredProcedure).ToList();

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
        //######################################################################################
        public dynamic CheckExistEmail(string Email, string AppId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_AppCheckExistCpfCnpjEmailCell";

                    var ret = con.Query(query, new
                    {
                        Email = Email,
                        AppId = AppId
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

        public dynamic Wa_CheckExistCell(string Cell, string AppId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_Wa_CheckExistCell";

                    var ret = con.Query(query, new
                    {
                        Cell = Cell,
                        AppId = AppId
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

        public dynamic CheckExistCell(string Cell, string AppId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_AppCheckExistCpfCnpjEmailCell";

                    var ret = con.Query(query, new
                    {
                        DddCell = Cell,
                        AppId = AppId
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

        public object CheckExistDocumentNumber(string DocumentNumber, string AppId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_AppCheckExistCpfCnpjEmailCell";

                    var ret = con.Query<object>(query, new
                    {
                        Cpf = DocumentNumber,
                        AppId = AppId
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

        public dynamic CreateCustomer(Customer customer)
        {

            customer.Password = Util.CalculateMD5Hash(("AppleStore@promo-clicks.com").ToLower(), customer.Password);

            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_CreateCustomer";

                    var ret = con.Query(query, customer, commandType: CommandType.StoredProcedure).ToList();

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

        public dynamic SaveCustomerComplement(CustomerComplement customerComplement)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_SaveCustomerComplement";

                    var ret = con.Query(query, customerComplement, commandType: CommandType.StoredProcedure).ToList();

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

        public Customer GetCurrentCustomer(int CustomerId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_GetCurrentCustomer";

                    var ret = con.Query<Customer>(query, new { CustomerId = CustomerId }, commandType: CommandType.StoredProcedure).FirstOrDefault();

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

        public dynamic UpdateCustomer(Customer customer)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_UpdateCustomer";

                    var ret = con.Query(query, customer, commandType: CommandType.StoredProcedure).ToList();

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

        public dynamic ReadPurchaseReceipt(PurchaseReceipt purchaseReceipt)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_PurchaseReceiptCreateAndOCR";
                    // 25/09/2024 - alteracao: nao chamar o vision se nao tem img
                    if (purchaseReceipt.PurchaseNumber != "") 
                    {
                        var ocrObj = GoogleVision.DetectText(purchaseReceipt.PurchaseNumber);
                        purchaseReceipt.PurchaseCaptured = JsonConvert.SerializeObject(ocrObj);
                    }
                     else
                    {
                        purchaseReceipt.PurchaseCaptured = "IMAGEM NAO ENVIADA";
                    }

                    var ret = con.Query(query, new
                    {
                        PurchaseCaptured = purchaseReceipt.PurchaseCaptured,
                        CustomerId = purchaseReceipt.CustomerId,
                        PurchaseReceiptTypeId = purchaseReceipt.Points,
                        WaMensagemId = purchaseReceipt.PurchaseReceiptStatusId
                    }, commandType: CommandType.StoredProcedure).ToList();

                    if (ret.Count() > 0 && purchaseReceipt.PurchaseNumber != "")   // 25/09/2024 - alteracao: nao chamar o vision se nao tem img
                    {
                        //var uploads = Path.Combine(this._env.ContentRootPath, "upload", "COO");
                        //if (!Directory.Exists(uploads))
                        //{
                        //    Directory.CreateDirectory(uploads);
                        //}

                        //File.WriteAllBytes(Path.Combine(uploads, Convert.ToString(ret.First().PurchaseReceiptId) + "-1.jpg"), Convert.FromBase64String(purchaseReceipt.PurchaseNumber));

                        Guid guidId = Guid.NewGuid();
                        string nomeArquivo = purchaseReceipt.PromotionId + "-" + Convert.ToString(ret.First().PurchaseReceiptId) + "-" + guidId.ToString() + ".jpg";

                        string imageRet = StorageHelpers.upload(purchaseReceipt.PurchaseNumber, nomeArquivo, _storageConfig.Value, _storageConfig.Value.ImagePurchaseReceipt).Result;

                        var saveImage = con.Query("SP_WebPromo_PurchaseReceiptComplements", new
                        {
                            purchaseReceiptId = ret.First().PurchaseReceiptId,
                            PhotoFileName = imageRet
                        }, commandType: CommandType.StoredProcedure).ToList();

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

        public dynamic SavePuchasereceiptImage(PurchaseReceipt purchaseReceipt)
            // so salva a immg do cupom e atualiza photfile name
        {
            if (purchaseReceipt.PurchaseNumber != "")   // 25/09/2024 - alteracao: nao chamar o vision se nao tem img
            {
                var uploads = Path.Combine(this._env.ContentRootPath, "upload", "COO");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                File.WriteAllBytes(Path.Combine(uploads, Convert.ToString(purchaseReceipt.PurchaseReceiptId) + "-1.jpg"), Convert.FromBase64String(purchaseReceipt.PurchaseNumber));

                Guid guidId = Guid.NewGuid();
                string nomeArquivo = purchaseReceipt.PromotionId + "-" + Convert.ToString(purchaseReceipt.PurchaseReceiptId) + "-" + guidId.ToString() + ".jpg";

                string imageRet = StorageHelpers.upload(purchaseReceipt.PurchaseNumber, nomeArquivo, _storageConfig.Value, _storageConfig.Value.ImagePurchaseReceipt).Result;

                using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
                {
                    try
                    {
                        con.Open();
                        var saveImage = con.Query("SP_WebPromo_PurchaseReceiptComplements", new
                        {
                            purchasereceiptid = purchaseReceipt.PurchaseReceiptId,
                            PhotoFileName = imageRet
                        }, commandType: CommandType.StoredProcedure).ToList();
                        return saveImage;
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
            return null;
        }


        public dynamic GetParticipations(int customerId, int promotionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var queryProc = "SP_ConsPromotionParamaters";
                    var retProc = con.Query(queryProc, new
                    {
                        promotionId = promotionId
                    }
                    , commandType: CommandType.StoredProcedure).ToList();

                    var query = "SP_ConsCustomerInstantPrizeParticipations";
                    if (retProc.Count() > 0)
                    {
                        query = Convert.ToString(retProc.First().PROC_GetParticipations);
                    }


                    var ret = con.Query(query, new
                    {
                        CustomerId = customerId
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

        public dynamic GetInstantPrizeParticipations(int customerId, int promotionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var queryProc = "SP_ConsPromotionParamaters";
                    var retProc = con.Query(queryProc, new
                    {
                        promotionId = promotionId
                    }
                    , commandType: CommandType.StoredProcedure).ToList();

                    var query = "";
                    if (retProc.Count() > 0)
                    {
                        query = Convert.ToString(retProc.First().PROC_GetInstantPrizeParticipations);
                    }


                    var ret = con.Query(query, new
                    {
                        CustomerId = customerId
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

        public dynamic GetParticipationsNew(int customerId) //teste de inclusao de nova chamada a proc
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_ConsCustomerInstantPrizeParticipations";

                    var ret = con.Query(query, new
                    {
                        CustomerId = customerId
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

        public dynamic SaveCodeCaptured_PrePurchaseReceipt(PurchaseReceipt purchaseReceipt)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_SaveCodeCaptured_PrePurchaseReceipt";
                    var ret = con.Query(query, new
                    {
                        CustomerId = purchaseReceipt.CustomerId,
                        PromotionId = purchaseReceipt.PromotionId,
                        CodeCaptured = purchaseReceipt.AccessCode
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


        public dynamic CheckPurchaseRepeat(PurchaseReceipt purchaseReceipt)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();

                    if (purchaseReceipt.PurchaseReceiptStatusId == 1)
                    {
                        var query = "SP_AppCheckPurchaseReceiptRepeated";
                        var ret = con.Query(query, new
                        {
                            chaveacesso = purchaseReceipt.AccessCode,
                            Datacompra = purchaseReceipt.PurchaseCaptured,
                            CustomerId = purchaseReceipt.CustomerId,
                            PromotionId = purchaseReceipt.PromotionId
                        }, commandType: CommandType.StoredProcedure).ToList();

                        if (ret.Count() > 0)
                        {
                            if (Convert.ToInt32(ret.First().Retorno) == 1)
                            {
                                var subQuery = "SP_WebPromo_CheckPurchaseReceiptAccessCode";
                                return con.Query(subQuery, new
                                {
                                    chaveacesso = purchaseReceipt.AccessCode,
                                    PurchaseReceiptId = purchaseReceipt.PurchaseReceiptId
                                }, commandType: CommandType.StoredProcedure).ToList();
                            }
                        }

                        return ret;
                    }
                    else
                    {
                        var query = "SP_AppCheckPurchaseReceiptRepeated";
                        var ret = con.Query(query, new
                        {
                            CustomerId = purchaseReceipt.CustomerId,
                            Cnpj = purchaseReceipt.SellerId,
                            Coo = purchaseReceipt.PurchaseNumber,
                            Datacompra = purchaseReceipt.PurchaseCaptured,
                            PromotionId = purchaseReceipt.PromotionId
                        }, commandType: CommandType.StoredProcedure).ToList();

                        return ret;
                    }
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

        public dynamic SaveQrCaptured(QrCaptured qrCaptured)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_SaveQrCaptured";

                    var ret = con.Query(query, new
                    {
                        QRCodeId = qrCaptured.QRCodeId,
                        PromotionId = qrCaptured.PromotionId,
                        CustomerId = qrCaptured.CustomerId,
                        PurchaseReceiptId = qrCaptured.PurchaseReceiptId,
                        QttProducts = qrCaptured.Qtt,
                        CaptureDateTime = qrCaptured.CaptureDateTime,
                        ValueProducts = qrCaptured.ValueProducts
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

        public dynamic PurchaseReceiptFinish(PurchaseReceipt purchaseReceipt)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var queryProc = "SP_ConsPromotionParamaters";
                    var retProc = con.Query(queryProc, new
                    {
                        promotionId = purchaseReceipt.PromotionId
                    }
                    , commandType: CommandType.StoredProcedure).ToList();

                    var query = "SP_WebWa_PurchaseReceiptFinish";// "SP_WebPromo_PurchaseReceiptFinish";
                    if (retProc.Count() > 0)
                    {
                        query = Convert.ToString(retProc.First().PROC_PurchaseReceiptFinish);
                    }                    

                    var ret = con.Query(query, new
                    {
                        customerId = purchaseReceipt.CustomerId,
                        promotionId = purchaseReceipt.PromotionId,
                        purchasereceiptId = purchaseReceipt.PurchaseReceiptId,
                        purchaseCnpj = purchaseReceipt.SellerId,
                        purchaseDate = purchaseReceipt.PurchaseCaptured,
                        purchaseNumber = purchaseReceipt.PurchaseNumber,
                        accesscode = purchaseReceipt.AccessCode,
                        purchaseCaptured = purchaseReceipt.PurchaseStatus
                    }
                    , commandType: CommandType.StoredProcedure).ToList();

                    try
                    {
                        if (ret.Count() > 0)
                        {
                            var item = JsonConvert.DeserializeObject<dynamic>(Convert.ToString(ret.First().Email));

                            var confEmail = this._configEmail.Value;

                            confEmail.FromEmail = Convert.ToString(item.From);
                            confEmail.FromText = Convert.ToString(item.NameFrom);

                            Mail.Send(Convert.ToString(item.To), Convert.ToString(item.NameTo), "", "", Convert.ToString(item.Subject), Convert.ToString(item.Message), confEmail);
                        }
                    }
                    catch { }

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

        public dynamic RecoveryPasswordGenerated(string cpf, string appId, int promotionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_RecoveryPasswordGeneratedGuid";

                    var ret = con.Query(query, new
                    {
                        cpf = cpf,
                        appId = appId
                    }
                    , commandType: CommandType.StoredProcedure).ToList();

                    foreach (var item in ret)
                    {
                        if (Convert.ToInt32(item.CustomerId) > 0)
                        {

                            var retPromoConf = con.Query<dynamic>("SP_WebPromo_PromotionParameters", new { promotionId = promotionId }, commandType: CommandType.StoredProcedure).ToList();

                            if (retPromoConf.Count() > 0)
                            {
                                var confEmail = this._configEmail.Value;

                                var res = retPromoConf.First();
                                confEmail.FromEmail = Convert.ToString(res.SysEmailFrom);
                                confEmail.FromText = Convert.ToString(res.SysEmailFromText);
                                confEmail.ContactFormToEmail = Convert.ToString(res.ContactFormToEmail);
                                confEmail.UrlRecuperarSenha = Convert.ToString(res.UrlPswRecovery);

                                string html = Convert.ToString(res.TemplatePswRecovery);
                                Mail.Send(Convert.ToString(item.Email), Convert.ToString(item.CustomerName), "", "", "Recuperação de Senha", html.Replace("LINK", confEmail.UrlRecuperarSenha + "?guid=" + Convert.ToString(item.DeviceToken)), confEmail);
                                item.Email = item.Email2;
                            }
                           
                        }
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

        public dynamic RecoveryPassword(string guid, string password, string appId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_RecoveryPasswordWithGuid";

                    var ret = con.Query(query, new
                    {
                        guid = guid,
                        password = Util.CalculateMD5Hash(("AppleStore@promo-clicks.com").ToLower(), password),
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

        public dynamic CustomerUnsubscribe(string guid, string appId, int CustomerId, string Documentnumber)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_CustomerUnsubscribe";

                    var ret = con.Query(query, new
                    {
                        guid = guid,
                        appId = appId,
                        CustomerId = CustomerId,
                        Documentnumber = Documentnumber
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


        public dynamic UpdatePassword(string oldPassword, string newPassword, int customerId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_CustomerUpdatePassword";

                    var ret = con.Query(query, new
                    {
                        CustomerId = customerId,
                        OldPassword = Util.CalculateMD5Hash(("AppleStore@promo-clicks.com").ToLower(), oldPassword),
                        NewPassword = Util.CalculateMD5Hash(("AppleStore@promo-clicks.com").ToLower(), newPassword)
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

        public dynamic BoletoRegistro(int promotionGroupId, string cnpj_emissor, string cpf_cliente, string nr_boleto, string vl_boleto, string dt_vcto, string dt_pagto, string email, string customername)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_BoletoRegistro";

                    var ret = con.Query(query, new
                    {
                        promotionGroupId = promotionGroupId,
                        cnpj_emissor = cnpj_emissor,
                        cpf_cliente = cpf_cliente,
                        nr_boleto = nr_boleto,
                        vl_boleto = vl_boleto,
                        dt_vcto = dt_vcto,
                        dt_pagto = dt_pagto,
                        @email = email,
                        @customername = customername
                    }
                    , commandType: CommandType.StoredProcedure).ToList();


                    try
                    {
                        if (ret.Count() > 0)
                        {
                            var item = JsonConvert.DeserializeObject<dynamic>(Convert.ToString(ret.First().Email));

                            var confEmail = this._configEmail.Value;

                            confEmail.FromEmail = Convert.ToString(item.From);
                            confEmail.FromText = Convert.ToString(item.NameFrom);

                            Mail.Send(Convert.ToString(item.To), Convert.ToString(item.NameTo), "", "", Convert.ToString(item.Subject), Convert.ToString(item.Message), confEmail);

                        }
                    }
                    catch { }

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


        public dynamic PromotionInstantPrizeCheckParticipation(int customerId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_PromotionInstantPrizeCheckParticipation_DiaNamoradosUrbano";

                    var ret = con.Query(query, new
                    {
                        CustomerId = customerId,
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

        public dynamic ConsCustomerLoyaltyPoints(int customerId, CustomerLoyaltyPointsFilter dados)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_ConsCustomerLoyaltyPoints";

                    var ret = con.Query(query, new
                    {
                        CustomerId = customerId,
                        @promotionid = dados.PromotionId,
                        @daysfilter = dados.DaysFilter,
                        @monthyearfilter = dados.MonthYearFilter
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

        public dynamic ConsCustomerLoyaltyPointsLastBalance(int customerId, int promotionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_ConsCustomerLoyaltyPointsLastBalance";

                    var ret = con.Query(query, new
                    {
                        CustomerId = customerId,
                        @promotionid = promotionId
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

        public dynamic LoyaltyPrizeCoupons(int customerId, int promotionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_LoyaltyPrizeCoupons";

                    var ret = con.Query(query, new
                    {
                        CustomerId = customerId,
                        @promotionid = promotionId
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

        public dynamic LoyaltyCustomerPrizeCoupons(int customerId, int promotionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_LoyaltyCustomerPrizeCoupons";

                    var ret = con.Query(query, new
                    {
                        CustomerId = customerId,
                        @promotionid = promotionId
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

        public dynamic LoyaltyCustomerExchangePrizeCoupons(int customerId, int promotionId, string cdcoupon)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_LoyaltyCustomerExchangePrizeCoupons";

                    var ret = con.Query(query, new
                    {
                        CustomerId = customerId,
                        @promotionid = promotionId,
                        cdcoupon = cdcoupon
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

        public dynamic LoyaltyPrizeCouponDetail(int customerId, int promotionId, int loyaltyprizecouponid)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_WebPromo_LoyaltyPrizeCouponDetail";

                    var ret = con.Query(query, new
                    {
                        CustomerId = customerId,
                        @promotionid = promotionId,
                        loyaltyprizecouponid = loyaltyprizecouponid
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
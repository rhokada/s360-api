using Dapper;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PromoClicks.Common.Helper;
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
using System.Diagnostics;


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

        public dynamic AppSupSellerFupList(int UserId) //, string SurveyTypeCd)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_SupSellerFupList";
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

        //public dynamic AppDataImport(DataImport dataimport)
        //{
        //    using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
        //    {
        //        try
        //        {
        //            con.Open();
        //            var query = "SP_DataImport";

        //            var ret = con.Query<dynamic>(query, dataimport, commandType: CommandType.StoredProcedure).ToList();

        //            return ret;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //        finally
        //        {
        //            con.Close();
        //        //}
        //        //}

        //        // --- Parte 2: Processar os �udios WebM na propriedade Data ---
        //            JToken parsedData = null;
        //            string rawDataString = dataimport.Data.ToString();
        //            try
        //            {
        //                // Tenta analisar como JArray primeiro, pois o �ltimo exemplo mostrou um array.
        //                parsedData = JArray.Parse(rawDataString);
        //            }
        //            catch (JsonReaderException) // Catch espec�fico para erro de leitura JSON
        //            {
        //                // Se n�o foi um array, tenta analisar como um JObject (para a estrutura original, se aplic�vel)
        //                try
        //                {
        //                    parsedData = JObject.Parse(rawDataString);
        //                }
        //                catch (JsonReaderException ex)
        //                {
        //                    Console.WriteLine($"Erro ao analisar JSON da propriedade Data: N�o � um JArray nem um JObject v�lido. {ex.Message}. Conte�do da Data: {rawDataString}");
        //                    //return ret; // N�o conseguiu analisar, retorna o resultado da SP
        //                }
        //                catch (Exception ex) // Outras exce��es inesperadas ao tentar JObject.Parse
        //                {
        //                    Console.WriteLine($"Erro inesperado ao tentar analisar JSON como JObject: {ex.Message}. Conte�do da Data: {rawDataString}");
        //                    //return ret;
        //                }
        //            }
        //            catch (Exception ex) // Outras exce��es inesperadas ao tentar JArray.Parse
        //            {
        //                Console.WriteLine($"Erro inesperado ao tentar analisar JSON como JArray: {ex.Message}. Conte�do da Data: {rawDataString}");
        //                //return ret;
        //            }

        //            // Se parsedData ainda for null, houve um erro no parsing.
        //            if (parsedData == null)
        //            {
        //                Console.WriteLine("N�o foi poss�vel extrair um JSON v�lido da propriedade 'Data' para processamento de �udio.");
        //                //return ret;
        //            }

        //            // Usa a fun��o auxiliar para encontrar todos os objetos de anexo de �udio
        //            foreach (var attachmentObject in FindAllAudioAttachments(parsedData))
        //            {
        //                if (attachmentObject.TryGetValue("filepath", StringComparison.OrdinalIgnoreCase, out var filepathToken) &&
        //                    attachmentObject.TryGetValue("filename", StringComparison.OrdinalIgnoreCase, out var filenameToken))
        //                {
        //                    string audioBase64String = filepathToken.ToString();
        //                    string originalFileName = filenameToken.ToString();

        //                    int userId = 0; // Valor padr�o caso n�o encontre ou para testes.
        //                    if (dataimport.GetType().GetProperty("UserId") != null)
        //                    {
        //                        userId = (int)dataimport.GetType().GetProperty("UserId").GetValue(dataimport, null);
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("A propriedade 'UserId' n�o foi encontrada no objeto 'DataImport'. O nome do arquivo usar� '0' como ID de usu�rio.");
        //                        // Voc� pode lan�ar uma exce��o ou usar um ID gen�rico/default, ou buscar em outro lugar.
        //                    }

        //                    string baseOriginalFileName = Path.GetFileNameWithoutExtension(originalFileName);
        //                    string newFormattedFileName = $"AUDIO-{userId}-{baseOriginalFileName}.webm";

        //                    try
        //                    {
        //                        dynamic saveResult = AppSaveAudioWebmFromBase64(audioBase64String, newFormattedFileName);
        //                        Console.WriteLine($"Processamento de �udio '{newFormattedFileName}': Sucesso = {saveResult.success}, Mensagem = {saveResult.message}");
        //                    }
        //                    catch (Exception audioEx)
        //                    {
        //                        Console.WriteLine($"Erro ao salvar anexo de �udio WEBM '{newFormattedFileName}': {audioEx.Message}");
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        public dynamic AppDataImport(DataImport dataimport)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_DataImport";

                    var ret = con.Query<dynamic>(query, dataimport, commandType: CommandType.StoredProcedure).ToList();

                    // --- Parte 1: An�lise e Desserializa��o do JSON da propriedade Data ---
                    JToken parsedData = null;
                    string rawDataString = dataimport.Data.ToString();
                    try
                    {
                        // Tenta analisar como JArray primeiro, pois os exemplos mostraram um array.
                        parsedData = JArray.Parse(rawDataString);
                    }
                    catch (JsonReaderException) // Catch espec�fico para erro de leitura JSON
                    {
                        // Se n�o foi um array, tenta analisar como um JObject (para a estrutura original, se aplic�vel)
                        try
                        {
                            parsedData = JObject.Parse(rawDataString);
                        }
                        catch (JsonReaderException ex)
                        {
                            Console.WriteLine($"Erro ao analisar JSON da propriedade Data: N�o � um JArray nem um JObject v�lido. {ex.Message}. Conte�do da Data: {rawDataString}");
                            // return ret; // N�o conseguiu analisar, retorna o resultado da SP
                        }
                        catch (Exception ex) // Outras exce��es inesperadas ao tentar JObject.Parse
                        {
                            Console.WriteLine($"Erro inesperado ao tentar analisar JSON como JObject: {ex.Message}. Conte�do da Data: {rawDataString}");
                            // return ret;
                        }
                    }
                    catch (Exception ex) // Outras exce��es inesperadas ao tentar JArray.Parse
                    {
                        Console.WriteLine($"Erro inesperado ao tentar analisar JSON como JArray: {ex.Message}. Conte�do da Data: {rawDataString}");
                        // return ret;
                    }

                    // Se parsedData ainda for null, houve um erro no parsing.
                    if (parsedData == null)
                    {
                        Console.WriteLine("N�o foi poss�vel extrair um JSON v�lido da propriedade 'Data' para processamento de anexos.");
                        return ret; // Retorna o resultado da SP
                    }

                    // --- Extra��o do CustomerId e SellerCode ---
                    // Tenta obter o CustomerId e SellerCode do JSON.
                    //int customerId = 0; // Mantido caso necess�rio para outras l�gicas, mas n�o para o nome do arquivo
                    string sellerCode = "UNKNOWN"; // Valor padr�o para caso n�o encontre
                    string dtSurvey = "NODATE"; // Valor padr�o para caso n�o encontre

                    if (parsedData is JArray jArray && jArray.Any())
                    {
                        var firstItem = jArray.First as JObject;
                        if (firstItem != null)
                        {
                            //if (firstItem.TryGetValue("customerId", StringComparison.OrdinalIgnoreCase, out var customerIdToken))
                            //{
                            //    customerId = customerIdToken.ToObject<int>();
                            //}
                            if (firstItem.TryGetValue("sellerCode", StringComparison.OrdinalIgnoreCase, out var sellerCodeToken) && !string.IsNullOrWhiteSpace(sellerCodeToken.ToString()))
                            {
                                sellerCode = sellerCodeToken.ToString();
                            }
                            // Extrai DtSurvey
                            if (firstItem.TryGetValue("DtSurvey", StringComparison.OrdinalIgnoreCase, out var dtSurveyToken) && !string.IsNullOrWhiteSpace(dtSurveyToken.ToString()))
                            {
                                dtSurvey = dtSurveyToken.ToString();
                            }
                        }
                    }
                    else if (parsedData is JObject jObject)
                    {
                        //if (jObject.TryGetValue("customerId", StringComparison.OrdinalIgnoreCase, out var customerIdToken))
                        //{
                        //    customerId = customerIdToken.ToObject<int>();
                        //}
                        if (jObject.TryGetValue("sellerCode", StringComparison.OrdinalIgnoreCase, out var sellerCodeToken) && !string.IsNullOrWhiteSpace(sellerCodeToken.ToString()))
                        {
                            sellerCode = sellerCodeToken.ToString();
                        }
                        // Extrai DtSurvey
                        if (jObject.TryGetValue("DtSurvey", StringComparison.OrdinalIgnoreCase, out var dtSurveyToken) && !string.IsNullOrWhiteSpace(dtSurveyToken.ToString()))
                        {
                            dtSurvey = dtSurveyToken.ToString();
                        }
                    }

                    if (string.Equals(sellerCode, "UNKNOWN", StringComparison.OrdinalIgnoreCase)) // Se sellerCode ainda n�o foi encontrado no JSON
                    {
                        Console.WriteLine("A propriedade 'sellerCode' n�o foi encontrada no JSON da propriedade 'Data'. O nome do arquivo usar� 'UNKNOWN' como c�digo do vendedor.");
                        // Aqui voc� poderia tentar obter de dataimport se existisse, ou de um contexto de usu�rio.
                    }
                    if (string.Equals(dtSurvey, "NODATE", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("A propriedade 'DtSurvey' n�o foi encontrada no JSON da propriedade 'Data'. O nome do arquivo usar� 'NODATE' como data.");
                    }

                    // --- Parte 2 e 3: Processar os �udios e imagens na propriedade Data ---
                    // Usa a fun��o auxiliar para encontrar todos os objetos de anexo
                    foreach (var attachmentObject in FindAllAttachments(parsedData))
                    {
                        if (attachmentObject.TryGetValue("filepath", StringComparison.OrdinalIgnoreCase, out var filepathToken) &&
                            attachmentObject.TryGetValue("filename", StringComparison.OrdinalIgnoreCase, out var filenameToken) &&
                            attachmentObject.TryGetValue("type", StringComparison.OrdinalIgnoreCase, out var attachmentTypeToken))
                        {
                            string base64String = filepathToken.ToString();
                            string originalFileName = filenameToken.ToString();
                            string attachmentType = attachmentTypeToken.ToString();

                            // Gera um novo GUID para a unicidade
                            //Guid newGuid = Guid.NewGuid();

                            // Extrai o nome do arquivo original sem extens�o para evitar duplicidades na extens�o
                            string baseOriginalFileName = Path.GetFileNameWithoutExtension(originalFileName);
                            string formattedFileName;

                            if (attachmentType.Equals("audio", StringComparison.OrdinalIgnoreCase))
                            {
                                // Formata o nome para �udio: AUDIO-sellercode-nomeoriginal.webm
                                formattedFileName = $"{baseOriginalFileName}.webm";
                                try
                                {
                                    dynamic saveResult = AppSaveAudioWebmFromBase64(base64String, formattedFileName);
                                    Console.WriteLine($"Processamento de �udio '{formattedFileName}': Sucesso = {saveResult.success}, Mensagem = {saveResult.message}");
                                }
                                catch (Exception audioEx)
                                {
                                    Console.WriteLine($"Erro ao salvar anexo de �udio WEBM '{formattedFileName}': {audioEx.Message}");
                                }
                            }
                            // --- Parte 3: Processamento de imagens ---
                            else if (attachmentType.Equals("image", StringComparison.OrdinalIgnoreCase))
                            {
                                // Formata o nome para imagem: IMAGE-sellercode-nomeoriginal.jpg
                                // Note: Mesmo que o original seja .png, o requisito � salvar como .jpg
                                formattedFileName = $"{baseOriginalFileName}.jpg";
                                try
                                {
                                    dynamic saveResult = AppSaveImageFromBase64(base64String, formattedFileName);
                                    Console.WriteLine($"Processamento de imagem '{formattedFileName}': Sucesso = {saveResult.success}, Mensagem = {saveResult.message}");
                                }
                                catch (Exception imageEx)
                                {
                                    Console.WriteLine($"Erro ao salvar anexo de imagem JPG '{formattedFileName}': {imageEx.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Tipo de anexo desconhecido '{attachmentType}' encontrado para o arquivo '{originalFileName}'. Ignorando.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Anexo encontrado sem todas as propriedades esperadas (filepath, filename, type). Ignorando.");
                        }
                    }
                    return ret; // Retorna o resultado inicial da SP
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

        public dynamic AppDataExport(DataImport dataimport)
        {
            using (var con = new SqlConnection(_connectionStrings.Default.ToString()))
            {
                try
                {
                    con.Open();
                    var query = "SP_DataExport";

                    List<string> jsonFragments = con.Query<string>(query, dataimport, commandType: CommandType.StoredProcedure).ToList();

                    // PASSO 2: Unir todos os fragmentos de string em uma �nica string JSON completa.
                    string fullJsonResult = string.Join("", jsonFragments);

                    // PASSO 3: Desserializar a string JSON completa em um objeto din�mico (ou um modelo C# forte).
                    // Como o SP retorna um array JSON ([...]), 'dynamic' � adequado.
                    dynamic ret = JsonConvert.DeserializeObject<dynamic>(fullJsonResult);

                    return ret; // Retorna o objeto JSON completo e desserializado.

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

        // M�DULO AUXILIAR: Para encontrar todos os anexos (�udio, imagem, etc.), independente da estrutura JSON
        private IEnumerable<JObject> FindAllAttachments(JToken token)
        {
            if (token == null) yield break;

            if (token is JArray topLevelArray)
            {
                foreach (var itemInArray in topLevelArray)
                {
                    if (itemInArray is JObject itemObject)
                    {
                        if (itemObject.TryGetValue("answers", StringComparison.OrdinalIgnoreCase, out var answersToken) && answersToken is JObject answersObject)
                        {
                            foreach (var answerProperty in answersObject.Properties())
                            {
                                if (answerProperty.Value is JObject answerObject)
                                {
                                    if (answerObject.TryGetValue("timeline", StringComparison.OrdinalIgnoreCase, out var timelineToken) && timelineToken is JArray timelineArray)
                                    {
                                        foreach (var timelineEntry in timelineArray)
                                        {
                                            if (timelineEntry is JObject attachmentCandidate &&
                                                attachmentCandidate.TryGetValue("type", StringComparison.OrdinalIgnoreCase, out var typeVal) &&
                                                typeVal.ToString().Equals("attachment", StringComparison.OrdinalIgnoreCase) &&
                                                attachmentCandidate.TryGetValue("attachment", StringComparison.OrdinalIgnoreCase, out var attachmentDetailsToken) &&
                                                attachmentDetailsToken is JObject attachmentDetailsObject)
                                            {
                                                yield return attachmentDetailsObject;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (token is JObject topLevelObject)
            {
                foreach (var property in topLevelObject.Properties())
                {
                    if (property.Value is JObject itemObject)
                    {
                        if (itemObject.TryGetValue("timeline", StringComparison.OrdinalIgnoreCase, out var timelineToken) && timelineToken is JArray timelineArray)
                        {
                            foreach (var timelineEntry in timelineArray)
                            {
                                if (timelineEntry is JObject attachmentCandidate &&
                                    attachmentCandidate.TryGetValue("type", StringComparison.OrdinalIgnoreCase, out var typeVal) &&
                                    typeVal.ToString().Equals("attachment", StringComparison.OrdinalIgnoreCase) &&
                                    attachmentCandidate.TryGetValue("attachment", StringComparison.OrdinalIgnoreCase, out var attachmentDetailsToken) &&
                                    attachmentDetailsToken is JObject attachmentDetailsObject)
                                {
                                    yield return attachmentDetailsObject;
                                }
                            }
                        }
                    }
                }
            }
        }


        public dynamic AppSaveAudioWebmFromBase64(string audioWebmBase64String, string fileName)
        {
            if (string.IsNullOrWhiteSpace(audioWebmBase64String))
            {
                return new { success = false, message = "A string Base64 do �udio WebM est� vazia ou nula. Por favor, forne�a um conte�do de �udio v�lido." };
            }

            string base64Data = audioWebmBase64String;
            const string prefix = "data:audio/webm;base64,";
            if (base64Data.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                base64Data = base64Data.Substring(prefix.Length);
            }
            try
            {
                byte[] audioBytes = Convert.FromBase64String(base64Data);

                var uploadsPath = Path.Combine(_env.ContentRootPath, "upload", "AUDIO");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                string fullLocalPath = Path.Combine(uploadsPath, fileName);
                File.WriteAllBytesAsync(fullLocalPath, audioBytes);

                string mimeType = "audio/webm";
                string storageUrl = StorageHelpers.upload(base64Data, fileName, _storageConfig.Value, _storageConfig.Value.s360audio).Result;

                return new { success = true, message = "�udio WebM salvo com sucesso.", localPath = fullLocalPath, storageUrl = storageUrl };
            }
            catch (FormatException ex)
            {
                return new { success = false, message = "Erro: A string fornecida n�o � um Base64 v�lido. Por favor, verifique o formato da string.", details = ex.Message };
            }
            catch (Exception ex)
            {
                return new { success = false, message = $"Ocorreu um erro inesperado ao processar e salvar o �udio WebM: {ex.Message}", details = ex.ToString() };
            }
        }


        public dynamic AppSaveImageFromBase64(string imageBase64String, string fileName)
        {
            if (string.IsNullOrWhiteSpace(imageBase64String))
            {
                return new { success = false, message = "A string Base64 da imagem est� vazia ou nula. Por favor, forne�a um conte�do de imagem v�lido." };
            }

            string base64Data = imageBase64String;
            int commaIndex = base64Data.IndexOf(',');
            if (commaIndex > -1)
            {
                base64Data = base64Data.Substring(commaIndex + 1);
            }

            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64Data);

                var uploadsPath = Path.Combine(_env.ContentRootPath, "upload", "IMAGE");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                string fullLocalPath = Path.Combine(uploadsPath, fileName);
                File.WriteAllBytesAsync(fullLocalPath, imageBytes);

                string storageUrl = StorageHelpers.upload(base64Data, fileName, _storageConfig.Value, _storageConfig.Value.s360images).Result;

                return new { success = true, message = "Imagem salva com sucesso.", localPath = fullLocalPath, storageUrl = storageUrl };
            }
            catch (FormatException ex)
            {
                return new { success = false, message = "Erro: A string fornecida n�o � um Base64 v�lido para imagem. Por favor, verifique o formato da string.", details = ex.Message };
            }
            catch (Exception ex)
            {
                return new { success = false, message = $"Ocorreu um erro inesperado ao processar e salvar a imagem: {ex.Message}", details = ex.ToString() };
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
                                Mail.Send(Convert.ToString(item.Email), Convert.ToString(item.CustomerName), "", "", "Recupera��o de Senha", html.Replace("LINK", confEmail.UrlRecuperarSenha + "?guid=" + Convert.ToString(item.DeviceToken)), confEmail);
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
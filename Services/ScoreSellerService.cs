using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApi.Helpers;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class ScoreSellerService : IScoreSellerService
    {
        private readonly ConnectionStrings _connectionStrings;

        public ScoreSellerService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public async Task<int> ProcessarAsync(string sellerCode, int contractId)
        {
            using var con = new SqlConnection(_connectionStrings.Default);
            await con.OpenAsync();

            // Executa SP_Score_Seller
            var scoreRow = await con.QueryFirstOrDefaultAsync<dynamic>(
                "SP_Score_Seller",
                new { SellerCode = sellerCode, ContractId = contractId },
                commandType: CommandType.StoredProcedure
            );

            if (scoreRow == null)
                throw new InvalidOperationException($"SP_Score_Seller não retornou dados para SellerCode={sellerCode}, ContractId={contractId}.");

            var dict = (IDictionary<string, object>)scoreRow;
            var jsonScoreCalculado = dict.ContainsKey("JsonScoreCalculado")
                ? dict["JsonScoreCalculado"]?.ToString()
                : null;

            if (string.IsNullOrWhiteSpace(jsonScoreCalculado))
                throw new InvalidOperationException("JsonScoreCalculado está vazio ou nulo.");

            JObject score;
            try { score = JObject.Parse(jsonScoreCalculado); }
            catch (Exception ex) { throw new InvalidOperationException($"JsonScoreCalculado inválido: {ex.Message}"); }

            // Busca usuário pelo código do vendedor
            var usuario = await con.QueryFirstOrDefaultAsync<UsuarioRow>(
                @"SELECT TOP 1 u.UserId, u.Email, u.Name, u.DddCell, u.NrCell
                  FROM DeptUser du
                  INNER JOIN [User] u ON du.UserId = u.UserId
                  WHERE du.CompanyCodeUser = @SellerCode AND u.ContractId = @ContractId",
                new { SellerCode = sellerCode, ContractId = contractId }
            );

            if (usuario == null)
                throw new InvalidOperationException($"Usuário não encontrado para SellerCode={sellerCode}, ContractId={contractId}.");

            // Busca template do contrato
            var jsonParam = await con.QueryFirstOrDefaultAsync<string>(
                "SELECT TOP 1 JsonParam FROM [Contract] WHERE ContractId = @ContractId",
                new { ContractId = contractId }
            );

            string emailSubject = "Seu Score de Vendas";
            string emailFrom = null;
            string emailNameFrom = null;

            if (!string.IsNullOrWhiteSpace(jsonParam))
            {
                try
                {
                    var contrato = JObject.Parse(jsonParam);
                    emailSubject = contrato["MsgEmailTemplate"]?["Subject"]?.ToString() ?? emailSubject;
                    emailFrom = contrato["MsgEmailTemplate"]?["From"]?.ToString();
                    emailNameFrom = contrato["MsgEmailTemplate"]?["NameFrom"]?.ToString();
                }
                catch { /* usa defaults */ }
            }

            var htmlBody = BuildEmailHtml(score, usuario.Name, emailSubject);
            var whatsAppBody = BuildWhatsAppText(score, usuario.Name);

            var phone = BuildPhone(usuario.DddCell, usuario.NrCell);

            var emailJson = BuildJsonData(usuario.Email, usuario.Name, phone, emailSubject, htmlBody, emailFrom, emailNameFrom);
            var whatsAppJson = BuildJsonData(usuario.Email, usuario.Name, phone, emailSubject, whatsAppBody, emailFrom, emailNameFrom);

            int inserted = 0;
            inserted += await con.ExecuteAsync(
                @"INSERT INTO MsgToSend (Type, MsgRefType, JsonData, Status, DtCreated, SellerCode, DtToSend)
                  VALUES (@Type, @MsgRefType, @JsonData, 'PENDING', GETDATE(), @SellerCode, GETDATE())",
                new { Type = "EMAIL", MsgRefType = "SCORE_SELLER", JsonData = emailJson, SellerCode = sellerCode }
            );
            inserted += await con.ExecuteAsync(
                @"INSERT INTO MsgToSend (Type, MsgRefType, JsonData, Status, DtCreated, SellerCode, DtToSend)
                  VALUES (@Type, @MsgRefType, @JsonData, 'PENDING', GETDATE(), @SellerCode, GETDATE())",
                new { Type = "WHATSAPP", MsgRefType = "SCORE_SELLER", JsonData = whatsAppJson, SellerCode = sellerCode }
            );

            return inserted;
        }

        private static string BuildEmailHtml(JObject score, string nomeVendedor, string titulo)
        {
            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset='utf-8'>
<style>
  body { font-family: Arial, sans-serif; background: #f4f4f4; margin: 0; padding: 20px; }
  .container { max-width: 700px; margin: 0 auto; background: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,.12); }
  .header { background: #1a3a5c; color: #fff; padding: 28px 32px; }
  .header h1 { margin: 0; font-size: 22px; }
  .header p { margin: 6px 0 0; font-size: 14px; opacity: .85; }
  .section { padding: 24px 32px; border-bottom: 1px solid #eee; }
  .section h2 { margin: 0 0 16px; font-size: 16px; color: #1a3a5c; text-transform: uppercase; letter-spacing: .5px; }
  .cards { display: flex; flex-wrap: wrap; gap: 12px; }
  .card { background: #f0f4f8; border-left: 4px solid #1a3a5c; border-radius: 4px; padding: 12px 16px; min-width: 140px; flex: 1; }
  .card .label { font-size: 11px; color: #666; text-transform: uppercase; margin-bottom: 4px; }
  .card .value { font-size: 20px; font-weight: bold; color: #1a3a5c; }
  table { width: 100%; border-collapse: collapse; font-size: 13px; }
  th { background: #1a3a5c; color: #fff; padding: 10px 12px; text-align: left; font-weight: 600; }
  td { padding: 9px 12px; border-bottom: 1px solid #eee; color: #333; }
  tr:last-child td { border-bottom: none; }
  tr:nth-child(even) td { background: #f9f9f9; }
  .footer { padding: 18px 32px; text-align: center; font-size: 12px; color: #999; }
</style></head><body><div class='container'>");

            sb.Append($"<div class='header'><h1>{EscapeHtml(titulo)}</h1><p>Olá, {EscapeHtml(nomeVendedor)}</p></div>");

            // Dados
            var dados = score["Dados"] as JObject;
            if (dados != null)
            {
                sb.Append("<div class='section'><h2>Dados do Vendedor</h2><div class='cards'>");
                foreach (var prop in dados.Properties())
                {
                    var arr = prop.Value as JArray;
                    var item = arr?.FirstOrDefault() as JObject;
                    if (item == null) continue;
                    var label = item["Label"]?.ToString() ?? prop.Name;
                    var valor = item["Valor"]?.ToString() ?? "";
                    sb.Append($"<div class='card'><div class='label'>{EscapeHtml(label)}</div><div class='value'>{EscapeHtml(valor)}</div></div>");
                }
                sb.Append("</div></div>");
            }

            // Estatísticas
            var estatisticas = score["Estatisticas"] as JObject;
            if (estatisticas != null)
            {
                sb.Append("<div class='section'><h2>Estatísticas</h2><div class='cards'>");
                foreach (var prop in estatisticas.Properties())
                {
                    var arr = prop.Value as JArray;
                    var item = arr?.FirstOrDefault() as JObject;
                    if (item == null) continue;
                    var label = item["Label"]?.ToString() ?? prop.Name;
                    var valor = item["Valor"]?.ToString() ?? "";
                    sb.Append($"<div class='card'><div class='label'>{EscapeHtml(label)}</div><div class='value'>{EscapeHtml(valor)}</div></div>");
                }
                sb.Append("</div></div>");
            }

            // Perguntas
            var perguntas = score["Perguntas"] as JArray;
            if (perguntas != null && perguntas.Count > 0)
            {
                sb.Append("<div class='section'><h2>Perguntas</h2><table><thead><tr>");

                var primeiraLinha = perguntas[0] as JObject;
                var colunas = new List<string>();
                if (primeiraLinha != null)
                {
                    foreach (var prop in primeiraLinha.Properties())
                    {
                        colunas.Add(prop.Name);
                        // Chave "Pergunta" usa "Pergunta" como cabeçalho; demais usam o Label
                        string header;
                        if (prop.Name == "Pergunta")
                            header = "Pergunta";
                        else
                        {
                            var item = prop.Value as JObject;
                            header = item?["Label"]?.ToString() ?? prop.Name;
                        }
                        sb.Append($"<th>{EscapeHtml(header)}</th>");
                    }
                }
                sb.Append("</tr></thead><tbody>");

                foreach (var row in perguntas)
                {
                    var r = row as JObject;
                    if (r == null) continue;
                    sb.Append("<tr>");
                    foreach (var col in colunas)
                    {
                        var cell = r[col];
                        string valor;

                        var item = cell as JObject;
                        valor = item?["Valor"]?.ToString() ?? "";

                        sb.Append($"<td>{EscapeHtml(valor)}</td>");
                    }
                    sb.Append("</tr>");
                }

                sb.Append("</tbody></table></div>");
            }

            sb.Append("<div class='footer'>Esta mensagem foi gerada automaticamente pelo sistema S360.</div>");
            sb.Append("</div></body></html>");
            return sb.ToString();
        }

        private static string BuildWhatsAppText(JObject score, string nomeVendedor)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"*Score de Vendas - {nomeVendedor}*");
            sb.AppendLine();

            var dados = score["Dados"] as JObject;
            if (dados != null)
            {
                sb.AppendLine("*Dados do Vendedor*");
                foreach (var prop in dados.Properties())
                {
                    var arr = prop.Value as JArray;
                    var item = arr?.FirstOrDefault() as JObject;
                    if (item == null) continue;
                    var label = item["Label"]?.ToString() ?? prop.Name;
                    var valor = item["Valor"]?.ToString() ?? "";
                    sb.AppendLine($"{label}: {valor}");
                }
                sb.AppendLine();
            }

            var estatisticas = score["Estatisticas"] as JObject;
            if (estatisticas != null)
            {
                sb.AppendLine("*Estatísticas*");
                foreach (var prop in estatisticas.Properties())
                {
                    var arr = prop.Value as JArray;
                    var item = arr?.FirstOrDefault() as JObject;
                    if (item == null) continue;
                    var label = item["Label"]?.ToString() ?? prop.Name;
                    var valor = item["Valor"]?.ToString() ?? "";
                    sb.AppendLine($"{label}: {valor}");
                }
                sb.AppendLine();
            }

            var perguntas = score["Perguntas"] as JArray;
            if (perguntas != null && perguntas.Count > 0)
            {
                sb.AppendLine("*Perguntas*");
                foreach (var row in perguntas)
                {
                    var r = row as JObject;
                    if (r == null) continue;


                    foreach (var prop in r.Properties())
                    {
                        var item = prop.Value as JObject;
                        var label = item?["Label"]?.ToString() ?? prop.Name;
                        if (prop.Name == "Pergunta") { 
                            label = $"▪ *Pergunta*";
                            var valora = item?["Valor"]?.ToString() ?? "";
                            sb.AppendLine($"{label}: {valora}");
                            continue;
                        }
                        var valor = item?["Valor"]?.ToString() ?? "";
                        sb.AppendLine($"  {label}: {valor}");
                    }
                }
            }

            return sb.ToString().TrimEnd();
        }

        private static string BuildPhone(string dddCell, string nrCell)
        {
            var digits = new string(
                ((dddCell ?? "") + (nrCell ?? "")).Where(char.IsDigit).ToArray()
            );
            if (digits.Length == 0) return "";
            if (digits.StartsWith("55") && digits.Length >= 12 && digits.Length <= 13) return digits;
            if (digits.Length >= 10 && digits.Length <= 11) return "55" + digits;
            return digits;
        }

        private static string BuildJsonData(string to, string toName, string phone, string subject, string body, string from, string nameFrom)
        {
            var obj = new
            {
                To = to ?? "",
                ToName = toName ?? "",
                Phone = phone ?? "",
                Subject = subject ?? "",
                Body = body ?? "",
                From = from ?? "",
                NameFrom = nameFrom ?? ""
            };
            return JsonConvert.SerializeObject(obj);
        }

        private static string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }

        private class UsuarioRow
        {
            public int UserId { get; set; }
            public string Email { get; set; }
            public string Name { get; set; }
            public string DddCell { get; set; }
            public string NrCell { get; set; }
        }
    }
}

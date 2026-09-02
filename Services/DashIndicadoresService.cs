using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class DashIndicadoresService : IDashIndicadoresService
    {
        private readonly ConnectionStrings _connectionStrings;

        public DashIndicadoresService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public List<DashIndicadoresRow> Select(int userId, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var raw = con.Query("SP_dash_SubmittedAnswerDetailsByUser", new
                    {
                        UserId        = userId,
                        token_usuario = tokenUsuario
                    }, commandType: CommandType.StoredProcedure);

                    return raw.Select(r =>
                    {
                        var d = (IDictionary<string, object>)r;
                        return new DashIndicadoresRow
                        {
                            SubmittedAnswerDetailId = Convert.ToInt32(d["SubmittedAnswerDetailId"] ?? 0),
                            IdQuestionario          = Convert.ToInt32(d["IdQuestionario"] ?? 0),
                            TipoQuestionario        = d.ContainsKey("Tipo Questionário") ? d["Tipo Questionário"]?.ToString() : d.ContainsKey("TipoQuestionario") ? d["TipoQuestionario"]?.ToString() : null,
                            Data                    = d["Data"]?.ToString(),
                            Supervisor              = d["Supervisor"]?.ToString(),
                            IdUsuario               = Convert.ToInt32(d["IdUsuario"] ?? 0),
                            CodVendedor             = d.ContainsKey("Cod Vendedor") ? d["Cod Vendedor"]?.ToString() : d.ContainsKey("CodVendedor") ? d["CodVendedor"]?.ToString() : null,
                            IdVendedor              = Convert.ToInt32(d["IdVendedor"] ?? 0),
                            Vendedor                = d["Vendedor"]?.ToString(),
                            CodCliente              = d.ContainsKey("Cod Cliente") ? d["Cod Cliente"]?.ToString() : d.ContainsKey("CodCliente") ? d["CodCliente"]?.ToString() : null,
                            Questao                 = d.ContainsKey("Questão") ? d["Questão"]?.ToString() : d.ContainsKey("Questao") ? d["Questao"]?.ToString() : null,
                            Rank                    = d["Rank"] == null ? (int?)null : Convert.ToInt32(d["Rank"]),
                            MetricaPadrao           = d.ContainsKey("Metrica padrão") ? Convert.ToBoolean(d["Metrica padrão"] ?? false) : d.ContainsKey("MetricaPadrao") ? Convert.ToBoolean(d["MetricaPadrao"] ?? false) : false,
                            TipoSN                  = d.ContainsKey("Tipo S/N") ? Convert.ToBoolean(d["Tipo S/N"] ?? false) : d.ContainsKey("TipoSN") ? Convert.ToBoolean(d["TipoSN"] ?? false) : false,
                            Resposta                = d["Resposta"]?.ToString(),
                            Sim                     = d.ContainsKey("SIM") ? Convert.ToInt32(d["SIM"] ?? 0) : d.ContainsKey("Sim") ? Convert.ToInt32(d["Sim"] ?? 0) : 0,
                            Nao                     = d.ContainsKey("NÃO") ? Convert.ToInt32(d["NÃO"] ?? 0) : d.ContainsKey("Nao") ? Convert.ToInt32(d["Nao"] ?? 0) : 0,
                            NaoRespondido           = d.ContainsKey("N/R") ? Convert.ToInt32(d["N/R"] ?? 0) : d.ContainsKey("NaoRespondido") ? Convert.ToInt32(d["NaoRespondido"] ?? 0) : 0,
                            Justificado             = d.ContainsKey("JUSTIFICADO") ? Convert.ToInt32(d["JUSTIFICADO"] ?? 0) : d.ContainsKey("Justificado") ? Convert.ToInt32(d["Justificado"] ?? 0) : 0,
                            Grupo                   = d["Grupo"]?.ToString(),
                            Metrica                 = d.ContainsKey("Métrica") ? d["Métrica"]?.ToString() : d.ContainsKey("Metrica") ? d["Metrica"]?.ToString() : null,
                            Peso                    = d.ContainsKey("PESO") ? Convert.ToDecimal(d["PESO"] ?? 0) : d.ContainsKey("Peso") ? Convert.ToDecimal(d["Peso"] ?? 0) : 0,
                            NivelCompetencia        = d.ContainsKey("Nível de competencia") ? Convert.ToBoolean(d["Nível de competencia"] ?? false) : d.ContainsKey("NivelCompetencia") ? Convert.ToBoolean(d["NivelCompetencia"] ?? false) : false,
                            TerminoAntecipado       = d.ContainsKey("Término antecipado") ? Convert.ToBoolean(d["Término antecipado"] ?? false) : d.ContainsKey("TerminoAntecipado") ? Convert.ToBoolean(d["TerminoAntecipado"] ?? false) : false,
                            Feedback                = d.ContainsKey("Feedback") ? Convert.ToBoolean(d["Feedback"] ?? false) : false,
                            TipoCompetencia     = d.ContainsKey("Tipo de Competencia") ? d["Tipo de Competencia"]?.ToString() : d.ContainsKey("TipoCompetencia") ? d["TipoCompetencia"]?.ToString() : null,
                        };
                    }).ToList();
                }
                catch (Exception ex) { throw ex; }
                finally { con.Close(); }
            }
        }
    }
}

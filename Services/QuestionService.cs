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
    public class QuestionService : IQuestionService
    {
        private readonly ConnectionStrings _connectionStrings;

        public QuestionService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public dynamic Select(QuestionFilterModel filtro)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_sl_Question", new
                    {
                        filtro.QuestionId,
                        filtro.IsComplement,
                        filtro.Rank,
                        filtro.Question,
                        filtro.AnswerTypeCd,
                        filtro.Group,
                        filtro.Metric,
                        filtro.IsFirstSurvey,
                        filtro.IsFinalSurvey,
                        filtro.IsCompetenceLevel,
                        filtro.IsFinishEarly,
                        filtro.IsStandardMetric,
                        filtro.IsSglYesNoType,
                        filtro.IsFeedback
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

        public dynamic Create(QuestionCreateModel model)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_cr_Question", new
                    {
                        model.IsComplement,
                        model.Rank,
                        model.Question,
                        model.Description,
                        model.AnswerTypeCd,
                        model.Group,
                        model.Metric,
                        model.IconMetric,
                        model.IsFirstSurvey,
                        model.IsFinalSurvey,
                        model.IsCompetenceLevel,
                        model.IsFinishEarly,
                        model.IsStandardMetric,
                        model.IsSglYesNoType,
                        model.IsFeedback
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

        public dynamic Update(QuestionUpdateModel model)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_up_Question", new
                    {
                        model.QuestionId,
                        model.IsComplement,
                        model.Rank,
                        model.Question,
                        model.Description,
                        model.AnswerTypeCd,
                        model.Group,
                        model.Metric,
                        model.IconMetric,
                        model.IsFirstSurvey,
                        model.IsFinalSurvey,
                        model.IsCompetenceLevel,
                        model.IsFinishEarly,
                        model.IsStandardMetric,
                        model.IsSglYesNoType,
                        model.IsFeedback
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

        public dynamic Delete(int questionId)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_dl_Question", new
                    {
                        QuestionId = questionId
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

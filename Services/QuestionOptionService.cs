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
    public class QuestionOptionService : IQuestionOptionService
    {
        private readonly ConnectionStrings _connectionStrings;

        public QuestionOptionService(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public dynamic Select(QuestionOptionFilterModel filtro, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_sl_QuestionOption", new
                    {
                        filtro.QuestionOptionId,
                        filtro.QuestionId,
                        filtro.ComplementQuestionId,
                        filtro.Rank,
                        filtro.OptionCd,
                        filtro.Description,
                        filtro.OpenMsgBox,
                        filtro.NeedNotes,
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

        public dynamic Create(QuestionOptionCreateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_cr_QuestionOption", new
                    {
                        model.QuestionId,
                        model.ComplementQuestionId,
                        model.Rank,
                        model.OptionCd,
                        model.Description,
                        model.OpenMsgBox,
                        model.NeedNotes,
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

        public dynamic Update(QuestionOptionUpdateModel model, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_up_QuestionOption", new
                    {
                        model.QuestionOptionId,
                        model.QuestionId,
                        model.ComplementQuestionId,
                        model.Rank,
                        model.OptionCd,
                        model.Description,
                        model.OpenMsgBox,
                        model.NeedNotes,
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

        public dynamic Delete(int questionOptionId, string tokenUsuario)
        {
            using (var con = new SqlConnection(_connectionStrings.Default))
            {
                try
                {
                    con.Open();
                    var ret = con.Query("SP_dl_QuestionOption", new
                    {
                        QuestionOptionId = questionOptionId,
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

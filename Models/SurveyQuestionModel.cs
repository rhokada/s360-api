using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela SurveyQuestion.</summary>
    public class SurveyQuestionModel
    {
        public int      SurveyQuestionId    { get; set; }
        public int      SurveyId            { get; set; }
        public int      QuestionId          { get; set; }
        public DateTime? DhCreate           { get; set; }
        public DateTime? DhUpdate           { get; set; }
        public string   Log                 { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de SurveyQuestion.</summary>
    public class SurveyQuestionFilterModel
    {
        public int? SurveyQuestionId    { get; set; }
        public int? SurveyId            { get; set; }
        public int? QuestionId          { get; set; }
        public string Question          { get; set; }
    }

    /// <summary>Campos necessários para criação de um SurveyQuestion.</summary>
    public class SurveyQuestionCreateModel
    {
        public int SurveyId     { get; set; }
        public int QuestionId   { get; set; }
    }

    /// <summary>Campos necessários para atualização de um SurveyQuestion.</summary>
    public class SurveyQuestionUpdateModel
    {
        public int SurveyQuestionId { get; set; }
        public int SurveyId         { get; set; }
        public int QuestionId       { get; set; }
    }
}

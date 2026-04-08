using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela SurveySup.</summary>
    public class SurveySupModel
    {
        public int      SurveySupId     { get; set; }
        public int      SupUserId       { get; set; }
        public int      SurveyId        { get; set; }
        public string   Name            { get; set; }
        public DateTime? DhCreate       { get; set; }
        public DateTime? DhUpdate       { get; set; }
        public string   Log             { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de SurveySup.</summary>
    public class SurveySupFilterModel
    {
        public int?     SurveySupId     { get; set; }
        public int?     SupUserId       { get; set; }
        public int?     SurveyId        { get; set; }
        public string   Name            { get; set; }
    }

    /// <summary>Campos necessários para criação de um SurveySup.</summary>
    public class SurveySupCreateModel
    {
        public int      SupUserId   { get; set; }
        public int      SurveyId    { get; set; }
        public string   Name        { get; set; }
    }

    /// <summary>Campos necessários para atualização de um SurveySup.</summary>
    public class SurveySupUpdateModel
    {
        public int      SurveySupId { get; set; }
        public int      SupUserId   { get; set; }
        public int      SurveyId    { get; set; }
        public string   Name        { get; set; }
    }
}

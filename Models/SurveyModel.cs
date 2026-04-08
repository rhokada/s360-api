using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela Survey.</summary>
    public class SurveyModel
    {
        public int      SurveyId        { get; set; }
        public int      SurveyTypeId    { get; set; }
        public string   Name            { get; set; }
        public DateTime DtIni           { get; set; }
        public DateTime? DtFin          { get; set; }
        public DateTime? DhUpdate       { get; set; }
        public string   Log             { get; set; }
        public DateTime? DhCreate       { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de Survey.</summary>
    public class SurveyFilterModel
    {
        public int?     SurveyId        { get; set; }
        public int?     SurveyTypeId    { get; set; }
        public string   Name            { get; set; }
    }

    /// <summary>Campos necessários para criação de um Survey.</summary>
    public class SurveyCreateModel
    {
        public int      SurveyTypeId    { get; set; }
        public string   Name            { get; set; }
        public DateTime DtIni           { get; set; }
        public DateTime? DtFin          { get; set; }
    }

    /// <summary>Campos necessários para atualização de um Survey.</summary>
    public class SurveyUpdateModel
    {
        public int      SurveyId        { get; set; }
        public int      SurveyTypeId    { get; set; }
        public string   Name            { get; set; }
        public DateTime DtIni           { get; set; }
        public DateTime? DtFin          { get; set; }
    }
}

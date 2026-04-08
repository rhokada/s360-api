using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela SurveyType.</summary>
    public class SurveyTypeModel
    {
        public int      SurveyTypeId    { get; set; }
        public string   SurveyTypeCd    { get; set; }
        public string   Name            { get; set; }
        public DateTime? DhUpdate       { get; set; }
        public string   Log             { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de SurveyType.</summary>
    public class SurveyTypeFilterModel
    {
        public int?     SurveyTypeId    { get; set; }
        public string   SurveyTypeCd    { get; set; }
        public string   Name            { get; set; }
    }

    /// <summary>Campos necessários para criação de um SurveyType.</summary>
    public class SurveyTypeCreateModel
    {
        public string   SurveyTypeCd    { get; set; }
        public string   Name            { get; set; }
    }

    /// <summary>Campos necessários para atualização de um SurveyType.</summary>
    public class SurveyTypeUpdateModel
    {
        public int      SurveyTypeId    { get; set; }
        public string   SurveyTypeCd    { get; set; }
        public string   Name            { get; set; }
    }
}

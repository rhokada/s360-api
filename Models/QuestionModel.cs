using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela Question.</summary>
    public class QuestionModel
    {
        public int      QuestionId          { get; set; }
        public bool     IsComplement        { get; set; }
        public int      Rank                { get; set; }
        public string   Question            { get; set; }
        public string   Description         { get; set; }
        public string   AnswerTypeCd        { get; set; }
        public DateTime? DhCreate           { get; set; }
        public DateTime? DhUpdate           { get; set; }
        public string   Log                 { get; set; }
        public string   Group               { get; set; }
        public string   Metric              { get; set; }
        public string   IconMetric          { get; set; }
        public bool?    IsFirstSurvey       { get; set; }
        public bool?    IsFinalSurvey       { get; set; }
        public bool?    IsCompetenceLevel   { get; set; }
        public bool?    IsFinishEarly       { get; set; }
        public bool?    IsStandardMetric    { get; set; }
        public bool?    IsSglYesNoType      { get; set; }
        public bool?    IsFeedback          { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de Question.</summary>
    public class QuestionFilterModel
    {
        public int?     QuestionId          { get; set; }
        public bool?    IsComplement        { get; set; }
        public int?     Rank                { get; set; }
        public string   Question            { get; set; }
        public string   AnswerTypeCd        { get; set; }
        public string   Group               { get; set; }
        public string   Metric              { get; set; }
        public bool?    IsFirstSurvey       { get; set; }
        public bool?    IsFinalSurvey       { get; set; }
        public bool?    IsCompetenceLevel   { get; set; }
        public bool?    IsFinishEarly       { get; set; }
        public bool?    IsStandardMetric    { get; set; }
        public bool?    IsSglYesNoType      { get; set; }
        public bool?    IsFeedback          { get; set; }
    }

    /// <summary>Campos necessários para criação de uma Question.</summary>
    public class QuestionCreateModel
    {
        public bool     IsComplement        { get; set; }
        public int      Rank                { get; set; }
        public string   Question            { get; set; }
        public string   Description         { get; set; }
        public string   AnswerTypeCd        { get; set; }
        public string   Group               { get; set; }
        public string   Metric              { get; set; }
        public string   IconMetric          { get; set; }
        public bool?    IsFirstSurvey       { get; set; }
        public bool?    IsFinalSurvey       { get; set; }
        public bool?    IsCompetenceLevel   { get; set; }
        public bool?    IsFinishEarly       { get; set; }
        public bool?    IsStandardMetric    { get; set; }
        public bool?    IsSglYesNoType      { get; set; }
        public bool?    IsFeedback          { get; set; }
    }

    /// <summary>Campos necessários para atualização de uma Question.</summary>
    public class QuestionUpdateModel
    {
        public int      QuestionId          { get; set; }
        public bool     IsComplement        { get; set; }
        public int      Rank                { get; set; }
        public string   Question            { get; set; }
        public string   Description         { get; set; }
        public string   AnswerTypeCd        { get; set; }
        public string   Group               { get; set; }
        public string   Metric              { get; set; }
        public string   IconMetric          { get; set; }
        public bool?    IsFirstSurvey       { get; set; }
        public bool?    IsFinalSurvey       { get; set; }
        public bool?    IsCompetenceLevel   { get; set; }
        public bool?    IsFinishEarly       { get; set; }
        public bool?    IsStandardMetric    { get; set; }
        public bool?    IsSglYesNoType      { get; set; }
        public bool?    IsFeedback          { get; set; }
    }
}

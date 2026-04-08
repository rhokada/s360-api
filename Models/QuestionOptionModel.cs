using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela QuestionOption.</summary>
    public class QuestionOptionModel
    {
        public int      QuestionOptionId        { get; set; }
        public int      QuestionId              { get; set; }
        public int?     ComplementQuestionId    { get; set; }
        public int      Rank                    { get; set; }
        public string   OptionCd                { get; set; }
        public string   Description             { get; set; }
        public DateTime? DhCreate               { get; set; }
        public DateTime? DhUpdate               { get; set; }
        public string   Log                     { get; set; }
        public bool?    OpenMsgBox              { get; set; }
        public bool?    NeedNotes               { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de QuestionOption.</summary>
    public class QuestionOptionFilterModel
    {
        public int?     QuestionOptionId        { get; set; }
        public int?     QuestionId              { get; set; }
        public int?     ComplementQuestionId    { get; set; }
        public int?     Rank                    { get; set; }
        public string   OptionCd                { get; set; }
        public string   Description             { get; set; }
        public bool?    OpenMsgBox              { get; set; }
        public bool?    NeedNotes               { get; set; }
    }

    /// <summary>Campos necessários para criação de uma QuestionOption.</summary>
    public class QuestionOptionCreateModel
    {
        public int      QuestionId              { get; set; }
        public int?     ComplementQuestionId    { get; set; }
        public int      Rank                    { get; set; }
        public string   OptionCd                { get; set; }
        public string   Description             { get; set; }
        public bool?    OpenMsgBox              { get; set; }
        public bool?    NeedNotes               { get; set; }
    }

    /// <summary>Campos necessários para atualização de uma QuestionOption.</summary>
    public class QuestionOptionUpdateModel
    {
        public int      QuestionOptionId        { get; set; }
        public int      QuestionId              { get; set; }
        public int?     ComplementQuestionId    { get; set; }
        public int      Rank                    { get; set; }
        public string   OptionCd                { get; set; }
        public string   Description             { get; set; }
        public bool?    OpenMsgBox              { get; set; }
        public bool?    NeedNotes               { get; set; }
    }
}

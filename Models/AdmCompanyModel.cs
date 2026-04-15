using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela Company com dados de JOIN.</summary>
    public class AdmCompanyModel
    {
        public int       CompanyId        { get; set; }
        public int?      AddressId        { get; set; }
        public int?      GroupCompanyId   { get; set; }
        public string    Name             { get; set; }
        public string    TaxID            { get; set; }
        public string    LogoUrl          { get; set; }
        public DateTime? DhCreate         { get; set; }
        public DateTime? DhUpdate         { get; set; }
        public string    Log              { get; set; }
        public int?      ParentCompanyId  { get; set; }
        /// <summary>Cidade do endereço (via LEFT JOIN Address).</summary>
        public string    City             { get; set; }
        /// <summary>Estado do endereço (via LEFT JOIN Address).</summary>
        public string    State            { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de Company.</summary>
    public class AdmCompanyFilterModel
    {
        public int?    CompanyId        { get; set; }
        /// <summary>Filtro por Name com LIKE.</summary>
        public string  Name             { get; set; }
        public string  TaxID            { get; set; }
        public int?    GroupCompanyId   { get; set; }
        public int?    ParentCompanyId  { get; set; }
    }

    /// <summary>Campos necessários para criação de uma Company.</summary>
    public class AdmCompanyCreateModel
    {
        public int?    AddressId        { get; set; }
        public int?    GroupCompanyId   { get; set; }
        public string  Name             { get; set; }
        public string  TaxID            { get; set; }
        public string  LogoUrl          { get; set; }
        public int?    ParentCompanyId  { get; set; }
    }

    /// <summary>Campos necessários para atualização de uma Company.</summary>
    public class AdmCompanyUpdateModel
    {
        public int     CompanyId        { get; set; }
        public int?    AddressId        { get; set; }
        public int?    GroupCompanyId   { get; set; }
        public string  Name             { get; set; }
        public string  TaxID            { get; set; }
        public string  LogoUrl          { get; set; }
        public int?    ParentCompanyId  { get; set; }
    }
}

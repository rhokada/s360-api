using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela CompanyDept com dados de JOIN.</summary>
    public class AdmCompanyDeptModel
    {
        public int       CompanyDeptId   { get; set; }
        public int       CompanyId       { get; set; }
        public string    CompanyName     { get; set; }
        public int?      AddressId       { get; set; }
        public string    Name            { get; set; }
        public string    ProfitCenter    { get; set; }
        public string    CostCenter      { get; set; }
        public DateTime? DhUpdate        { get; set; }
        public string    Log             { get; set; }
        /// <summary>Cidade do endereço (via LEFT JOIN Address).</summary>
        public string    City            { get; set; }
        /// <summary>Estado do endereço (via LEFT JOIN Address).</summary>
        public string    State           { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de CompanyDept.</summary>
    public class AdmCompanyDeptFilterModel
    {
        public int?    CompanyDeptId  { get; set; }
        public int?    CompanyId      { get; set; }
        /// <summary>Filtro por Name com LIKE.</summary>
        public string  Name           { get; set; }
    }

    /// <summary>Campos necessários para criação de um CompanyDept.</summary>
    public class AdmCompanyDeptCreateModel
    {
        public int     CompanyId     { get; set; }
        public int?    AddressId     { get; set; }
        public string  Name          { get; set; }
        public string  ProfitCenter  { get; set; }
        public string  CostCenter    { get; set; }
    }

    /// <summary>Campos necessários para atualização de um CompanyDept.</summary>
    public class AdmCompanyDeptUpdateModel
    {
        public int     CompanyDeptId  { get; set; }
        public int     CompanyId      { get; set; }
        public int?    AddressId      { get; set; }
        public string  Name           { get; set; }
        public string  ProfitCenter   { get; set; }
        public string  CostCenter     { get; set; }
    }
}

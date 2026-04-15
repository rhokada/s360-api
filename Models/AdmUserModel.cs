using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela User (sem campos de acesso).</summary>
    public class AdmUserModel
    {
        public int       UserId           { get; set; }
        public string    AppId            { get; set; }
        public string    Name             { get; set; }
        public string    Email            { get; set; }
        public string    DddCell          { get; set; }
        public string    NrCell           { get; set; }
        public bool?     Active           { get; set; }
        public DateTime? DhCreate         { get; set; }
        public DateTime? DhUpdate         { get; set; }
        public int?      ContractId       { get; set; }
        public string    PbiLogin         { get; set; }
        public string    Log              { get; set; }
        /// <summary>Título do DeptUser principal (via OUTER APPLY TOP 1).</summary>
        public string    Title            { get; set; }
        /// <summary>Código do funcionário no DeptUser principal.</summary>
        public string    CompanyCodeUser  { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de User.</summary>
    public class AdmUserFilterModel
    {
        public int?    UserId      { get; set; }
        public string  AppId       { get; set; }
        public string  Name        { get; set; }
        public string  Email       { get; set; }
        public bool?   Active      { get; set; }
        public int?    ContractId  { get; set; }
    }

    /// <summary>Campos necessários para criação de um User.</summary>
    public class AdmUserCreateModel
    {
        public string  AppId          { get; set; }
        public string  Name           { get; set; }
        public string  Email          { get; set; }
        public string  DddCell        { get; set; }
        public string  NrCell         { get; set; }
        public bool?   Active         { get; set; }
        public int?    ContractId     { get; set; }
        public string  PbiLogin       { get; set; }
    }

    /// <summary>Campos necessários para atualização de um User.</summary>
    public class AdmUserUpdateModel
    {
        public int     UserId         { get; set; }
        public string  AppId          { get; set; }
        public string  Name           { get; set; }
        public string  Email          { get; set; }
        public string  DddCell        { get; set; }
        public string  NrCell         { get; set; }
        public bool?   Active         { get; set; }
        public int?    ContractId     { get; set; }
        public string  PbiLogin       { get; set; }
    }
}

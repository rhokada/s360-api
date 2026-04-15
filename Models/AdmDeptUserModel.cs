using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela DeptUser com dados de JOIN.</summary>
    public class AdmDeptUserModel
    {
        public int       DeptUserId       { get; set; }
        public int       UserId           { get; set; }
        public string    UserName         { get; set; }
        public string    UserEmail        { get; set; }
        public int       CompanyDeptId    { get; set; }
        public string    DeptName         { get; set; }
        public string    CompanyName      { get; set; }
        public string    Title            { get; set; }
        public string    CompanyCodeUser  { get; set; }
        public DateTime? DhUpdate         { get; set; }
        public string    Log              { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de DeptUser.</summary>
    public class AdmDeptUserFilterModel
    {
        public int?    DeptUserId       { get; set; }
        public int?    UserId           { get; set; }
        public int?    CompanyDeptId    { get; set; }
        /// <summary>Filtro por Title com LIKE.</summary>
        public string  Title            { get; set; }
        public string  CompanyCodeUser  { get; set; }
    }

    /// <summary>Campos necessários para criação de um DeptUser.</summary>
    public class AdmDeptUserCreateModel
    {
        public int     UserId           { get; set; }
        public int     CompanyDeptId    { get; set; }
        public string  Title            { get; set; }
        public string  CompanyCodeUser  { get; set; }
    }

    /// <summary>Campos necessários para atualização de um DeptUser.</summary>
    public class AdmDeptUserUpdateModel
    {
        public int     DeptUserId       { get; set; }
        public int     UserId           { get; set; }
        public int     CompanyDeptId    { get; set; }
        public string  Title            { get; set; }
        public string  CompanyCodeUser  { get; set; }
    }
}

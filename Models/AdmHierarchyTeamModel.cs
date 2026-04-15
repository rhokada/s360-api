using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela HierarchyTeam com dados de JOIN.</summary>
    public class AdmHierarchyTeamModel
    {
        public int       HierarchyTeamId   { get; set; }
        public int       HierarchyId       { get; set; }
        public string    HierarchyName     { get; set; }
        public int       UserId            { get; set; }
        public string    UserName          { get; set; }
        public string    UserEmail         { get; set; }
        public int       BossId            { get; set; }
        public string    BossName          { get; set; }
        public string    BossEmail         { get; set; }
        public bool?     Active            { get; set; }
        public DateTime? DhCreate          { get; set; }
        public DateTime? DhUpdate          { get; set; }
        /// <summary>DeptUserId do funcionário (via OUTER APPLY TOP 1).</summary>
        public int?      DeptUserId        { get; set; }
        public int?      CompanyDeptId     { get; set; }
        public string    Title             { get; set; }
        public string    CompanyCodeUser   { get; set; }
        public string    DeptName          { get; set; }
        public int?      CompanyId         { get; set; }
        public string    CompanyName       { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de HierarchyTeam.</summary>
    public class AdmHierarchyTeamFilterModel
    {
        public int?   HierarchyTeamId  { get; set; }
        public int?   HierarchyId      { get; set; }
        public int?   UserId           { get; set; }
        public int?   BossId           { get; set; }
        public bool?  Active           { get; set; }
    }

    /// <summary>Campos necessários para criação de um HierarchyTeam.</summary>
    public class AdmHierarchyTeamCreateModel
    {
        public int    HierarchyId  { get; set; }
        public int    UserId       { get; set; }
        public int    BossId       { get; set; }
        public bool?  Active       { get; set; }
    }

    /// <summary>Campos necessários para atualização de um HierarchyTeam.</summary>
    public class AdmHierarchyTeamUpdateModel
    {
        public int    HierarchyTeamId  { get; set; }
        public int    HierarchyId      { get; set; }
        public int    UserId           { get; set; }
        public int    BossId           { get; set; }
        public bool?  Active           { get; set; }
    }
}

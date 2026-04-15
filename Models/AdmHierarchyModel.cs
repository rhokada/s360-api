using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela Hierarchy.</summary>
    public class AdmHierarchyModel
    {
        public int       HierarchyId  { get; set; }
        public string    Name         { get; set; }
        public DateTime? DhCreate     { get; set; }
        public DateTime? DhUpdate     { get; set; }
        public string    Log          { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de Hierarchy.</summary>
    public class AdmHierarchyFilterModel
    {
        public int?    HierarchyId  { get; set; }
        public string  Name         { get; set; }
    }

    /// <summary>Campos necessários para criação de uma Hierarchy.</summary>
    public class AdmHierarchyCreateModel
    {
        public string  Name  { get; set; }
    }

    /// <summary>Campos necessários para atualização de uma Hierarchy.</summary>
    public class AdmHierarchyUpdateModel
    {
        public int     HierarchyId  { get; set; }
        public string  Name         { get; set; }
    }
}

using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela Address.</summary>
    public class AdmAddressModel
    {
        public int       AddressId      { get; set; }
        public string    Street         { get; set; }
        public string    Street2        { get; set; }
        public string    Neighborhood   { get; set; }
        public string    City           { get; set; }
        public string    State          { get; set; }
        public string    ZipCode        { get; set; }
        public string    Country        { get; set; }
        public DateTime? DhUpdate       { get; set; }
        public string    Log            { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de Address.</summary>
    public class AdmAddressFilterModel
    {
        public int?    AddressId     { get; set; }
        /// <summary>Filtro por Street com LIKE.</summary>
        public string  Street        { get; set; }
        public string  Neighborhood  { get; set; }
        public string  City          { get; set; }
        public string  State         { get; set; }
        public string  ZipCode       { get; set; }
        public string  Country       { get; set; }
    }

    /// <summary>Campos necessários para criação de um Address.</summary>
    public class AdmAddressCreateModel
    {
        public string  Street        { get; set; }
        public string  Street2       { get; set; }
        public string  Neighborhood  { get; set; }
        public string  City          { get; set; }
        public string  State         { get; set; }
        public string  ZipCode       { get; set; }
        public string  Country       { get; set; }
    }

    /// <summary>Campos necessários para atualização de um Address.</summary>
    public class AdmAddressUpdateModel
    {
        public int     AddressId     { get; set; }
        public string  Street        { get; set; }
        public string  Street2       { get; set; }
        public string  Neighborhood  { get; set; }
        public string  City          { get; set; }
        public string  State         { get; set; }
        public string  ZipCode       { get; set; }
        public string  Country       { get; set; }
    }
}

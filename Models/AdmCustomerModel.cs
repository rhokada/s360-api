using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela Customer com dados de JOIN.</summary>
    public class AdmCustomerModel
    {
        public int       CustomerId    { get; set; }
        public int       CompanyId     { get; set; }
        public string    CompanyName   { get; set; }
        public bool?     ToBeConfirmed { get; set; }
        public string    CustomerCode  { get; set; }
        public string    Name          { get; set; }
        public string    CNPJ          { get; set; }
        public string    Street        { get; set; }
        public string    Street2       { get; set; }
        public string    Neighborhood  { get; set; }
        public string    City          { get; set; }
        public string    State         { get; set; }
        public string    ZipCode       { get; set; }
        public DateTime? DhCreate      { get; set; }
        public DateTime? DhUpdate      { get; set; }
        public string    Log           { get; set; }
        public Guid?     GuId          { get; set; }
        public string    OriginCd      { get; set; }
        public int?      DataImportId  { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de Customer.</summary>
    public class AdmCustomerFilterModel
    {
        public int?    CustomerId    { get; set; }
        public int?    CompanyId     { get; set; }
        public string  Name          { get; set; }
        public string  CustomerCode  { get; set; }
        public string  CNPJ          { get; set; }
        public string  City          { get; set; }
        public string  State         { get; set; }
        public bool?   ToBeConfirmed { get; set; }
        public string  SellerFilter  { get; set; }
        public int?    PageNumber    { get; set; }
        public int?    PageSize      { get; set; }
    }

    /// <summary>Campos necessários para criação de um Customer.</summary>
    public class AdmCustomerCreateModel
    {
        public int     CompanyId     { get; set; }
        public bool?   ToBeConfirmed { get; set; }
        public string  CustomerCode  { get; set; }
        public string  Name          { get; set; }
        public string  CNPJ          { get; set; }
        public string  Street        { get; set; }
        public string  Street2       { get; set; }
        public string  Neighborhood  { get; set; }
        public string  City          { get; set; }
        public string  State         { get; set; }
        public string  ZipCode       { get; set; }
        public string  OriginCd      { get; set; }
        public int?    DataImportId  { get; set; }
    }

    /// <summary>Campos necessários para atualização de um Customer.</summary>
    public class AdmCustomerUpdateModel
    {
        public int     CustomerId    { get; set; }
        public int     CompanyId     { get; set; }
        public bool?   ToBeConfirmed { get; set; }
        public string  CustomerCode  { get; set; }
        public string  Name          { get; set; }
        public string  CNPJ          { get; set; }
        public string  Street        { get; set; }
        public string  Street2       { get; set; }
        public string  Neighborhood  { get; set; }
        public string  City          { get; set; }
        public string  State         { get; set; }
        public string  ZipCode       { get; set; }
        public string  OriginCd      { get; set; }
        public int?    DataImportId  { get; set; }
    }
}

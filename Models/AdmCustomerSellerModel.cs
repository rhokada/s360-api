using System;

namespace WebApi.Models
{
    /// <summary>Representa a entidade completa da tabela CustomerSeller com dados de JOIN.</summary>
    public class AdmCustomerSellerModel
    {
        public int       CustomerSellerId  { get; set; }
        public int       CustomerId        { get; set; }
        public string    CustomerName      { get; set; }
        public string    CustomerCode      { get; set; }
        public int       SellerUserId      { get; set; }
        public string    SellerName        { get; set; }
        public string    SellerEmail       { get; set; }
        public bool      Active            { get; set; }
        public DateTime? DhCreate          { get; set; }
        public DateTime? DhUpdate          { get; set; }
        public string    Log               { get; set; }
    }

    /// <summary>Filtros opcionais para consulta de CustomerSeller.</summary>
    public class AdmCustomerSellerFilterModel
    {
        public int?   CustomerId    { get; set; }
        public int?   SellerUserId  { get; set; }
        public bool?  Active        { get; set; }
    }

    /// <summary>Campos necessários para criação de um CustomerSeller.</summary>
    public class AdmCustomerSellerCreateModel
    {
        public int    CustomerId    { get; set; }
        public int    SellerUserId  { get; set; }
        public bool?  Active        { get; set; }
    }

    /// <summary>Campos necessários para atualização de um CustomerSeller.</summary>
    public class AdmCustomerSellerUpdateModel
    {
        public int    CustomerSellerId  { get; set; }
        public int    CustomerId        { get; set; }
        public int    SellerUserId      { get; set; }
        public bool?  Active            { get; set; }
    }
}

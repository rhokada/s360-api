using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
   public class BoletoRegistroModel
   {
      public int promotionGroupId { get; set; }
      [Required]

      public string cpf_cliente { get; set; }

      [Required]
      public string cnpj_emissor { get; set; }

      [Required]
      public string nr_boleto { get; set; }

      [Required]
      public string vl_boleto { get; set; }

      [Required]
      public string dt_vcto { get; set; }

      [Required]
      public string dt_pagto { get; set; }

     [Required]
     public string email { get; set; }

     [Required]
     public string customername { get; set; }

    }
}
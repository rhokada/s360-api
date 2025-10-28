using System;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class WhatsAppProductsModel
    {
        [Required]
        public int customerId { get; set; }
        [Required]
        public int promotionId { get; set; }
        [Required]
        public int purchasereceiptId { get; set; }
        [Required]
        public int QrCodeId { get; set; }
        [Required]
        public DateTime CaptureDateTime { get; set; }
        [Required]
        public int QttProdutcs { get; set; }
        [Required]
        public int ValueProdutcs { get; set; }
    }
}
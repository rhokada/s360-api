using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class WhatsAppCoupomModel
    {
        [Required]
        public int customerId { get; set; }

        [Required]
        public int promotionId { get; set; }

        [Required]
        public string UrlImage { get; set; }
        [Required]
        public int QR { get; set; }
        [Required]
        public int MensagemId { get; set; }
        


    }
}
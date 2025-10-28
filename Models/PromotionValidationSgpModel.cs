using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class PromotionValidationSgpModel
    {
        public int? PromotionId { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string? Serie { get; set; }
        public string? ValidationShortDescription { get; set; }
        public string? PurchaseReceiptValidationStatusShortDescription { get; set; }
        public string? PrizeStatusShortDescription { get; set; }

        public int? ProfileId { get; set; }
        public int? PromotionValidationId { get; set; }
        public string? InputPurchaseReceiptValidationStatusId { get; set; }
        public string? Analysis { get; set; }
        public int? PurchaseReceiptId { get; set; }
        public string? JSonProductSefaz { get; set; }
        public string? AccessCode { get; set; }
        public string? Notes { get; set; }
        //@JSonProductSefaz
    }
}

using System;

namespace WebApi.Models
{
    public class PurchaseReceipt
    {
        public virtual int PurchaseReceiptId { get; set; }
        public virtual DateTime? PurchaseDate { get; set; }
        public virtual string PurchaseNumber { get; set; }
        public virtual string PurchaseCaptured { get; set; }
        public virtual string AccessCode { get; set; }
        public virtual int? PromotionId { get; set; }
        public virtual int? CustomerId { get; set; }
        public virtual string SellerId { get; set; }
        public virtual DateTime? CaptureDate { get; set; }
        public virtual decimal? Price { get; set; }
        public virtual bool? IsCouponPromotion { get; set; }
        public virtual bool? IsValidCouponPromotion { get; set; }
        public virtual int? Points { get; set; }
        public virtual bool? IsImage { get; set; }
        public virtual string CaptureLat { get; set; }
        public virtual string CaptureLong { get; set; }
        public virtual int? PurchaseReceiptStatusId { get; set; }
        public virtual string StatusObservation { get; set; }
        public virtual DateTime? DateAlterPointStatus { get; set; }
        public virtual int? UserAlterPointStatus { get; set; }
        public virtual string PointStatusProcess { get; set; }
        public virtual DateTime? EndProcessDate { get; set; }
        public virtual bool? IsProcessFinished { get; set; }
        public virtual string PurchaseStatus { get; set; }
        public string recaptcha { get; set; }
    }
}
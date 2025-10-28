using System;

namespace WebApi.Entities
{
    public class QrCaptured
    {
        public virtual int QRCapturedId { get; set; }
        public virtual int? QRCodeId { get; set; }
        public virtual int? PromotionId { get; set; }
        public virtual int? CustomerId { get; set; }
        public virtual DateTime? CaptureDateTime { get; set; }
        public virtual string CaptureLat { get; set; }
        public virtual string CaptureLong { get; set; }
        public virtual int? PurchaseReceiptId { get; set; }
        public virtual int? CompanyId { get; set; }
        public virtual int? DistanceFromCompany { get; set; }
        public virtual int? Qtt { get; set; }
        public virtual decimal? ValueProducts { get; set; }
    }
}
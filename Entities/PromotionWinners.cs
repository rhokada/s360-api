using System;

namespace WebApi.Entities
{
    public class PromotionWinners
    {
        public virtual int PromotionWinnersId { get; set; }
        public virtual int? PromotionId { get; set; }
        public virtual DateTime? DateProc { get; set; }
        public virtual int? ProcId { get; set; }
        public virtual int? CustomerId { get; set; }
        public virtual int? Sequence { get; set; }
        public virtual string CustomerInfo { get; set; }
        public virtual string PrizesInfo { get; set; }
    }
}
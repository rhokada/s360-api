namespace WebApi.Entities
{
    public class PromotionParameters
    {
        public virtual int? promotionId { get; set; }
        public virtual string Promo { get; set; }
        public virtual string DtIni { get; set; }
        public virtual string DtFim { get; set; }
        public virtual string UrlPromo { get; set; }
        public virtual string Status { get; set; }
    }
}
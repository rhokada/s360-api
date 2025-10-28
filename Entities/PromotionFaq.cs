namespace WebApi.Entities
{
    public class PromotionFaq
    {
        public virtual int PromotionFaqId { get; set; }
        public virtual int PromotionId { get; set; }
        public virtual string Question { get; set; }
        public virtual string Answer { get; set; }
        public virtual int? Seq { get; set; }
    }
}
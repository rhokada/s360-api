

namespace WebApi.Models
{
    public class CustomerLoyaltyPointsFilter
    {
        public int PromotionId { get; set; }
        public int DaysFilter { get; set; }
        public string MonthYearFilter { get; set; }
    }
}

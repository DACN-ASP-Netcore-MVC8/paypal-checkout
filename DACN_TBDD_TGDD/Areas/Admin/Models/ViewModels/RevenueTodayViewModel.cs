namespace DACN_TBDD_TGDD.Areas.Admin.Models.ViewModels
{
    public class RevenueTodayViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public Dictionary<long, ProductSaleViewModel> ProductSales { get; set; }
    }
}

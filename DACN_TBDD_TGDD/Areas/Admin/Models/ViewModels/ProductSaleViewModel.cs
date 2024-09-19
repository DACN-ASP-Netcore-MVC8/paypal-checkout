namespace DACN_TBDD_TGDD.Areas.Admin.Models.ViewModels
{
    public class ProductSaleViewModel
    {
        public string ProductName { get; set; }
        public long SoldQuantity { get; set; }
        public int Stock
        {
            get; set;
        }
    }
}

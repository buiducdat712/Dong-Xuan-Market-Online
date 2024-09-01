namespace Dong_Xuan_Market_Online.Models.ViewModels
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; }
        public decimal GrandTotal { get; set; }
        public string AppliedVoucherCode { get; set; }
        public decimal? DiscountAmount { get; set; }
    }
}

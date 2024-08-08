namespace Dong_Xuan_Market_Online.Models.ViewModels
{
    public class ProductViewModel
    {
        public IEnumerable<ProductModel> Products { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public string SelectedCategory { get; set; }
    }
}

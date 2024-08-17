namespace Dong_Xuan_Market_Online.Models.ViewModels
{
    public class ProductEditViewModel
    {
        public ProductModel Product { get; set; }
        public List<ProductImages> ProductImages { get; set; }

        public ProductEditViewModel()
        {
            ProductImages = new List<ProductImages>();
        }
    }
}

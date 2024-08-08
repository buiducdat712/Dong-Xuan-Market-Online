namespace Dong_Xuan_Market_Online.Models.ViewModels
{
    public class ProductPaginateViewModel
    {
        public IEnumerable<ProductModel> Products { get; set; }
        public Paginate Paginate { get; set; }
        public IEnumerable<ProductModel> SidebarProducts { get; set; }

        public string SelectedCate { get; set; } // Thêm thuộc tính này
    }
}

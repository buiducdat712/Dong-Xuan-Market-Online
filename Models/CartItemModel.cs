namespace Dong_Xuan_Market_Online.Models
{
    public class CartItemModel
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public string SellerId { get; set; }  // Đảm bảo thuộc tính này tồn tại

        public decimal Total
        {
            get { return Quantity * Price; }
        }

        public CartItemModel() { }

        public CartItemModel(ProductModel product)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            Price = product.Price;
            Quantity = 1;
            Image = product.Image;
            SellerId = product.SellerId;  // Gán SellerId từ ProductModel
        }

        public ProductModel Product { get; set; }
    }
}

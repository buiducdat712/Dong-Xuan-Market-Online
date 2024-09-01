using Dong_Xuan_Market_Online.Models;
using Dong_Xuan_Market_Online.Repository;

namespace Dong_Xuan_Market_Online.Services
{
    public class OrderService : IOrderService
    {
        private readonly DataContext _dataContext;

        public OrderService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task ProcessOrder(OrderModel order)
        {
            foreach (var orderItem in order.OrderDetails)
            {
                var product = await _dataContext.Products.FindAsync(orderItem.ProductId);
                if (product != null)
                {
                    if (product.StockQuantity < orderItem.Quantity)
                    {
                        throw new Exception($"Không đủ số lượng sản phẩm {product.Name} trong kho.");
                    }

                    product.StockQuantity -= orderItem.Quantity;
                    product.SoldQuantity = (product.SoldQuantity ?? 0) + orderItem.Quantity;

                    _dataContext.Products.Update(product);
                }
            }
            await _dataContext.SaveChangesAsync();
        }
    }
}

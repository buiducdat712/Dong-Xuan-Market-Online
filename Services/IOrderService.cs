using Dong_Xuan_Market_Online.Models;

namespace Dong_Xuan_Market_Online.Services
{
    public interface IOrderService
    {
        Task ProcessOrder(OrderModel order);
    }
}

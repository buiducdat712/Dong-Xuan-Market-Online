using Microsoft.AspNetCore.Mvc;

namespace Dong_Xuan_Market_Online.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public ProductModel Product { get; set; }
        public RatingModel RatingModel { get; set; }
        public List<ProductImages> SortedImages { get; set; }
        public List<ProductModel> RelatedProducts { get; set; }
        public string ActionName { get; set; }
        public List<RatingModel> Ratings { get; set; }
        public RatingModel NewRating { get; set; }

        public ProductDetailsViewModel()
        {
            NewRating = new RatingModel();
        }
        // Tính toán điểm đánh giá trung bình
        public double AverageRating
        {
            get
            {
                if (Ratings == null || Ratings.Count == 0)
                    return 0;

                return Ratings.Average(r => r.Rating);
            }
        }

        // Lấy số sao đầy đủ và số sao rỗng để hiển thị
        public int FullStarCount => (int)AverageRating;
        public int EmptyStarCount => 5 - FullStarCount;
    }
}

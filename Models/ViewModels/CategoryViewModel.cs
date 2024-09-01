namespace Dong_Xuan_Market_Online.Models.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public ICollection<CategorySubModel> SubCategories { get; set; } = new HashSet<CategorySubModel>();
    }
}

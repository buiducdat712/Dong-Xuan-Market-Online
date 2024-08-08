using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dong_Xuan_Market_Online.Repository.Components
{
    public class CategoriesViewComponent : ViewComponent
    {
        public readonly DataContext _dataContext;
        public CategoriesViewComponent(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.Categories.ToListAsync());
    }
}

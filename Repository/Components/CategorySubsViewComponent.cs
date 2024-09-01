using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dong_Xuan_Market_Online.Repository.Components
{
    [ViewComponent(Name = "CategorySubs")]
    public class CategorySubsViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;

        public CategorySubsViewComponent(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int categoryId)
        {
            var categorySubs = await _dataContext.CategorySubModels
                .Where(c => c.CategoryId == categoryId)
                .ToListAsync();
            return View("Default", categorySubs);
        }
    }
}

﻿using Dong_Xuan_Market_Online.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dong_Xuan_Market_Online.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string Slug { get; set; }
        public int Status { get; set; }

        public string Logo { get; set; } = "noimage.jpg";
        public ICollection<CategorySubModel> SubCategories { get; set; } = new HashSet<CategorySubModel>();

        [NotMapped]
        [FileExtension]
        public IFormFile ImageUpLoad { get; set; }
    }
}

﻿using System.ComponentModel.DataAnnotations;
using _16Nov_task.Models;

namespace _16Nov_task.Areas.ProniaAdmin.ViewModels
{
    public class UpdateProductVM
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
        public IFormFile? MainPhoto { get; set; }
        public IFormFile? HoverPhoto { get; set; }
        public List<IFormFile>? Photos { get; set; }
        public int CategoryId { get; set; }
        public List<int> TagIds { get; set; }
        public List<int>? ImageIds { get; set; }
        public List<Category>? Categories { get; set; }
        public List<Tag>? Tags { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using _16Nov_task.Models;

namespace _16Nov_task.Areas.ProniaAdmin.ViewModels
{
    public class CreateProductVM
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
        [Required]
        public int? CategoryId { get; set; }
    }  
}

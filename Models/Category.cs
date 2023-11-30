using System.ComponentModel.DataAnnotations;

namespace _16Nov_task.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="Ad mutleq daxil edilmelidir")]
        [MaxLength(25, ErrorMessage="Adin uzunlugu 25-den chox olamamlidir")]
        public string Name { get; set; }
        public List<Product>? Products { get; set; }
    }
}

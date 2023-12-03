using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _16Nov_task.Models
{
    public class Slide
    {
        public int Id { get; set; }
        [Required(ErrorMessage="basliq mutleq daxil edilmelidir")]
        [MaxLength(50, ErrorMessage ="basliq ucun maksimal uzunluq 50-dir.")]
        public string Title { get; set; }
        [MaxLength(30)]
        public string Subtitle { get; set; }
        [MaxLength(250)]
        public string Description { get; set; }
        public string? Image { get; set; }
        public int Order { get; set; }
        [NotMapped]
        public IFormFile? Photo { get; set; }
    }
}

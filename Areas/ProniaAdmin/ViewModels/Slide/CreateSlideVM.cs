using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace _16Nov_task.Areas.ProniaAdmin.ViewModels
{
    public class CreateSlideVM
    {
        [Required(ErrorMessage = "basliq mutleq daxil edilmelidir")]
        [MaxLength(50, ErrorMessage = "basliq ucun maksimal uzunluq 50-dir.")]
        public string Title { get; set; }
        [MaxLength(30)]
        public string Subtitle { get; set; }
        [MaxLength(250)]
        public string Description { get; set; }
        public int Order { get; set; }
        [NotMapped]
        [Required]
        public IFormFile Photo { get; set; }
    }
}

using _16Nov_task.Models;

namespace _16Nov_task.ViewModels
{
    public class OrderVM
    {
        public string Adress { get; set; }
        public List<BasketItem>? BasketItems { get; set; }
    }
}

using _16Nov_task.Models;

namespace _16Nov_task.ViewModels
{
    public class BasketItemVM
    {
        public  int Id { get; set; }
        public string Name { get; set; }
        public string Image {  get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public decimal SubTotal { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}

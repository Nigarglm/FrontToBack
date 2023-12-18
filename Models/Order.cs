namespace _16Nov_task.Models
{
    public class Order
    {
        public int Id { get; set; }
        public bool? Status { get; set; }
        public string Adress { get; set; }
        public DateTime PurchaseAt { get; set; }
        public decimal TotalPrice { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public List<BasketItem> BasketItems { get; set; }

    }
}

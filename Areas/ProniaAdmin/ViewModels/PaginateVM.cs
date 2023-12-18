namespace _16Nov_task.Areas.ProniaAdmin.ViewModels
{
    public class PaginateVM<T> where T : class, new()
    {
        public int CurrentPage { get; set; }
        public double TotalPage { get; set; }
        public List<T> Items { get; set;}
    }
}

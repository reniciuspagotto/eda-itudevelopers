namespace EventDrivenArchitectureExample.Data.Messages
{
    public class OrderPlaced
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

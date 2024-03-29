﻿namespace EventDrivenArchitectureExample.Data.Entities
{
    public class Order
    {
        public Order(int id, int productId, int quantity, decimal totalPurchase, string status)
        {
            Id = id;
            ProductId = productId;
            Quantity = quantity;
            TotalPurchase = totalPurchase;
            Status = status;
        }

        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPurchase { get; set; }
        public string Status { get; set; }
    }
}

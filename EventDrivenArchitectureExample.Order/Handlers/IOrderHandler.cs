﻿using EventDrivenArchitectureExample.Data.Messages;
using System.Threading.Tasks;

namespace EventDrivenArchitectureExample.Order.Handlers
{
    public interface IOrderHandler
    {
        Task<string> Create(Data.Entities.Order order);
        Task Cancel(OrderCreatedCompensationMessage outOfStockMessage);
        Task Finalize(PaymentCheckedMessage outOfStockMessage);
        Task<Data.Entities.Order> GetById(int orderId);
    }
}

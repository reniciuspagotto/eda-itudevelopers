﻿using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using EventDrivenArchitectureExample.Data.Messages;
using EventDrivenArchitectureExample.Data.Settings;
using EventDrivenArchitectureExample.Order.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventDrivenArchitectureExample.Order.HostedService
{
    public class ProcessCancelOrderConsumer : BackgroundService
    {
        private readonly IOrderHandler _orderHandler;

        public ProcessCancelOrderConsumer(IServiceProvider serviceProvider)
        {
            _orderHandler = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IOrderHandler>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var eventHubConnectionString = ApplicationSettings.EventHubConnection;
            var eventHubName = "order-created-compensation";

            var blobStorageConnectionString = ApplicationSettings.BlobConnection;
            var productContainer = "order-created-compensation-manager";

            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

            BlobContainerClient storageClient = new BlobContainerClient(blobStorageConnectionString, productContainer);
            EventProcessorClient processor = new EventProcessorClient(storageClient, consumerGroup, eventHubConnectionString, eventHubName);

            processor.ProcessEventAsync += ProcessEventHandler;
            processor.ProcessErrorAsync += ProcessErrorHandler;

            await processor.StartProcessingAsync();
        }

        public async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            var orderMessage = JsonSerializer.Deserialize<OrderCreatedCompensationMessage>(eventArgs.Data.Body.ToArray());

            Console.WriteLine("\tReceived event: {0}", orderMessage);

            await _orderHandler.Cancel(orderMessage);

            await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
        }

        public static Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            // Write details about the error to the console window
            Console.WriteLine($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            Console.WriteLine(eventArgs.Exception.Message);
            return Task.CompletedTask;
        }
    }
}

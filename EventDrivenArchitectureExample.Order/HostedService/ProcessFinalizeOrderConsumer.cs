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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventDrivenArchitectureExample.Order.HostedService
{
    public class ProcessFinalizeOrderConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderHandler _orderHandler;

        public ProcessFinalizeOrderConsumer(IServiceProvider serviceProvider)
        {
            _orderHandler = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IOrderHandler>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var eventHubConnectionString = ApplicationSettings.EventHubConnection;
            var eventHubName = "payment-checked";

            var blobStorageConnectionString = ApplicationSettings.BlobConnection;
            var productContainer = "payment-checked-manager";

            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

            BlobContainerClient storageClient = new BlobContainerClient(blobStorageConnectionString, productContainer);
            EventProcessorClient processor = new EventProcessorClient(storageClient, consumerGroup, eventHubConnectionString, eventHubName);

            processor.ProcessEventAsync += ProcessEventHandler;
            processor.ProcessErrorAsync += ProcessErrorHandler;

            await processor.StartProcessingAsync();
        }

        public async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            var orderMessage = JsonSerializer.Deserialize<PaymentCheckedMessage>(eventArgs.Data.Body.ToArray());

            Console.WriteLine("\tReceived event: {0}", orderMessage);

            await _orderHandler.Finalize(orderMessage);

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

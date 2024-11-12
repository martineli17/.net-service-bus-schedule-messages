using Azure.Messaging.ServiceBus;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Integrations.MessageBus
{
    public class ServiceBus
    {
        private readonly ServiceBusClient _client;

        public ServiceBus(string connectionString)
        {
            _client = new(connectionString);
        }

        public async Task<MessageBusScheduleRequest<TContent>.Response> ScheduleMessageAsync<TContent>(MessageBusScheduleRequest<TContent> request, CancellationToken cancellationToken)
        {
            var sender = _client.CreateSender(request.QueueName);
            var message = new ServiceBusMessage(JsonSerializer.Serialize(request.Payload));
            var messageNumber = await sender.ScheduleMessageAsync(message, TimeZoneInfo.ConvertTimeToUtc(request.ScheduleTo.Value), cancellationToken);
            await sender.CloseAsync(cancellationToken).ConfigureAwait(false);

            return new()
            {
                MessageNumber = messageNumber,
            };
        }

        public async Task CancelScheduleMessageAsync(MessageBusCancelScheduledRequest request, CancellationToken cancellationToken)
        {
            var sender = _client.CreateSender(request.QueueName);
            await sender.CancelScheduledMessageAsync(request.MessageNumber, cancellationToken);
            await sender.CloseAsync(cancellationToken).ConfigureAwait(false);    
        }

        public async Task SubscribeAsync<TContent>(MessageBusSubscribeRequest<TContent> request)
        {
            var processor = _client.CreateProcessor(request.QueueName, new ServiceBusProcessorOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5),
            });

            processor.ProcessErrorAsync += (error) =>
            {
                Console.WriteLine($"Error from Service Bus Proccess: {error.EntityPath} - {error.Exception.InnerException}");
                return Task.CompletedTask;
            };

            processor.ProcessMessageAsync += async (@event) =>
            {
                try
                {
                    var payload = JsonSerializer.Deserialize<TContent>(@event.Message.Body);
                    var result = await request.Handler(payload);

                    if (result)
                        await @event.CompleteMessageAsync(@event.Message);
                    else
                        await @event.DeadLetterMessageAsync(@event.Message);
                }
                catch (Exception ex)
                {
                    await @event.AbandonMessageAsync(@event.Message);
                    Console.WriteLine($"Error from Service Bus Proccess: {@event.EntityPath} - {ex.InnerException}");
                }
            };

            await processor.StartProcessingAsync();
        }
    }
}

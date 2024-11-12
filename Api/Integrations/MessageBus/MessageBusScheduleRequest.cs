using System;

namespace Api.Integrations.MessageBus
{
    public readonly struct MessageBusScheduleRequest<TContent>
    {
        public TContent Payload { get; init; }
        public string QueueName { get; init; }
        public DateTime? ScheduleTo { get; init; }

        public readonly struct Response
        {
            public long MessageNumber { get; init; }
        }
    }
}

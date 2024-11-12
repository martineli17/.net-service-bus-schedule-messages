namespace Api.Integrations.MessageBus
{
    public readonly struct MessageBusCancelScheduledRequest
    {
        public long MessageNumber { get; init; }
        public string QueueName { get; init; }
    }
}

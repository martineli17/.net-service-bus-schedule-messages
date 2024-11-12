using System;
using System.Threading.Tasks;

public readonly struct MessageBusSubscribeRequest<TContent>
{
    public string QueueName { get; init; }
    public Func<TContent, Task<bool>> Handler { get; init; }
}

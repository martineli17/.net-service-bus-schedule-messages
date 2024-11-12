using System;

namespace Api.Dtos
{
    public readonly struct AddSchedulingRequest
    {
        public string Description { get; init; }
        public DateTime ScheduleTo { get; init; }
    }
}

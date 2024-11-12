using System;

namespace Api.Entities
{
    public class Scheduling
    {
        public long MessageNumber { get; init; }
        public string RedirectToQueue { get; init; }
        public string Description { get; init; }
        public DateTime ScheduleTo { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}

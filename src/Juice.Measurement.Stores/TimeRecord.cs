namespace Juice.Measurement.Stores
{
	public record TimeRecord
	{
        public required string Name { get; init; }
        public required string FullName { get; init; }
        public TimeSpan StartedTime { get; init; }
        public TimeSpan ElapsedTime { get; init; }
        public DateTimeOffset RecordedDate { get; init; } = DateTimeOffset.Now;
        public required string ScopeId { get; init; }
        public required string TraceId { get; init; }
    }
}

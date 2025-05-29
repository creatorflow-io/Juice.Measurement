namespace Juice.Measurement.Stores
{
	public record TimeRecord
	{
        public string Name { get; init; }
        public string FullName { get; init; }
        public TimeSpan StartedTime { get; init; }
        public TimeSpan ElapsedTime { get; init; }
        public DateTimeOffset RecordedDate { get; init; }
        public string ScopeId { get; init; }
        public string TraceId { get; init; }

        public TimeRecord(string name, string fullName, TimeSpan startedTime, TimeSpan elapsedTime, string scopeId, string traceId, DateTimeOffset recordedDate)
        {
            Name = name;
            FullName = fullName;
            StartedTime = startedTime;
            ElapsedTime = elapsedTime;
            ScopeId = scopeId;
            TraceId = traceId;
            RecordedDate = recordedDate;
        }
    }
}

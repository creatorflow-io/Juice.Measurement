namespace Juice.Measurement.Stores
{
    public record TimeSummary(string TraceId, string Name, string RootScopeId, string Summary)
    {
        public DateTimeOffset RecordedDate { get; init; } = DateTimeOffset.Now;
    }
}

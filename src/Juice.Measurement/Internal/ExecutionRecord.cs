namespace Juice.Measurement.Internal
{
    public record ExecutionRecord(string OriginalScopeName, string ScopeName, int Depth, TimeSpan ElapsedTime);
}

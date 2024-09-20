namespace Juice.Measurement
{
    public record ExecutionRecord(string OriginalScopeName, string ScopeName, int Depth, TimeSpan ElapsedTime);
}

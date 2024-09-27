namespace Juice.Measurement.Internal
{
    public record ScopeEnd(string OriginalScopeName, string Name, int Depth, TimeSpan ElapsedTime): ITrackRecord;
}

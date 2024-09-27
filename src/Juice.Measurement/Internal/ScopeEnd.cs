namespace Juice.Measurement.Internal
{
    public record ScopeEnd(string Name, string FullName, int Depth, TimeSpan ElapsedTime): ITrackRecord;
}

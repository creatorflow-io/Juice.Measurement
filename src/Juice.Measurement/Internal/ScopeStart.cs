namespace Juice.Measurement.Internal
{
    public record ScopeStart(string Name, int Depth, TimeSpan ElapsedTime) : ITrackRecord;
}

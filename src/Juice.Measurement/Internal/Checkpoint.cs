namespace Juice.Measurement.Internal
{
    public record Checkpoint(string Name, int Depth, TimeSpan ElapsedTime) : ITrackRecord;
}

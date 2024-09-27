namespace Juice.Measurement.Internal
{
    public record Checkpoint(string Name, string FullName, int Depth, TimeSpan ElapsedTime) : ITrackRecord;
}

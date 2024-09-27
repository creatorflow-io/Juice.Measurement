namespace Juice.Measurement.Internal
{
    public interface ITrackRecord
    {
        string Name { get; }
        int Depth { get; }
        TimeSpan ElapsedTime { get; }
    }
}

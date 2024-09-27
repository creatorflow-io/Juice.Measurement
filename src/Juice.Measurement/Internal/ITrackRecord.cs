namespace Juice.Measurement.Internal
{
    public interface ITrackRecord
    {
        string Name { get; }
        string FullName { get; }
        int Depth { get; }
        TimeSpan ElapsedTime { get; }
    }
}

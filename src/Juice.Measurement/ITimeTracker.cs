namespace Juice.Measurement
{
    public interface ITimeTracker: IDisposable
    {
        /// <summary>
        /// Execution records.
        /// </summary>
        List<ExecutionRecord> Records { get; }

        /// <summary>
        /// Create a new scope to tracking time.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IDisposable NewExecutionScope(string name);

        string ToString(bool displayMillisecond);
    }
}

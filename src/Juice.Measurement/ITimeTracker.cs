using Juice.Measurement.Internal;

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
        IDisposable BeginScope(string name);

        /// <summary>
        /// Print the elapsed time from begin scope.
        /// </summary>
        /// <param name="name"></param>
        void Checkpoint(string name);

        /// <summary>
        /// Print the execution records in a table.
        /// </summary>
        /// <param name="displayMillisecond"></param>
        /// <param name="maxDepth"></param>
        /// <returns></returns>
        string ToString(bool displayMillisecond, int? maxDepth = default);
    }
}

using System.Diagnostics;
using Juice.Measurement.Internal;

namespace Juice.Measurement
{
    public interface ITimeTracker : IDisposable
    {
        TimeSpan ElapsedTime { get; }
        /// <summary>
        /// Execution records.
        /// </summary>
        List<ITrackRecord> Records { get; }

        /// <summary>
        /// Create a new scope to tracking time.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        IDisposable BeginScope(string name, string? scopeId = default);

        /// <summary>
        /// Print the elapsed time from begin scope.
        /// </summary>
        /// <param name="name"></param>
        void Checkpoint(string name);

        /// <summary>
        /// Print the execution records in a table.
        /// </summary>
        /// <param name="humanReadable"></param>
        /// <param name="maxDepth"></param>
        /// <param name="checkpoint"></param>
        /// <returns></returns>
        string ToString(bool humanReadable, int? maxDepth = default, bool checkpoint = true);

        ICollection<ScopeRecord> GetScopes()
            => Records.OfType<IScope>().Where(x => x.ScopeId != null).DistinctBy(x => x.ScopeId!)
                .Select(s =>
                {
                    var start = Records.OfType<ScopeStart>().FirstOrDefault(x => x.ScopeId == s.ScopeId);
                    var end = Records.OfType<ScopeEnd>().FirstOrDefault(x => x.ScopeId == s.ScopeId);
                    if (start == null || end == null)
                    {
                        return null;
                    }
                    return new ScopeRecord(start.Name, start.FullName, start.Depth, start.RecordTime, end.ElapsedTime, start.ScopeId!);
                }).OfType<ScopeRecord>().ToArray();
    }
}

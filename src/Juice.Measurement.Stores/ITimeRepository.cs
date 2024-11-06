using Juice.Measurement.Internal;

namespace Juice.Measurement.Stores
{
    public interface ITimeRepository
    {
        /// <summary>
        /// Get all time records for a trace id
        /// </summary>
        /// <param name="traceId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<TimeRecord>> GetTimeRecordsAsync(string traceId, CancellationToken token);
        /// <summary>
        /// Get time summary for a trace id
        /// </summary>
        /// <param name="traceId"></param>
        /// <returns></returns>
        Task<TimeSummary?> GetTimeSummaryAsync(string traceId);
        /// <summary>
        /// Save track data
        /// </summary>
        /// <returns></returns>
        Task SaveTrackDataAsync(TimeSummary summary, IEnumerable<TimeRecord> records);
    }

    public static class TimeRepositoryExtensions
    {
        public static Task SaveTrackDataAsync(this ITimeRepository repository, ITimeTracker tracker, string traceId, string scopeId, string name)
        {
            var summary = new TimeSummary(traceId, name, scopeId, tracker.ToString(true));
            var records = tracker.GetScopes()
                .Select(s => new TimeRecord
                {
                    FullName = s.FullName,
                    Name = s.Name,
                    StartedTime = s.StartedTime,
                    ElapsedTime = s.ElapsedTime,
                    ScopeId = s.ScopeId,
                    TraceId = traceId,
                    RecordedDate = summary.RecordedDate
                });

            return repository.SaveTrackDataAsync(summary, records);
        }
    }
}

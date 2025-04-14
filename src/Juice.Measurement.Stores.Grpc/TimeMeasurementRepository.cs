using Google.Protobuf.WellKnownTypes;
using Juice.Measurement.Api.Grpc;

namespace Juice.Measurement.Stores.Grpc
{
    internal class TimeMeasurementRepository(TimeStore.TimeStoreClient client, ITimeTracker? tracker = default) : ITimeRepository
    {
        private readonly TimeStore.TimeStoreClient _client = client;

        public async Task<IEnumerable<TimeRecord>> GetTimeRecordsAsync(string traceId, CancellationToken token)
        {
            var request = new Filter { TraceId = traceId };
            var response = await _client.GetRecordsAsync(request, cancellationToken: token);
            return response.Records.Select(x => new TimeRecord
            {
                Name = x.Name,
                FullName = x.FullName,
                StartedTime = x.Started.ToTimeSpan(),
                ElapsedTime = x.Elapsed.ToTimeSpan(),
                RecordedDate = response.RecordedDate.ToDateTimeOffset(),
                ScopeId = x.ScopeId,
                TraceId = traceId
            });
        }

        public async Task<TimeSummary?> GetTimeSummaryAsync(string traceId)
        {
            var request = new Filter { TraceId = traceId };

            var response = await _client.GetSummaryAsync(request);
            var summary = response.Value;
            return summary == null ? null
                : new TimeSummary(traceId, summary.Name, summary.ScopeId, summary.Summary_)
            {
                RecordedDate = summary.RecordedDate.ToDateTimeOffset()
            };
        }

        public async Task SaveTrackDataAsync(TimeSummary summary, IEnumerable<TimeRecord> records)
        {
            using var _ = tracker?.BeginScope("Save track data", "timetracker.stores.grpc.save");
            var data = new TimeData
            {
                Name = summary.Name,
                ScopeId = summary.RootScopeId,
                Summary = summary.Summary,
                TraceId = summary.TraceId,
                RecordedDate = Timestamp.FromDateTimeOffset(summary.RecordedDate),
                Records = { records.Select(x => new Api.Grpc.TimeRecord
                {
                    Name = x.Name,
                    FullName = x.FullName,
                    Started = Duration.FromTimeSpan(x.StartedTime),
                    Elapsed = Duration.FromTimeSpan(x.ElapsedTime),
                    ScopeId = x.ScopeId
                }) }
            };
            await _client.SaveAsync(data);
        }
    }
}

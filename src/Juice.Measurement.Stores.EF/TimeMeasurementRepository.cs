using Microsoft.EntityFrameworkCore;

namespace Juice.Measurement.Stores.EF
{
    internal class TimeMeasurementRepository(MeasurementDbContext context,
        ITimeTracker? tracker = default) : ITimeRepository
    {
        public async Task<IEnumerable<TimeRecord>> GetTimeRecordsAsync(string traceId, CancellationToken token)
            => await context.TimeRecords.Where(x => x.TraceId == traceId).ToListAsync(token);
        public Task<TimeSummary?> GetTimeSummaryAsync(string traceId)
            => context.TimeSummaries.FirstOrDefaultAsync(x => x.TraceId == traceId);

        public async Task SaveTrackDataAsync(TimeSummary summary, IEnumerable<TimeRecord> records)
        {
            using var _ = tracker?.BeginScope("Save track data", "timetracker.stores.ef.save");
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            context.Add(summary);
            tracker?.Checkpoint("Add summary");
            context.AddRange(records);
            tracker?.Checkpoint("Add records");
            context.ChangeTracker.DetectChanges();
            tracker?.Checkpoint("Detect changes");
            await context.SaveChangesAsync();

        }
    }
}

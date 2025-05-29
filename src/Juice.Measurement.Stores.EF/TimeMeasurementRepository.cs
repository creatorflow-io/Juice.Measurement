using Microsoft.EntityFrameworkCore;

namespace Juice.Measurement.Stores.EF
{
    internal class TimeMeasurementRepository: ITimeRepository
    {
        private readonly MeasurementDbContext _context;
        private readonly ITimeTracker? _tracker;

        public TimeMeasurementRepository(MeasurementDbContext context, ITimeTracker? tracker = default)
        {
            this._context = context;
            this._tracker = tracker;
        }

        public async Task<IEnumerable<TimeRecord>> GetTimeRecordsAsync(string traceId, CancellationToken token)
            => await _context.TimeRecords.Where(x => x.TraceId == traceId).ToListAsync(token);
        public Task<TimeSummary?> GetTimeSummaryAsync(string traceId)
            => _context.TimeSummaries.FirstOrDefaultAsync(x => x.TraceId == traceId);

        public async Task SaveTrackDataAsync(TimeSummary summary, IEnumerable<TimeRecord> records)
        {
            using var _ = _tracker?.BeginScope("Save track data", "timetracker.stores.ef.save");
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            _context.Add(summary);
            _tracker?.Checkpoint("Add summary");
            _context.AddRange(records);
            _tracker?.Checkpoint("Add records");
            _context.ChangeTracker.DetectChanges();
            _tracker?.Checkpoint("Detect changes");
            await _context.SaveChangesAsync();

        }
    }
}

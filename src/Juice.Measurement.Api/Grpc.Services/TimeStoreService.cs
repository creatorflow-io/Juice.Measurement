
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Juice.Measurement.Stores;

namespace Juice.Measurement.Api.Grpc.Services
{
    internal class TimeStoreService(ITimeRepository timeRepository) : TimeStore.TimeStoreBase
    {
        private readonly ITimeRepository _timeRepository = timeRepository;

        public override async Task<TimeStoreResult> Save(TimeData request, ServerCallContext context)
        {
            try
            {
                var summary = new TimeSummary(request.TraceId, request.Name, request.ScopeId, request.Summary)
                {
                    RecordedDate = request.RecordedDate.ToDateTimeOffset()
                };
                var records = request.Records
                    .Select(s =>
                    {
                        return new Stores.TimeRecord
                        {
                            Name = s.Name,
                            FullName = s.FullName,
                            StartedTime = s.Started.ToTimeSpan(),
                            ElapsedTime = s.Elapsed.ToTimeSpan(),
                            ScopeId = s.ScopeId,
                            TraceId = request.TraceId,
                            RecordedDate = summary.RecordedDate
                        };
                    });
                await _timeRepository.SaveTrackDataAsync(summary, records);
                return new TimeStoreResult
                {
                    Succeeded = true
                };
            }
            catch (Exception ex)
            {
                return new TimeStoreResult
                {
                    Succeeded = false,
                    Message = ex.Message
                };
            }

        }

        public override async Task<SummaryValue?> GetSummary(Filter request, ServerCallContext context)
        {
            var sumamry = await _timeRepository.GetTimeSummaryAsync(request.TraceId);
            if (sumamry != null)
            {
                return new SummaryValue
                {
                    Value = new Summary
                    {
                        TraceId = sumamry.TraceId,
                        Name = sumamry.Name,
                        ScopeId = sumamry.RootScopeId,
                        Summary_ = sumamry.Summary,
                        RecordedDate = Timestamp.FromDateTimeOffset(sumamry.RecordedDate)
                    }
                };
            }
            return new SummaryValue();
        }

        public override async Task<TimeRecords> GetRecords(Filter request, ServerCallContext context)
        {
            var records = await _timeRepository.GetTimeRecordsAsync(request.TraceId, context.CancellationToken);
            return new TimeRecords
            {
                Records = { records.Select(x => new TimeRecord
                {
                    Name = x.Name,
                    FullName = x.FullName,
                    Started = Duration.FromTimeSpan(x.StartedTime),
                    Elapsed = Duration.FromTimeSpan(x.ElapsedTime),
                    ScopeId = x.ScopeId,
                })},
                RecordedDate = records.Count() > 0 ? Timestamp.FromDateTimeOffset(records.First().RecordedDate) : null
            };
        }
    }
}

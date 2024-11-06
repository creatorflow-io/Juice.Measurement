using System;
using Juice.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Juice.Measurement.Stores;
using System.Linq;
using Juice.Measurement.Internal;
using Xunit.Abstractions;
using Grpc.Net.Client.Balancer;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System.Collections.Generic;
using FluentAssertions;

namespace Juice.Measurement.Test.Helpers
{
    internal class StoreTestHelper
    {
        private static async Task<(TimeSummary?, TimeRecord[], string message, TimeSpan persistTime)> TestInternalAsync(
            IServiceProvider serviceProvider, string name)
        {
            var scopeId = "xunit.timetracker.scopeId";
            var tracker = serviceProvider.GetRequiredService<ITimeTracker>();
            using var _ = tracker.BeginScope(name, "xunit.timetracker");
            var repo = serviceProvider.GetRequiredService<ITimeRepository>();
            tracker.Checkpoint("Get repo");

            var idGenerator = serviceProvider.GetRequiredService<IStringIdGenerator>();
            var traceId = idGenerator.GenerateRandomId(6);
            tracker.Checkpoint("Generate id");
            await Task.Delay(10);
            using var _1 = tracker.BeginScope("Inner scope", "xunit.timetracker.inner");
            await Task.Delay(20);
            _1.Dispose();
            using var _2 = tracker.BeginScope("Inner scope 2");
            _2.Dispose();
            _.Dispose();
            using (tracker.BeginScope("Persist", "xunit.timetracker.persist"))
            {
                await repo.SaveTrackDataAsync(tracker, traceId, scopeId, "XUnit test");
            }
            var summary = await repo.GetTimeSummaryAsync(traceId);
            tracker.Checkpoint("Get summary");
            var records = await repo.GetTimeRecordsAsync(traceId, default);
            tracker.Checkpoint("Get records");
            var message = tracker.ToString(true);
            var persistTime = tracker.Records.OfType<ScopeEnd>().Single(s => s.ScopeId == "xunit.timetracker.persist").ElapsedTime;
            return (summary, records.ToArray(), message, persistTime);
        }

        public static async Task TestAsync(
            IServiceProvider serviceProvider, string name, ITestOutputHelper output)
        {
            var results = new List<(TimeSummary?, TimeRecord[], string, TimeSpan)>();
            {
                using var scope = serviceProvider.CreateScope();
                results.Add(await TestInternalAsync(scope.ServiceProvider, $"Warmup {name} store"));
            }

            await Parallel.ForAsync(0, 10, async (i, token) =>
            {
                using var scope = serviceProvider.CreateScope();

                results.Add(await TestInternalAsync(scope.ServiceProvider, $"Test {name} store"));
            });

            var max = results.Max(r => r.Item4);
            var min = results.Min(r => r.Item4);
            var avg = results.Average(r => r.Item4.TotalMilliseconds);

            output.WriteLine($"Max: {max.TotalMilliseconds} ms, Min: {min.TotalMilliseconds} ms, Avg: {avg} ms");

            var data = results.Select(r => r.Item4.TotalMilliseconds).ToArray();
            foreach (var item in data)
            {
                output.WriteLine(new string('#', Math.Min((int)Math.Round(item, MidpointRounding.ToEven), 100)) + " " + item.ToString());
            }

            foreach (var (summary, records, message, _) in results)
            {
                output.WriteLine(message ?? "NULL message");
                summary.Should().NotBeNull();
                records.Should().NotBeEmpty();
            }
        }
    }
}

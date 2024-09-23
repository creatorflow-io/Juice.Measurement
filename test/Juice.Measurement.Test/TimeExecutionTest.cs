using System.Linq;
using System.Threading.Tasks;
using Juice.Measurement.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Juice.Measurement.Test
{
    public class TimeExecutionTest
    {
        private readonly ITestOutputHelper _output;

        public TimeExecutionTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TimeTracker_shouldAsync()
        {
            ITimeTracker timeTracker = new TimeTracker();
            using (timeTracker.BeginScope("Test"))
            {
                // Do something
                await Task.Delay(12);

                using (timeTracker.BeginScope("Inner Test"))
                {
                    // Do something
                    await Task.Delay(15);

                    using (timeTracker.BeginScope("Inner Inner Test"))
                    {
                        // Do something
                        timeTracker.Checkpoint("Checkpoint 1");
                        await Task.Delay(20);
                        timeTracker.Checkpoint("Checkpoint 2");
                    }
                }

                using (timeTracker.BeginScope("Inner Test 2"))
                {
                    // Do something
                    await Task.Delay(10);
                    timeTracker.Checkpoint("Checkpoint 3");
                    timeTracker.Checkpoint("Checkpoint 4");
                }
            }
            _output.WriteLine(timeTracker.ToString());
            _output.WriteLine(timeTracker.ToString(false));
            _output.WriteLine("Longest run: " + timeTracker.Records.Where(r => r.Depth > 1).MaxBy(r => r.ElapsedTime));
        }
    }
}

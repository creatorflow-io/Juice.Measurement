using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
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
                timeTracker.Checkpoint("Checkpoint 0");

                using (timeTracker.BeginScope("Inner Test"))
                {
                    // Do something
                    await Task.Delay(15);

                    using (timeTracker.BeginScope("Inner Inner Test"))
                    {
                        // Do something
                        timeTracker.Checkpoint("Checkpoint 1.1");
                        await Task.Delay(20);
                        timeTracker.Checkpoint("Checkpoint 1.2");
                    }
                }
                timeTracker.Checkpoint("Checkpoint 1");
                using (timeTracker.BeginScope("Inner Test 2"))
                {
                    // Do something
                    await Task.Delay(10);
                    timeTracker.Checkpoint("Checkpoint 1.3");
                    timeTracker.Checkpoint("Checkpoint 1.4");
                }
            }
            _output.WriteLine(timeTracker.ToString());
            _output.WriteLine(timeTracker.ToString(false, 2));
            _output.WriteLine(timeTracker.ToString(true, 2, false));
            _output.WriteLine("Longest run: " + timeTracker.Records.Where(r => r.Depth > 1).MaxBy(r => r.ElapsedTime));
            timeTracker.Records.Should().Contain(c => c.Name == "Checkpoint 0");
            timeTracker.Records.Should().Contain(c => c.Name == "Checkpoint 1");
            timeTracker.Records.Should().Contain(c => c.Name == "Checkpoint 1.1");
            timeTracker.Records.Should().Contain(c => c.Name == "Checkpoint 1.2");
            timeTracker.Records.Should().Contain(c => c.Name == "Checkpoint 1.3");
            timeTracker.Records.Should().Contain(c => c.Name == "Checkpoint 1.4");

            timeTracker.Records.Should().Contain(r => r.Name == "Test");
            timeTracker.Records.Should().Contain(r => r.Name == "Inner Test");
            timeTracker.Records.Should().Contain(r => r.Name == "Inner Inner Test");
            timeTracker.Records.Should().Contain(r => r.Name == "Inner Test 2");

        }
    }
}

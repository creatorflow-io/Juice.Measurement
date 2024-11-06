using Juice.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using Xunit;
using Xunit.Abstractions;
using Juice.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Juice.Services;

namespace Juice.Measurement.Test
{
    [TestCaseOrderer("Juice.XUnit.PriorityOrderer", "Juice.XUnit")]
    public class GrpcStoreTest
    {
        private readonly ITestOutputHelper _output;

        public GrpcStoreTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [IgnoreOnCIFact(DisplayName = "Persist gRPC store")]
        public  async Task Measured_should_persist_ef_Async()
        {
            var resolver = new DependencyResolver
            {
                CurrentDirectory = AppContext.BaseDirectory
            };

            resolver.ConfigureServices(services =>
            {
                var configService = services.BuildServiceProvider().GetRequiredService<IConfigurationService>();
                var configuration = configService.GetConfiguration(GetType().Assembly);

                // Register DbContext class

                services.AddDefaultStringIdGenerator();

                services.AddSingleton(provider => _output);

                services.AddLogging(builder =>
                {
                    builder.ClearProviders()
                    //.AddTestOutputLogger() // does not work with Parallel
                    .AddConfiguration(configuration.GetSection("Logging"));
                });

                services.AddMeasurementGrpcStores(options =>
                {
                    options.Address = new Uri("https://localhost:7289/");
                });

                services.AddExecutionTimeMeasurement();
            });

            //await StoreTestHelper.TestAsync(resolver.ServiceProvider, "gRPC", _output);
            await Task.CompletedTask;
        }

    }
}

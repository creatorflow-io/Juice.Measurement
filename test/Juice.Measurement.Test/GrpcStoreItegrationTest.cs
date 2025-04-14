
using Juice.Extensions.DependencyInjection;
using Juice.Services;
using Juice.XUnit;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Juice.Measurement.Test.Helpers;

namespace Juice.Measurement.Test
{
    public class GrpcStoreItegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper output)
                : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly ITestOutputHelper _output = output;

        [IgnoreOnCIFact(DisplayName = "Persist gRPC store")]
        public async Task Measured_should_persist_grpc_Async()
        {
            var resolver = new DependencyResolver
            {
                CurrentDirectory = AppContext.BaseDirectory
            };
            var client = factory.CreateClient();

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
                    options.Address = factory.Server.BaseAddress;
                    options.ChannelOptionsActions.Add(channelOptions =>
                    {
                        channelOptions.HttpHandler = factory.Server.CreateHandler();
                    });
                });

                services.AddExecutionTimeMeasurement();

            });

            _output.WriteLine(client.BaseAddress?.ToString() ?? "");

            var health = await client.GetStringAsync("/health");
            health.Should().Be("Healthy");
            _output.WriteLine(health);

            await StoreTestHelper.TestAsync(resolver.ServiceProvider, "gRPC", _output);
        }
    }
}

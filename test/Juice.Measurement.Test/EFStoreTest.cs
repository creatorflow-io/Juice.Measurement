using Juice.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using Xunit;
using Xunit.Abstractions;
using Juice.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Juice.Measurement.Stores.EF;
using Juice.EF.Extensions;
using Juice.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Juice.Measurement.Test.Helpers;
using Juice.Measurement.Stores;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;

namespace Juice.Measurement.Test
{
    [TestCaseOrderer("Juice.XUnit.PriorityOrderer", "Juice.XUnit")]
    public class EFStoreTest
    {
        private readonly ITestOutputHelper _output;

        public EFStoreTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [IgnoreOnCITheory(DisplayName = "Migrations"), TestPriority(999)]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        public async Task DbContext_should_migrate_Async(string provider)
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

                services.AddSingleton(provider => _output);

                services.AddLogging(builder =>
                {
                    builder.ClearProviders()
                    .AddTestOutputLogger()
                    .AddConfiguration(configuration.GetSection("Logging"));
                });

                services.AddMeasurementEFStores(configuration, options =>
                {
                    options.DatabaseProvider = provider;
                });

            });

            var context = resolver.ServiceProvider.
                CreateScope().ServiceProvider.GetRequiredService<MeasurementDbContext>();

            await context.MigrateAsync();

            _output.WriteLine("Migration completed");
        }

        [IgnoreOnCITheory(DisplayName = "Persist EF store")]
        [InlineData("SqlServer")]
        [InlineData("PostgreSQL")]
        public async Task Measured_should_persist_ef_Async(string provider)
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

                services.AddMeasurementEFStores(configuration, options =>
                {
                    options.DatabaseProvider = provider;
                });

                services.AddExecutionTimeMeasurement();
            });

            await StoreTestHelper.TestAsync(resolver.ServiceProvider, "EF", _output);

        }

    }
}

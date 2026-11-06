using System;
using System.Threading.Tasks;
using FluentAssertions;
using Juice.Measurement.Stores.EF;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Juice.Measurement.Test
{
    /// <summary>A context going back to the pool must not take its open transaction to the next user.</summary>
    public sealed class PooledContextResetTest : IDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private readonly ServiceProvider _services;
        private readonly IDbContextFactory<MeasurementDbContext> _factory;

        public PooledContextResetTest()
        {
            _connection.Open();
            _services = new ServiceCollection()
                .AddPooledDbContextFactory<MeasurementDbContext>(o => o.UseSqlite(_connection))
                .BuildServiceProvider();
            _factory = _services.GetRequiredService<IDbContextFactory<MeasurementDbContext>>();
        }

        public void Dispose()
        {
            _services.Dispose();
            _connection.Dispose();
        }

        [Fact]
        public async Task An_async_return_ends_the_open_transactionAsync()
        {
            var first = _factory.CreateDbContext();
            await first.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
            await first.DisposeAsync();

            await using var second = _factory.CreateDbContext();

            second.Should().BeSameAs(first, "the pool hands the same context back");
            second.Database.CurrentTransaction.Should().BeNull();
        }

        [Fact]
        public void A_sync_return_ends_the_open_transaction()
        {
            var first = _factory.CreateDbContext();
            first.Database.BeginTransaction();
            first.Dispose();

            using var second = _factory.CreateDbContext();

            second.Should().BeSameAs(first, "the pool hands the same context back");
            second.Database.CurrentTransaction.Should().BeNull();
        }
    }
}

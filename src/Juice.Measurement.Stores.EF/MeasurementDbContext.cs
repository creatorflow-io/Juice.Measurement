using Juice.EF;
using Juice.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Juice.Measurement.Stores.EF
{
    public class MeasurementDbContext(DbContextOptions<MeasurementDbContext> options) : DbContext(options), ISchemaDbContext
    {
        public DbSet<TimeRecord> TimeRecords { get; set; }
        public DbSet<TimeSummary> TimeSummaries { get; set; }

        public string? Schema { get; private set; }

        public void SetSchema(string? schema)
        {
            Schema = schema;
        }

        private ITimeTracker? _tracker;

        public void SetTimeTracker(ITimeTracker? tracker)
        {
            _tracker = tracker;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            using var _ = _tracker?.BeginScope("OnModelCreating", "timetracker.stores.ef.onModelCreating");
            modelBuilder.Entity<TimeRecord>(entity =>
            {
                entity.Property<Guid>("Id");
                entity.HasKey("Id");
                entity.Property(e => e.Name).HasMaxLength(LengthConstants.NameLength).IsRequired();
                entity.Property(e => e.FullName).HasMaxLength(LengthConstants.ShortDescriptionLength).IsRequired();
                entity.Property(e => e.StartedTime).IsRequired();
                entity.Property(e => e.ElapsedTime).IsRequired();
                entity.Property(e => e.ScopeId).HasMaxLength(LengthConstants.IdentityLength).IsRequired();
                entity.Property(e => e.TraceId).HasMaxLength(LengthConstants.IdentityLength).IsRequired();

                entity.HasIndex(e => e.TraceId);
                entity.HasIndex(e => e.ScopeId);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.RecordedDate);

                entity.ToTable("TimeRecords", Schema);
            });

            modelBuilder.Entity<TimeSummary>(entity =>
            {
                entity.HasKey(e => e.TraceId);
                entity.Property(e => e.Name).HasMaxLength(LengthConstants.NameLength).IsRequired();
                entity.Property(e => e.RootScopeId).HasMaxLength(LengthConstants.IdentityLength).IsRequired();

                entity.HasIndex(e => e.RootScopeId);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.RecordedDate);

                entity.ToTable("TimeSummaries", Schema);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            using var _ = _tracker?.BeginScope("SaveChanges", "timetracker.stores.ef.saveChanges");
            return await base.SaveChangesAsync(cancellationToken);
        }
    }

    internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MeasurementDbContext>
    {
        public MeasurementDbContext CreateDbContext(string[] args)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            var resolver = new DependencyResolver
            {
                CurrentDirectory = AppContext.BaseDirectory
            };
            resolver.ConfigureServices(services =>
            {
                // Register DbContext class
                var configService = services.BuildServiceProvider().GetRequiredService<IConfigurationService>();

                var configuration = configService.GetConfiguration(args);

                var provider = configuration.GetSection("Provider").Get<string>() ?? "SqlServer";

                var connectionName =
                    provider switch
                    {
                        "PostgreSQL" => "PostgreConnection",
                        "SqlServer" => "SqlServerConnection",
                        _ => throw new NotSupportedException($"Unsupported provider: {provider}")
                    };

                var connectionString = configuration.GetConnectionString(connectionName);

                services.AddDbContext<MeasurementDbContext>(options =>
                {
                    switch (provider)
                    {
                        case "PostgreSQL":
                            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

                            options.UseNpgsql(
                                connectionString,
                                x =>
                                {
                                    x.MigrationsAssembly("Juice.Measurement.Stores.EF.PostgreSQL");
                                });
                            break;

                        case "SqlServer":

                            options.UseSqlServer(
                                connectionString,
                                x =>
                                {
                                    x.MigrationsAssembly("Juice.Measurement.Stores.EF.SqlServer");
                                });
                            break;
                        default:
                            throw new NotSupportedException($"Unsupported provider: {provider}");
                    }
                });
            });

            var context = resolver.ServiceProvider.GetRequiredService<MeasurementDbContext>();
            context.SetSchema("Measurement");
            return context;
        }
    }

    internal class PooledDbContextFactory : IDbContextFactory<MeasurementDbContext>
    {
        private readonly IDbContextFactory<MeasurementDbContext> _factory;
        private readonly ITimeTracker? _tracker;
        private DbOptions<MeasurementDbContext>? _options;

        public PooledDbContextFactory(IDbContextFactory<MeasurementDbContext> factory,
            DbOptions<MeasurementDbContext>? options, ITimeTracker? tracker = default)
        {
            _factory = factory;
            _options = options;
            _tracker = tracker;
        }

        public MeasurementDbContext CreateDbContext()
        {
            using var _ = _tracker?.BeginScope("Create DbContext", "timetracker.stores.ef.pooledFactory");
            var context = _factory.CreateDbContext();
            context.SetSchema(_options?.Schema);
            context.SetTimeTracker(_tracker);
            return context;
        }
    }

}

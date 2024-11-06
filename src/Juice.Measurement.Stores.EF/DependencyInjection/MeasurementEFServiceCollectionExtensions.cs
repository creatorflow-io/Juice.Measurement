using Juice.EF.Migrations;
using Juice.EF;
using Juice.Measurement.Stores.EF;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Juice.Measurement.Stores;
namespace Microsoft.Extensions.DependencyInjection
{
    public static class MeasurementEFServiceCollectionExtensions
    {
        private static IServiceCollection AddMeasurementDbContext(this IServiceCollection services, IConfiguration configuration,
            Action<DbOptions>? configureOptions)
        {
            services.AddScoped(p =>
            {
                var options = new DbOptions<MeasurementDbContext> { DatabaseProvider = "SqlServer", Schema = "Measurement" };
                configureOptions?.Invoke(options);
                return options;
            });

            var dbOptions = services.BuildServiceProvider()
                .GetRequiredService<DbOptions<MeasurementDbContext>>();
            var provider = dbOptions.DatabaseProvider;
            var schema = dbOptions.Schema;
            var connectionName = dbOptions.ConnectionName ??
                provider switch
                {
                    "PostgreSQL" => "PostgreConnection",
                    "SqlServer" => "SqlServerConnection",
                    _ => throw new NotSupportedException($"Unsupported provider: {provider}")
                };

            services.AddPooledDbContextFactory<MeasurementDbContext>(options =>
            {
                switch (provider)
                {
                    case "PostgreSQL":
                        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                        options.UseNpgsql(
                           configuration.GetConnectionString(connectionName),
                            x =>
                            {
                                x.MigrationsHistoryTable("__EFMeasurementMigrationsHistory", schema);
                                x.MigrationsAssembly("Juice.Measurement.Stores.EF.PostgreSQL");
                            });
                        break;

                    case "SqlServer":
                        options.UseSqlServer(
                            configuration.GetConnectionString(connectionName),
                        x =>
                        {
                            x.MigrationsHistoryTable("__EFMeasurementMigrationsHistory", schema);
                            x.MigrationsAssembly("Juice.Measurement.Stores.EF.SqlServer");
                        });
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported provider: {provider}");
                }
                options
                    .ReplaceService<IMigrationsAssembly, DbSchemaAwareMigrationAssembly>()
                ;
            });

            services.AddScoped<PooledDbContextFactory>();
            services.AddScoped(sp => sp.GetRequiredService<PooledDbContextFactory>().CreateDbContext());

            return services;
        }

        public static IServiceCollection AddMeasurementEFStores(this IServiceCollection services, IConfiguration configuration,
            Action<DbOptions>? configureOptions = default)
        {
            services.AddMeasurementDbContext(configuration, configureOptions);
            services.AddScoped<ITimeRepository, TimeMeasurementRepository>();
            return services;
        }
    }
}

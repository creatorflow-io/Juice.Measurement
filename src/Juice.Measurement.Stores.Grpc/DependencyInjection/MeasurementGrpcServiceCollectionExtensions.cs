
using Grpc.Net.Client;
using Grpc.Net.ClientFactory;
using Juice.Measurement.Api.Grpc;
using Juice.Measurement.Stores;
using Juice.Measurement.Stores.Grpc;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MeasurementGrpcServiceCollectionExtensions
    {
        public static IServiceCollection AddMeasurementGrpcStores(this IServiceCollection services,
            Action<GrpcClientFactoryOptions> configure)
        {
            services.AddGrpcClient<TimeStore.TimeStoreClient>(configure);
            services.AddScoped<ITimeRepository, TimeMeasurementRepository>();

            return services;
        }
        public static IServiceCollection AddMeasurementGrpcStores(this IServiceCollection services,
            GrpcChannel channel)
        {
            services.AddScoped(sp => new TimeStore.TimeStoreClient(channel));
            services.AddScoped<ITimeRepository, TimeMeasurementRepository>();

            return services;
        }
    }
}

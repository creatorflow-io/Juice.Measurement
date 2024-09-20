using Juice.Measurement;
using Juice.Measurement.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MeasurementServiceCollectionExtensions
    {
        public static IServiceCollection AddExecutionTimeMeasurement(this IServiceCollection services)
        {
            services.AddSingleton<ITimeTracker, TimeTracker>();
            return services;
        }
    }
}

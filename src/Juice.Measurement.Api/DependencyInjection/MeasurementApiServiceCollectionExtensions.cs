
using Juice.Measurement.Api.Grpc.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MeasurementApiServiceCollectionExtensions
    {
        /// <summary>
        /// Maps the Measurement gRPC service.
        /// </summary>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        public static IEndpointRouteBuilder MapMeasurementGrpcServices(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<TimeStoreService>();
            return endpoints;
        }
    }
}

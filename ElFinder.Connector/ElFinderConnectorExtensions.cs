using System;
using ElFinder.Connector;

namespace Microsoft.AspNetCore.Builder
{
    public static class ElFinderConnectorExtensions
    {
        public static IApplicationBuilder UseElFinderConnector(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<ElFinderConnectorMiddleware>();
        }
    }
}

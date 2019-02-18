﻿using System;
using ElFinder.Connector;
using ElFinder.Connector.Volume;
using ElFinder.Connector.Volume.Default;

namespace Microsoft.Extensions.DependencyInjection

{
    public static class ElFinderConnectorServiceExtensions
    {
        public static IServiceCollection AddElFinderConnector(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<Connector>();
            services.AddTransient<IPathHasher, DefaultPathHasher>();

            return services;
        }

        public static IServiceCollection AddElFinderConnector(this IServiceCollection services, Action<ElFinderConnectorOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.Configure(configureOptions);
            services.AddElFinderConnector();

            return services;
        }
    }
}

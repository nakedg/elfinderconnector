using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElFinder.Connector
{
    public class ElFinderConnectorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly ElFinderConnectorOptions _options;
        private readonly Connector _connector;

        public ElFinderConnectorMiddleware(RequestDelegate next, ILoggerFactory loggerFactory,  IOptions<ElFinderConnectorOptions> options, Connector connector)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ElFinderConnectorMiddleware>();
            _options = options.Value;
            _connector = connector;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(new PathString(_options.ConnectorUrl)))
            {
                await _connector.ProcessRequest(context);

            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}

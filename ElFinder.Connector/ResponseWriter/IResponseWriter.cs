using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElFinder.Connector.ResponseWriter
{
    public interface IResponseWriter
    {
        Task WriteAsync(HttpContext context);
    }
}

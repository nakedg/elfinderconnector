using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ElFinder.Connector.ResponseWriter
{
    public class StringResponseWriter : IResponseWriter
    {

        public async Task WriteAsync(HttpContext context)
        {
            var str = System.IO.File.ReadAllText("c:\\temp\\resp.json");
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(str);
        }
    }
}

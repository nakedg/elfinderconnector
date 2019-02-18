using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ElFinder.Connector.ResponseWriter
{
    public class JsonResponseWriter : IResponseWriter
    {

        public object Data { get; set; }

        public JsonResponseWriter()
        {

        }

        public JsonResponseWriter(object data)
        {
            Data = data;
        }

        public async Task WriteAsync(HttpContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(Data);

            await context.Response.WriteAsync(json);
        }
    }
}

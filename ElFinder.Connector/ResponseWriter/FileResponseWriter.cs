using System;
using System.Collections.Generic;
//using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace ElFinder.Connector.ResponseWriter
{
    public class FileResponseWriter : IResponseWriter
    {
        public System.IO.Stream Stream { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public FileResponseWriter(System.IO.Stream stream, string filename = null, string contentType = null)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));

            FileName = filename;

            ContentType = contentType;
        }

        public async Task WriteAsync(HttpContext context)
        {
            context.Response.StatusCode = 200;

            if (string.IsNullOrEmpty(ContentType))
            {
                var ext = string.IsNullOrEmpty(FileName) ? ".unknown" : System.IO.Path.GetExtension(FileName);
                ContentType = MimeTypeMap.GetMimeType(ext);
            }

            context.Response.ContentType = ContentType;

            if (Stream.CanSeek)
            {
                Stream.Seek(0, System.IO.SeekOrigin.Begin);
                context.Response.ContentLength = Stream.Length;
            }

            if (!string.IsNullOrEmpty(FileName))
            {
                var contentDisposition = new ContentDispositionHeaderValue("attachment");
                contentDisposition.SetHttpFileName(FileName);
                context.Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();
            }

            

            var outputStream = context.Response.Body;

            using (Stream)
            {
                await Stream.CopyToAsync(outputStream);
            }

        }
    }
}

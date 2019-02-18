using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using ElFinder.Connector.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace ElFinder.Connector
{
    public class Connector
    {

        private readonly ElFinder _elFinder = new ElFinder();

        public async Task ProcessRequest(HttpContext context)
        {
            var request = context.Request;

            IFormFileCollection files = null;

            NameValueCollection values = request.Query.AsNameValueCollection();

            if (HttpMethods.IsPost(request.Method) && request.HasFormContentType)
            {
                files = request.Form.Files;
                values.Add(request.Form.AsNameValueCollection());
            }

            var cmdName = values.Get("cmd");

            if (string.IsNullOrEmpty(cmdName))
            {
                throw new InvalidOperationException("Command not found or command empty");
            }
            /*Dictionary<string, string> parametrs = new Dictionary<string, string>(values.Count - 1);

            foreach (string key in values)
            {
                if (key != "cmd")
                {
                    parametrs.Add(key, values[key]);
                }
            }*/


            var cmd = Command.CommandFactory.CreateCommand(cmdName, values, _elFinder, files);
            var result = cmd.Execute();
            await result.WriteAsync(context);
        }
    }
}

﻿using Microsoft.AspNetCore.Http;
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
        private readonly IElFinderFactory _elFinderFactory;

        public Connector(IElFinderFactory elFinderFactory)
        {
            _elFinderFactory = elFinderFactory;
        }
        

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

            var elFinder = _elFinderFactory.Create();
            var cmd = Command.CommandFactory.CreateCommand(cmdName, values, elFinder, files);
            var result = await cmd.Execute();
            await result.WriteAsync(context);
        }
    }
}

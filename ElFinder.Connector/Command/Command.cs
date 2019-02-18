using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using ElFinder.Connector.ResponseWriter;
using System.Linq;

namespace ElFinder.Connector.Command
{
    public abstract class Command : ICommand
    {

        protected readonly NameValueCollection CmdParams;

        public ElFinder ElFinder { get; private set; }

        public string Name { get; protected set; }

        public Command(NameValueCollection cmdParams, ElFinder elFinder)
        {
            CmdParams = cmdParams;
            ElFinder = elFinder;
            InitCommand();
        }

        protected abstract void InitCommand();

        public abstract IResponseWriter Execute();

        protected bool ParseBoolParameter(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (int.TryParse(value, out int n))
            {
                return n > 0;
            }
            else
            {
                return false;
            }
        }

        protected Dictionary<string, string> ParseDict(string paramName, NameValueCollection cmdParams)
        {
            var keys = cmdParams.AllKeys.Where(k => k.StartsWith(paramName)).ToArray();

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var item in keys)
            {
                var keyValue = ParseKeyValue(item);
                result.Add(keyValue, cmdParams.Get(item));
            }

            return result;
        }

        private string ParseKeyValue(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            int start = key.IndexOf('[') + 1;
            int end = key.IndexOf(']');
            return key.Substring(start, end - start);
        }
    }
}

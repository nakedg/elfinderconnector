using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace ElFinder.Connector.Extensions
{
    public static class StringCollectionExtensions
    {
        public static NameValueCollection AsNameValueCollection(this IEnumerable<KeyValuePair<string, StringValues>> collection)
        {
            var nv = new NameValueCollection();

            foreach (var item in collection)
            {
                if (item.Key.EndsWith("[]"))
                {
                    foreach (var v in item.Value)
                    {
                        nv.Add(item.Key, v);
                    }
                }
                else
                {
                    nv.Add(item.Key, item.Value.First());
                }
            }

            return nv;
        }

        public static NameValueCollection AsNameValueCollection(this IDictionary<string, StringValues> collection)
        {
            var nv = new NameValueCollection();

            foreach (var item in collection)
            {
                nv.Add(item.Key, item.Value.First());
            }

            return nv;
        }
    }
}

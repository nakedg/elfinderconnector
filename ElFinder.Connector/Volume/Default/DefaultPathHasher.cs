using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector.Volume.Default
{
    public class DefaultPathHasher : IPathHasher
    {
        public string Decode(string hash)
        {
            return Encoding.UTF8.GetString(Microsoft.AspNetCore.WebUtilities.Base64UrlTextEncoder.Decode(hash));
        }

        public string Encode(string path)
        {
            return Microsoft.AspNetCore.WebUtilities.Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(path));
        }
    }
}

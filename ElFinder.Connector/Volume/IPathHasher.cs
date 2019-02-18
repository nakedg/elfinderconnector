using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector.Volume
{
    public interface IPathHasher
    {
        string Encode(string path);
        string Decode(string hash);
    }
}

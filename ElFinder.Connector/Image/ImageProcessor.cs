using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector.Image
{
    public interface IImageProcessor
    {
        IImg Load(System.IO.Stream stream);
    }
}

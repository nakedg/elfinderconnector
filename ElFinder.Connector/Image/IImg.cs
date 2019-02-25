using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector.Image
{
    public interface IImg: IDisposable
    {
        string GetDeminisation();
        System.IO.Stream Resize(int width, int height);
    }
}

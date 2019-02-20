using ElFinder.Connector.Volume;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector
{
    public class DefaultElFinderFactoryOptions
    {
        public IVolume<IVolumeDriver>[] Volumes { get; set; }
    }
}

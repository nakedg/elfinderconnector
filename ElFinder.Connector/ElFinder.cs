using System;
using System.Collections.Generic;
using System.Text;
using ElFinder.Connector.Volume;
using System.Linq;
using ElFinder.Connector.Models;

namespace ElFinder.Connector
{
    public class ElFinder
    {
        public ElFinder()
        {
            Volumes = new IVolume<IVolumeDriver>[] {
                new Volume<FsVolumeDriver>(new FsVolumeDriver("c:\\temp"))
            };
        }

        public IVolume<IVolumeDriver>[] Volumes { get; private set; }

        public IVolume<IVolumeDriver> Default
        {
            get
            {
                if (Volumes != null)
                {
                    return Volumes.FirstOrDefault(v => v.CanRead);
                }

                return null;
            }
        }

        public IVolume<IVolumeDriver> GetVolume(string hash)
        {
            if (string.IsNullOrEmpty(hash))
            {
                return null;
            }
            return Volumes.FirstOrDefault(v => hash.StartsWith(v.VolumeId));
        }
    }
}

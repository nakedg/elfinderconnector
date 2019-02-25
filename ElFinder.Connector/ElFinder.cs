using System;
using System.Collections.Generic;
using System.Text;
using ElFinder.Connector.Volume;
using System.Linq;
using ElFinder.Connector.Models;
using ElFinder.Connector.Image;

namespace ElFinder.Connector
{
    public class ElFinder
    {
        public ElFinder()
        {
            /*Volumes = new IVolume<IVolumeDriver>[] {
                new Volume<FsVolumeDriver>(new FsVolumeDriver("c:\\temp"))
            };*/
        }

        private Volume.Volume[] _volumes;
        public Volume.Volume[] Volumes
        {
            get { return _volumes; }
            set {
                foreach (var item in value)
                {
                    item.ElFinder = this;
                }
                _volumes = value;
            }
        }

        public Volume.Volume Default
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

        public Volume.Volume GetVolume(string hash)
        {
            if (string.IsNullOrEmpty(hash))
            {
                return null;
            }
            return Volumes.FirstOrDefault(v => hash.StartsWith(v.VolumeId));
        }

        public IImageProcessor ImageProcessor { get; set; }
    }
}

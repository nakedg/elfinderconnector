using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector.Models
{
    public class DirectoryEl
    {
        public bool IsOwner { get; set; }
        public long Timestamp { get; set; }
        public string Mime { get; set; }
        public byte Read { get; set; }
        public byte Write { get; set; }
        public int Size { get; set; }
        public string Hash { get; set; }
        public string Name { get; set; }
        public string ParentHash { get; set; }
        public string VolumeId { get; set; }
    }
}

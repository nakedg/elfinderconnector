using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector.Models
{
    public class ElFinderFsEntry
    {
        public string Name { get; set; }

        public string Hash { get; set; }

        public string PHash { get; set; }

        public string Mime { get; set; }

        /// <summary>
        /// file modification time in unix timestamp
        /// </summary>
        public int Ts { get; set; }

        public string Date { get; set; }

        /// <summary>
        /// file size in bytes
        /// </summary>
        public int Size { get; set; }

        public bool Dirs { get; set; }

        public bool Read { get; set; }

        public bool Write { get; set; }

        public bool Locked { get; set; }

        public string Tmb { get; set; }

        public string Alias { get; set; }

        public string THash { get; set; }

        public string Dim { get; set; }

        public bool IsOwner { get; set; }

        public string Csscls { get; set; }

        public string VolumeId { get; set; }

        public string NetKey { get; set; }

        public Dictionary<string, string> Options { get; set; }
    }
}

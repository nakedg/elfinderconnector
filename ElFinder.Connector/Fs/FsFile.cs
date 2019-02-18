using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector.Fs
{
    public class FsFile: FsBase
    {
        /// <summary>
        /// file size in bytes
        /// </summary>
        [JsonProperty("size")]
        public long Size { get; set; }
    }

    public class FsImageFile : FsFile
    {
        /// <summary>
        /// Only for images. Thumbnail file name, if file do not have thumbnail yet, but it can be generated than it must have value "1"
        /// </summary>
        [JsonProperty("tmb")]
        public string Tmb { get; set; }

        /// <summary>
        /// For images - file dimensions. Optionally.
        /// </summary>
        [JsonProperty("dim")]
        public string Deminisation { get; set; }
    }
}

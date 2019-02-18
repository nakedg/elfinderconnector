using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ElFinder.Connector.Fs
{
    public abstract class FsBase
    {
        /// <summary>
        /// name of file/dir. Required
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// hash of current file/dir path, first symbol must be letter, 
        /// symbols before _underline_ - volume id, Required.
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// hash of parent directory. Required except roots dirs.
        /// </summary>
        [JsonProperty("phash")]
        public string ParentHash { get; set; }

        /// <summary>
        /// mime type. Required.
        /// </summary>
        [JsonProperty("mime")]
        public string Mime { get; set; }

        /// <summary>
        /// file modification time in unix timestamp. Required.
        /// </summary>
        [JsonProperty("ts")]
        public long Timestamp { get; set; }

        /// <summary>
        /// is readable
        /// </summary>
        [JsonProperty("read")]
        public int Read { get; set; }

        /// <summary>
        /// is writable
        /// </summary>
        [JsonProperty("write")]
        public int Write { get; set; }

        /// <summary>
        /// is file locked. If locked that object cannot be deleted,  renamed or moved
        /// </summary>
        [JsonProperty("locked")]
        public int Locked { get; set; }
    }
}

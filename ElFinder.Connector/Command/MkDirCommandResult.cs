using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector.Command
{
    public class MkDirCommandResult
    {
        [JsonProperty("added")]
        public Fs.FsDirectory[] Added { get; set; }

        [JsonProperty("changed")]
        public Fs.FsDirectory[] Changed { get; set; }

        [JsonProperty("hashes")]
        public Dictionary<string, string> Hashes { get; set; }
    }
}

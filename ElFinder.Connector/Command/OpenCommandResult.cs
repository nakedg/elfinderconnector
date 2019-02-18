using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ElFinder.Connector.Command
{
    public class OpenCommandResult
    {
        [JsonProperty("api")]
        public float? Api { get; set; }

        [JsonProperty("cwd")]
        public Fs.FsBase Cwd { get; set; }

        [JsonProperty("files")]
        public Fs.FsBase[] Files { get; set; }

        [JsonProperty("uplMaxFile ")]
        public int UplMaxFile { get; set; }

        [JsonProperty("uplMaxSize")]
        public string UplMaxSize { get; set; }

        [JsonProperty("options")]
        public Fs.RootOptions Options { get; set; } = new Fs.RootOptions();
    }
}

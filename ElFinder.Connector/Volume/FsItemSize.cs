using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector.Volume
{
    public struct FsItemSize
    {
        [JsonProperty("dirCnt")]
        public int DirectoryCount { get; set; }

        [JsonProperty("fileCnt")]
        public int FileCount { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }
    }
}

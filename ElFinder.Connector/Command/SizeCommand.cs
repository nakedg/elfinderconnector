using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using ElFinder.Connector.ResponseWriter;
using ElFinder.Connector.Volume;

namespace ElFinder.Connector.Command
{
    public class SizeCommand: Command
    {

        public Dictionary<string, string> Targets { get; set; }

        public SizeCommand(NameValueCollection cmdParams, ElFinder elFinder)
            :base(cmdParams, elFinder)
        {

        }

        protected override void InitCommand()
        {
            Targets = ParseDict("targets", CmdParams);
        }

        public override IResponseWriter Execute()
        {
            Dictionary<string, FsItemSize> result = new Dictionary<string, FsItemSize>();

            int TotalDirectoryCount = 0;
            int TotalFileCount = 0;
            long TotalSize = 0;

            foreach (var item in Targets.Values)
            {
                var volume = ElFinder.GetVolume(item);
                var size = volume.GetSize(item);
                result.Add(item, size);
                TotalDirectoryCount += size.DirectoryCount;
                TotalFileCount += size.FileCount;
                TotalSize += size.Size;
            }

            return new JsonResponseWriter(new
            {
                dirCnt = TotalDirectoryCount,
                fileCnt = TotalFileCount,
                size = TotalSize,
                sizes = result
            });
        }
    }
}

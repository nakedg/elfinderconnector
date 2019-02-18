using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using ElFinder.Connector.ResponseWriter;

namespace ElFinder.Connector.Command
{
    public class FileCommand: Command
    {
        public string Target { get; set; }

        public bool IsDownload { get; set; }

        public string CookiePath { get; set; }

        public FileCommand(NameValueCollection cmdParams, ElFinder elFinder)
            :base(cmdParams, elFinder)
        {

        }

        protected override void InitCommand()
        {
            Target = CmdParams.Get("target");
            IsDownload = ParseBoolParameter(CmdParams.Get("download"));
            CookiePath = CmdParams.Get("cpath");
        }

        public override IResponseWriter Execute()
        {
            var volume = ElFinder.GetVolume(Target);

            var (stream, filename) = volume.GetFile(Target);

            return new FileResponseWriter(stream, filename);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using ElFinder.Connector.ResponseWriter;

namespace ElFinder.Connector.Command
{
    public class TmbCommand: Command
    {

        public string[] Targets { get; set; }

        public TmbCommand(NameValueCollection cmdParams, ElFinder elFinder)
            :base(cmdParams, elFinder)
        {

        }

        protected override void InitCommand()
        {
            Targets = CmdParams.GetValues("targets[]");
        }

        public override IResponseWriter Execute()
        {
            Dictionary<string, string> images = new Dictionary<string, string>();

            foreach (var target in Targets)
            {
                var volume = ElFinder.GetVolume(target);
                var (stream, filename) = volume.GetFile(target);
                using (stream)
                {
                    var tmbName = volume.CreateTmb(stream, filename);
                    images.Add(target, tmbName);
                }
            }

            return new JsonResponseWriter(new { images });
        }
    }
}

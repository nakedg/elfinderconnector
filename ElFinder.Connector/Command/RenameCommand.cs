using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using ElFinder.Connector.ResponseWriter;

namespace ElFinder.Connector.Command
{
    public class RenameCommand: Command
    {

        public string Target { get; set; }

        public string NewName { get; set; }

        public RenameCommand(NameValueCollection cmdParams, ElFinder elFinder)
            :base(cmdParams, elFinder)
        {
            
        }

        protected override void InitCommand()
        {
            Target = CmdParams.Get("target");
            NewName = CmdParams.Get("name");
        }

        public override IResponseWriter Execute()
        {
            var volume = ElFinder.GetVolume(Target);

            var added = volume.Rename(Target, NewName);

            return new JsonResponseWriter(new { added = new Fs.FsBase[] { added }, removed = new string[] { Target } });
        }
    }
}

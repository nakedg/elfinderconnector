using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using ElFinder.Connector.ResponseWriter;

namespace ElFinder.Connector.Command
{
    public class TreeCommand : Command
    {
        public string Target { get; set; }

        public TreeCommand(NameValueCollection cmdParams, ElFinder elFinder)
            :base(cmdParams, elFinder)
        {

        }

        protected override void InitCommand()
        {
            Target = CmdParams.Get("target");
        }

        public override IResponseWriter Execute()
        {
            var volume = ElFinder.GetVolume(Target);
             
            var items = volume.GetDirectoryItems(Target);

            return new JsonResponseWriter(new { tree = items.OfType<Fs.FsDirectory>().ToArray() });
        }

        
    }
}

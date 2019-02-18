using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using ElFinder.Connector.ResponseWriter;

namespace ElFinder.Connector.Command
{
    public class ParentsCommand : Command
    {

        public string Target { get; set; }

        public string Until { get; set; }

        public ParentsCommand(NameValueCollection cmdParams, ElFinder elFinder)
            : base(cmdParams, elFinder)
        {

        }

        protected override void InitCommand()
        {
            Target = CmdParams.Get("target");
            Until = CmdParams.Get("until");
        }

        public override IResponseWriter Execute()
        {
            var volume = ElFinder.GetVolume(Target);

            var parents = volume.GetParents(Target, Until);

            return new JsonResponseWriter(new { tree = parents });

        }

    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using ElFinder.Connector.ResponseWriter;
using System.Linq;

namespace ElFinder.Connector.Command
{
    public class LsCommand: Command
    {
        public string Target { get; set; }

        public string[] Intersect { get; set; }

        public LsCommand(NameValueCollection cmdParams, ElFinder elFinder)
            :base(cmdParams, elFinder)
        {

        }

        protected override void InitCommand()
        {
            Target = CmdParams.Get("target");
            Intersect = CmdParams.GetValues("intersect[]") ?? new string[0];
        }

        public override IResponseWriter Execute()
        {
            var volume = ElFinder.GetVolume(Target);
            var items = volume.GetDirectoryItems(Target);

            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var item in items)
            {
                if (Intersect.Length == 0 || Intersect.Contains(item.Name))
                {
                    result.Add(item.Hash, item.Name);
                }
            }

            return new JsonResponseWriter(new { list = result });
        }
    }
}

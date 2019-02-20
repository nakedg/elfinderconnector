using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using ElFinder.Connector.ResponseWriter;

namespace ElFinder.Connector.Command
{
    public class RmCommand: Command
    {
        public string[] Targets { get; set; }

        public RmCommand(NameValueCollection cmdParams, ElFinder elFinder)
            :base(cmdParams, elFinder)
        {
            
        }

        protected override void InitCommand()
        {
            Targets = CmdParams.GetValues("targets[]");
        }

        public override IResponseWriter Execute()
        {
            List<string> successfullyRemoved = new List<string>();

            foreach (var target in Targets)
            {
                var volume = ElFinder.GetVolume(target);
                try
                {
                    volume.Delete(target);
                    successfullyRemoved.Add(target);
                }
                catch (Exception e)
                {
                    //log
                }

            }

            return new JsonResponseWriter(new { removed = successfullyRemoved });
        }
    }
}

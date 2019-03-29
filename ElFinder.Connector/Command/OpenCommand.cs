using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElFinder.Connector.Models;
using ElFinder.Connector.Volume;

namespace ElFinder.Connector.Command
{
    public class OpenCommand : Command
    {
        public bool Init { get; set; }

        public string Target { get; set; }

        public bool Tree { get; set; }

        public OpenCommand(NameValueCollection cmdParams, ElFinder elFinder)
            :base(cmdParams, elFinder)
        {
            Name = "open";
        }

        protected override void InitCommand()
        {
            Init = ParseBoolParameter(CmdParams.Get("init"));
            Target = CmdParams.Get("target");
            Tree = ParseBoolParameter(CmdParams.Get("tree"));

            if (!Init && string.IsNullOrEmpty(Target))
            {
                throw new InvalidOperationException("Terget required where not init");
            }
        }

        public override async Task<ResponseWriter.IResponseWriter> Execute()
        {
            OpenCommandResult result = new OpenCommandResult();

            var volume = ElFinder.GetVolume(Target);

            Fs.FsBase cwd = null;
            if (volume != null)
            {
                cwd = await volume.GetCurrentWorkingDirectory(Target);
            }
            
            string hash = Init ? "default folder" : "#" + Target;

            if ((cwd == null || !(cwd.Read == 1)) && Init)
            {
                volume = ElFinder.Default;
                Target = volume.DefaultPath;
                cwd = await volume.GetCurrentWorkingDirectory(Target);
            }

            if (Init)
            {
                result.Api = 2.1046f;
            }

            List<Fs.FsBase> files = new List<Fs.FsBase>();

            result.Cwd = cwd;
            files.AddRange(await volume.GetDirectoryItems(Target));
            result.UplMaxSize = "1M";
            result.UplMaxFile = 3;

            if (Tree)
            {
                List<Fs.FsBase> roots = new List<Fs.FsBase>();
                foreach (var r in ElFinder.Volumes)
                {
                    if (files.FirstOrDefault(f => f.Hash == r.Root.Hash) == null)
                    {
                        files.Add(r.Root);
                    }
                }
            }

            result.Files = files.ToArray();

            result.Options.UploadMime = new Fs.UploadMimeOptions
            {
                Allow = new string[] { "image/x-ms-bmp", "image/gif", "image/jpeg", "image/png", "image/x-icon", "text/plain" },
                Deny = new string[] { "all" },
                FirstOrder = "deny"
            };

            var writer = new ResponseWriter.JsonResponseWriter(result);
            return writer;
        }

    }
}

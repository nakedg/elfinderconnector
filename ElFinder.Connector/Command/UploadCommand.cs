using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using ElFinder.Connector.ResponseWriter;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ElFinder.Connector.Command
{
    public class UploadCommand: Command
    {
        public IFormFileCollection Files { get; set; }
        public string Target { get; set; }
        public string[] UploadPaths { get; set; }
        public string[] MTimes { get; set; }
        public string[] Names { get; set; }
        public string[] Renames { get; set; }
        public string Suffix { get; set; }
        public Dictionary<string, string> Hashes { get; set; }
        public bool Overwrite { get; set; }

        public UploadCommand(NameValueCollection cmdParams, ElFinder elFinder, IFormFileCollection files)
            :base(cmdParams, elFinder)
        {
            Files = files;
        }

        protected override void InitCommand()
        {
            Target = CmdParams.Get("target");
            UploadPaths = CmdParams.GetValues("upload_path[]");
            MTimes = CmdParams.GetValues("mtime[]");
            Names = CmdParams.GetValues("name[]");
            Renames = CmdParams.GetValues("renames[]");
            Suffix = CmdParams.Get("suffix");
            Hashes = ParseDict("hashes", CmdParams);
            Overwrite = ParseBoolParameter(CmdParams.Get("overwrite"));
        }

        public override IResponseWriter Execute()
        {
            var volume = ElFinder.GetVolume(Target);

            List<Fs.FsBase> uploadedFiles = new List<Fs.FsBase>();

            for (int i = 0; i < Files.Count; i++)
            {
                var file = Files[i];
                var hashPath = UploadPaths.Length > i ? UploadPaths[i] : Target;
                System.IO.MemoryStream ms = new System.IO.MemoryStream((int)file.Length);
                file.CopyTo(ms);
                uploadedFiles.Add(volume.Upload(hashPath, file.FileName, ms));
            }

            return new JsonResponseWriter(new { added = uploadedFiles.ToArray() });
        }
    }
}

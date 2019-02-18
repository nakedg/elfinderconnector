using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using ElFinder.Connector.ResponseWriter;

namespace ElFinder.Connector.Command
{
    public class MkDirCommand : Command
    {

        public string Target { get; set; }

        public string DirectoryName { get; set; }

        public string[] Dirs { get; set; }

        public MkDirCommand(NameValueCollection cmdParams, ElFinder elFinder)
            :base(cmdParams, elFinder)
        {

        }

        protected override void InitCommand()
        {
            Target = CmdParams.Get("target");
            DirectoryName = CmdParams.Get("name");
            Dirs = CmdParams.GetValues("dirs[]");

        }

        public override IResponseWriter Execute()
        {
            var result = new MkDirCommandResult();

            var volume = ElFinder.GetVolume(Target);

            if (volume != null)
            {
                if (!string.IsNullOrEmpty(DirectoryName))
                {
                    var dir = volume.CreateDirectory(Target, DirectoryName);
                    result.Added = new Fs.FsDirectory[] { dir };
                }
                else
                {
                    result.Hashes = new Dictionary<string, string>();
                    List<Fs.FsDirectory> dirs = new List<Fs.FsDirectory>();
                    foreach (var item in Dirs)
                    {
                        var d = volume.CreateDirectory(Target, item);
                        dirs.Add(d);
                        result.Hashes.Add(item, d.Hash);
                    }
                    result.Added = dirs.ToArray();
                }

                result.Changed = new Fs.FsDirectory[] { (Fs.FsDirectory)volume.GetCurrentWorkingDirectory(Target) };

                return new JsonResponseWriter(result);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}

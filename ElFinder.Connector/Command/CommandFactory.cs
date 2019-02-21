using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace ElFinder.Connector.Command
{
    public static class CommandFactory
    {
        public static ICommand CreateCommand(string name, NameValueCollection cmdParams, ElFinder elFinder, IFormFileCollection files)
        {
            switch (name)
            {
                case "open":
                    return new OpenCommand(cmdParams, elFinder);
                case "mkdir":
                    return new MkDirCommand(cmdParams, elFinder);
                case "tree":
                    return new TreeCommand(cmdParams, elFinder);
                case "file":
                    return new FileCommand(cmdParams, elFinder);
                case "parents":
                    return new ParentsCommand(cmdParams, elFinder);
                case "upload":
                    return new UploadCommand(cmdParams, elFinder, files);
                case "ls":
                    return new LsCommand(cmdParams, elFinder);
                case "rename":
                    return new RenameCommand(cmdParams, elFinder);
                case "rm":
                    return new RmCommand(cmdParams, elFinder);
                case "size":
                    return new SizeCommand(cmdParams, elFinder);
                default:
                    throw new InvalidOperationException($"Command not found. Command name: {name}");
            }
        }
    }
}

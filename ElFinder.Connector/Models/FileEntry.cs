using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector.Models
{
    public class FileEntry: BaseFsEntry
    {
        public long Size { get; set; }
        public override bool IsDirectory() => false;
        public override bool IsFile() => true;
    }
}

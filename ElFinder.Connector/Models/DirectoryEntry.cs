using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector.Models
{
    public class DirectoryEntry : BaseFsEntry
    {
        public override bool IsDirectory() => true;
        public override bool IsFile() => false;

        public bool HasSubDirectories { get; set; }
    }
}

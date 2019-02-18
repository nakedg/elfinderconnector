using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector.Models
{
    public abstract class BaseFsEntry
    {
        public DateTimeOffset LastModified { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string ParentPath { get; set; }

        //public long Size { get; set; }

        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }

        public abstract bool IsDirectory();
        public abstract bool IsFile();
    }
}

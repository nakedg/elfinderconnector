using ElFinder.Connector.Models;

namespace ElFinder.Connector.Volume
{
    public interface IVolumeDriver
    {
        string Root { get; set; }

        string Prefix { get; set; }

        DirectoryEntry GetCurrentWorkingDirectory(string path);
        BaseFsEntry[] GetDirectoryItems(string path);
        DirectoryEntry CreateDirectory(string path, string name);
        (System.IO.Stream, string) GetFile(string path);
        BaseFsEntry[] GetParents(string path, string untilPath);

        BaseFsEntry Upload(string path, string name, System.IO.Stream stream);
    }
}
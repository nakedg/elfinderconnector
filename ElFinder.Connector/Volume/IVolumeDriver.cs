using ElFinder.Connector.Models;
using System.Threading.Tasks;

namespace ElFinder.Connector.Volume
{
    public interface IVolumeDriver
    {
        string Root { get; set; }

        string Prefix { get; set; }

        Task<DirectoryEntry> GetCurrentWorkingDirectory(string path);
        Task<BaseFsEntry[]> GetDirectoryItems(string path);
        Task<DirectoryEntry> CreateDirectory(string path, string name);
        Task<(System.IO.Stream, string)> GetFile(string path);
        Task<BaseFsEntry[]> GetParents(string path, string untilPath);

        Task<BaseFsEntry> Upload(string path, string name, System.IO.Stream stream);

        Task<BaseFsEntry> Rename(string path, string newName);

        Task Delete(string path);

        Task<FsItemSize> GetSize(string path);
    }
}
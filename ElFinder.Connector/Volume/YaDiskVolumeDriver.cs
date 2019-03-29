using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ElFinder.Connector.Models;
using WebDav;
using System.Linq;

namespace ElFinder.Connector.Volume
{
    public class YaDiskVolumeDriver : IVolumeDriver
    {
        private static IWebDavClient Client;

        public string Root { get; set; } = "/files";
        public string Prefix { get; set; } = "ya";

        public YaDiskVolumeDriver(string url, string username, string password, string root)
        {
            Root = root;
            Client = new WebDavClient(new WebDavClientParams { BaseAddress = new Uri(url), Credentials = new System.Net.NetworkCredential(username, password) });
            Task.Run(() => Init()).GetAwaiter().GetResult();
        }

        private async Task Init()
        {
            var exist = await DirectoryExist(Root);
            if (!exist)
            {
                var resp = await Client.Mkcol(Root);
            }
        }

        public async Task<DirectoryEntry> CreateDirectory(string path, string name)
        {
            if (name.StartsWith("\\") || name.StartsWith("/"))
            {
                name = name.Substring(1);
            }

            var fullPath = GetFullPath(path);
            fullPath = CombinePath(fullPath, name);
            var result = await Client.Mkcol(fullPath);

            if (result.IsSuccessful)
            {
                return new DirectoryEntry
                {
                    CanRead = true,
                    CanWrite = true,
                    HasSubDirectories = false,
                    LastModified = DateTimeOffset.Now,
                    Name = name,
                    ParentPath = path,
                    Path = GetRelativePath(Root, fullPath)
                };
            }
            else
            {
                throw new InvalidOperationException($"Не удалось создать каталог. Status: {result.StatusCode} - {result.Description}");
            }

        }

        public async Task Delete(string path)
        {
            var fullPath = GetFullPath(path);
            var result = await Client.Delete(fullPath);

            if (!result.IsSuccessful)
            {
                throw new InvalidOperationException($"Не удалось удалить каталог. Status: {result.StatusCode} - {result.Description}");
            }
            
        }

        public async Task<DirectoryEntry> GetCurrentWorkingDirectory(string path)
        {
            var fullpath = GetFullPath(path);

            var directoryInfo = await GetDirectoryInfo(fullpath);

            var hasDirectories = directoryInfo.DirectoryItems.Any(r => r.IsCollection);

            return (DirectoryEntry)Convert(directoryInfo.Directory, hasDirectories);
        }

        public async Task<BaseFsEntry[]> GetDirectoryItems(string path)
        {
            string fullpath = GetFullPath(path);

            var directoryInfo = await GetDirectoryInfo(fullpath);

            List<BaseFsEntry> baseEntrys = new List<BaseFsEntry>(directoryInfo.DirectoryItems.Count());

            foreach (var item in directoryInfo.DirectoryItems)
            {
                bool hasSubDirectories = false;
                if (item.IsCollection)
                {
                    var itemInfo = await GetDirectoryInfo(item.Uri);
                    hasSubDirectories = itemInfo.DirectoryItems.Any(r => r.IsCollection);
                }

                baseEntrys.Add(Convert(item, hasSubDirectories));
            }

            return baseEntrys.ToArray();

        }

        public async Task<(Stream, string)> GetFile(string path)
        {
            string fullPath = GetFullPath(path);

            var result = await Client.Propfind(fullPath);
            

            if (result.IsSuccessful)
            {
                var file = result.Resources.FirstOrDefault();

                if (file == null)
                {
                    throw new FileNotFoundException($"Path: {fullPath}");
                }

                using (var stream = await Client.GetRawFile(file.Uri))
                {
                    byte[] buffer = new byte[32768];
                    var ms = new MemoryStream();
                    int readedBytes = 0;
                    while ((readedBytes = await stream.Stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await ms.WriteAsync(buffer, 0, readedBytes);
                    }

                    return (ms, file.DisplayName);
                }
            }
            else {
                throw new FileNotFoundException($"Path: {fullPath}");
            }
        }

        public async Task<BaseFsEntry[]> GetParents(string path, string untilPath)
        {
            var fullPath = GetFullPath(path);

            string untilFullPath;

            if (string.IsNullOrEmpty(untilPath))
            {
                untilFullPath = Root;
            }
            else
            {
                untilFullPath = GetFullPath(untilPath);
            }

            var dir = await GetDirectoryInfo(fullPath);


            Stack<WebDavDirectoryInfo> stack = new Stack<WebDavDirectoryInfo>();
            stack.Push(dir);
            List<BaseFsEntry> res = new List<BaseFsEntry>();
            while (stack.Count > 0)
            {
                var item = stack.Pop();

                if (item.Directory.Uri != untilFullPath)
                {
                    var parentPath = GetParentPath(item.Directory.Uri);
                    var parent = await GetDirectoryInfo(parentPath);
                    var dirs = parent.DirectoryItems.Where(r => r.IsCollection);

                    foreach (var d in dirs)
                    {
                        var di = await GetDirectoryInfo(d.Uri);
                        res.Add(Convert(di.Directory, di.DirectoryItems.Any(r => r.IsCollection)));
                    }

                    stack.Push(parent);
                }
                else
                {
                    res.Add(Convert(item.Directory, item.DirectoryItems.Any(r => r.IsCollection)));
                }
            }

            return res.ToArray();
        }

        public async Task<FsItemSize> GetSize(string path)
        {
            var fullpath = GetFullPath(path);

            var directoryInfo = await GetDirectoryInfo(fullpath);

            if (directoryInfo.Directory.IsCollection)
            {
                return await CalculateDirectorySize(directoryInfo);
            }
            else
            {
                return new FsItemSize { FileCount = 1, DirectoryCount = 0, Size = directoryInfo.Directory.ContentLength ?? 0 };
            }
        }

        private async Task<FsItemSize> CalculateDirectorySize(WebDavDirectoryInfo di)
        {
            Stack<WebDavDirectoryInfo> stack = new Stack<WebDavDirectoryInfo>();

            stack.Push(di);

            FsItemSize result = new FsItemSize();

            while (stack.Count > 0)
            {
                var dir = stack.Pop();
                result.DirectoryCount++;

                foreach (var item in dir.DirectoryItems)
                {
                    if (item.IsCollection)
                    {
                        var directoryInfo = await GetDirectoryInfo(item.Uri);
                        stack.Push(directoryInfo);
                    }
                    else
                    {
                        result.Size += item.ContentLength ?? 0;
                        result.FileCount++;
                    }
                }
            }

            return result;
        }

        public async Task<BaseFsEntry> Rename(string path, string newName)
        {
            var fullpath = GetFullPath(path);
            var newPath = CombinePath(GetParentPath(fullpath), newName);

            var result = await Client.Move(fullpath, newPath);

            if (result.IsSuccessful)
            {
                var directoryInfo = await GetDirectoryInfo(newPath);
                return Convert(directoryInfo.Directory, directoryInfo.DirectoryItems.Any(r => r.IsCollection));
            }
            else
            {
                throw new InvalidOperationException($"Status code: {result.StatusCode}. Description: {result.Description}");
            }
        }

        public async Task<BaseFsEntry> Upload(string path, string name, Stream stream)
        {
            var fullpath = GetFullPath(path);
            var filepath = CombinePath(fullpath, name);

            stream.Seek(0, SeekOrigin.Begin);

            var result = await Client.PutFile(filepath, stream);

            if (result.IsSuccessful)
            {
                var directoryInfo = await GetDirectoryInfo(filepath);
                return Convert(directoryInfo.Directory, directoryInfo.DirectoryItems.Any(r => r.IsCollection));
            }
            else
            {
                throw new InvalidOperationException($"Status code: {result.StatusCode}. Description: {result.Description}");
            }
        }

        private async Task<bool> DirectoryExist(string fullpath)
        {
            var result = await Client.Propfind(fullpath);

            if (result.IsSuccessful)
            {
                return true;
            }
            else if (result.StatusCode == 404)
            {
                return false;
            }
            else
            {
                throw new InvalidOperationException($"Status code: {result.StatusCode}. Description: {result.Description}");
            }
        }

        private string GetFullPath(string relativePath)
        {
            if (relativePath.StartsWith("\\") || relativePath.StartsWith("/"))
            {
                relativePath = relativePath.Substring(1);
            }

            return Path.Combine(Root, relativePath).Replace("\\", "/");
        }

        private string GetRelativePath(string relativeTo, string path)
        {
            path = path.Replace("\\", "/");

            if (path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            if (path.StartsWith(relativeTo))
            {
                return path.Replace(relativeTo, "");
            }
            else
            {
                return null;
            }
        }

        private string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path) || path == "/" || path == "\\")
            {
                return null;
            }

            path = path.Replace("\\", "/");

            if (path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            var idx = path.LastIndexOf("/");
            return path.Substring(0, idx);
        }

        private string CombinePath(string path1, string path2)
        {
            if (path2.StartsWith("/"))
            {
                path2 = path2.Substring(1);
            }
            return Path.Combine(path1, path2).Replace("\\", "/");
        }

        private BaseFsEntry Convert(WebDavResource resource, bool hasDirectories)
        {
            if (resource.IsCollection)
            {
                return new DirectoryEntry
                {
                    CanRead = true,
                    CanWrite = true,
                    HasSubDirectories = hasDirectories,
                    LastModified = resource.LastModifiedDate ?? new DateTime(),
                    Name = resource.DisplayName,
                    Path = GetRelativePath(Root, resource.Uri),
                    ParentPath = GetRelativePath(Root, GetParentPath(resource.Uri))
                };
            }
            else
            {
                return new FileEntry
                {
                    CanRead = true,
                    CanWrite = true,
                    LastModified = resource.LastModifiedDate ?? new DateTime(),
                    Name = resource.DisplayName,
                    Path = GetRelativePath(Root, resource.Uri),
                    ParentPath = GetRelativePath(Root, GetParentPath(resource.Uri)),
                    Size = resource.ContentLength ?? 0,
                    Md5Hash = resource.ETag
                };
            }
        }

        private async Task<WebDavDirectoryInfo> GetDirectoryInfo(string path)
        {
            var result = await Client.Propfind(path);

            if (result.IsSuccessful)
            {
                return new WebDavDirectoryInfo
                {
                    Directory = result.Resources.First(),
                    DirectoryItems = result.Resources.Skip(1)
                };
            }
            else
            {
                throw new InvalidOperationException($"Status code: {result.StatusCode}. Description: {result.Description}");
            }
        }
    }

    struct WebDavDirectoryInfo
    {
        public WebDavResource Directory { get; set; }
        public IEnumerable<WebDavResource> DirectoryItems { get; set; }
    }
}

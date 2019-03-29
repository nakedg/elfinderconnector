using System;
using System.Collections.Generic;
using System.Text;
using ElFinder.Connector.Models;
using System.IO;
using System.Threading.Tasks;

namespace ElFinder.Connector.Volume
{
    public class FsVolumeDriver : IVolumeDriver
    {

        public FsVolumeDriver(string root)
        {
            Root = root;

            Init();

        }

        private void Init()
        {
            if (!Directory.Exists(Root))
            {
                Directory.CreateDirectory(Root);
            }
        }

        public string Prefix { get; set; } = "l";

        public string Root { get; set; }

        public Task<DirectoryEntry> GetCurrentWorkingDirectory(string path)
        {
            string fullPath = GetFullPath(path);// Path.Combine(Root, path);

            if (Directory.Exists(fullPath))
            {
                DirectoryInfo di = new DirectoryInfo(fullPath);

                var cwd = Convert(di);

                return Task.FromResult(cwd);
            }
            else
            {
                throw new DirectoryNotFoundException($"Path: {fullPath}");
            }
        }


        public Task<BaseFsEntry[]> GetDirectoryItems(string path)
        {
            string fullPath = GetFullPath(path);// Path.Combine(Root, path);

            if (System.IO.Directory.Exists(fullPath))
            {
                   
                DirectoryInfo di = new DirectoryInfo(fullPath);
                var fsItems = di.GetFileSystemInfos();

                List<BaseFsEntry> baseEntrys = new List<BaseFsEntry>(fsItems.Length);

                foreach (var item in fsItems)
                {
                    if (item is DirectoryInfo)
                    {
                        var d = item as DirectoryInfo;

                        var dir = Convert(d);
                        baseEntrys.Add(dir);
                    }
                    else
                    {
                        var f = item as FileInfo;

                        var file = Convert(f);

                        baseEntrys.Add(file);
                    }
                }
                return Task.FromResult(baseEntrys.ToArray());
            }
            else
            {
                throw new DirectoryNotFoundException("Каталог не найден");
            }
            
        }

        public Task<DirectoryEntry> CreateDirectory(string path, string name)
        {
            var currentDirectoryPath = GetFullPath(path);

            if (name.StartsWith("\\") || name.StartsWith("/"))
            {
                name = name.Substring(1);
            }

            if (Directory.Exists(currentDirectoryPath))
            {
                var newDirectoryPath = Path.Combine(currentDirectoryPath, name);
                Directory.CreateDirectory(newDirectoryPath);

                return GetCurrentWorkingDirectory(GetRelativePath(Root, newDirectoryPath));
            }
            else
            {
                throw new DirectoryNotFoundException($"Path: {currentDirectoryPath}");
            }
        }

        public Task<(Stream, string)> GetFile(string path)
        {
            var fullPath = GetFullPath(path);

            if (File.Exists(fullPath))
            {
                return Task.FromResult( ((Stream, string))(File.OpenRead(fullPath), Path.GetFileName(fullPath)) );
            }
            else
            {
                throw new FileNotFoundException($"Path: {fullPath}");
            }
        }

        public Task<BaseFsEntry[]> GetParents(string path, string untilPath)
        {
            var fullpath = GetFullPath(path);

            string untilFullPath;

            if (string.IsNullOrEmpty(untilPath))
            {
                untilFullPath = Root;
            }
            else
            {
                untilFullPath = GetFullPath(untilPath);
            }

            if (!Directory.Exists(fullpath))
            {
                throw new DirectoryNotFoundException($"Path: {fullpath}");
            }

            DirectoryInfo dir = new DirectoryInfo(fullpath);

            Stack<DirectoryInfo> stack = new Stack<DirectoryInfo>();
            stack.Push(dir);
            List<BaseFsEntry> result = new List<BaseFsEntry>();
            while (stack.Count > 0)
            {
                var item = stack.Pop();



                if (item.FullName != untilFullPath)
                {
                    var parent = item.Parent;
                    var dirs = parent.GetDirectories();

                    foreach (var d in dirs)
                    {
                        result.Add(Convert(d));

                    }

                    stack.Push(parent);
                }
                else
                {
                    result.Add(Convert(item));
                }

            }


            return Task.FromResult(result.ToArray());
        }

        public async Task<BaseFsEntry> Upload(string path, string name, Stream stream)
        {
            var fullpath = GetFullPath(path);

            if (!Directory.Exists(fullpath))
            {
                throw new DirectoryNotFoundException($"Path: {fullpath}");
            }

            var filepath = Path.Combine(fullpath, name);

            stream.Seek(0, SeekOrigin.Begin);

            if (!File.Exists(filepath))
            {
                using (var fs = File.Create(filepath))
                {
                    await stream.CopyToAsync(fs);
                }
            }
            else
            {
                //todo
            }

            var f = new FileInfo(filepath);

            var file = Convert(f);

            return file;
        }

        public Task<BaseFsEntry> Rename(string path, string newName)
        {
            var fullpath = GetFullPath(path);

            if (File.Exists(fullpath))
            {
                //file rename
                FileInfo fi = new FileInfo(fullpath);

                var newFullpath = Path.Combine(fi.DirectoryName, newName);

                var removed = Convert(fi);

                fi.MoveTo(newFullpath);

                FileInfo newFi = new FileInfo(newFullpath);
                return Task.FromResult((BaseFsEntry)Convert(newFi));
            }
            else if (Directory.Exists(fullpath))
            {
                //directory rename
                DirectoryInfo di = new DirectoryInfo(fullpath);

                var newPath = Path.Combine(di.Parent.FullName, newName);

                di.MoveTo(newPath);

                var newDi = new DirectoryInfo(newPath);

                return Task.FromResult((BaseFsEntry)Convert(newDi));
            }
            else
            {
                throw new FileNotFoundException("Directory or file not found", fullpath);
            }
        }

        public Task Delete(string path)
        {
            var fullpath = GetFullPath(path);

            if (Directory.Exists(fullpath))
            {
                Directory.Delete(fullpath, true);
            }
            else if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }
            else
            {
                throw new FileNotFoundException("Directory or file not found", fullpath);
            }

            return Task.CompletedTask;
        }

        public Task<FsItemSize> GetSize(string path)
        {
            var fullpath = GetFullPath(path);

            if (File.Exists(fullpath))
            {
                FileInfo fi = new FileInfo(fullpath);
                return Task.FromResult(new FsItemSize { FileCount = 1, DirectoryCount = 0, Size = fi.Length });
            }
            else if (Directory.Exists(fullpath))
            {
                DirectoryInfo di = new DirectoryInfo(fullpath);
                return CalculateDirectorySize(di);
            }
            else
            {
                throw new FileNotFoundException("Directory or file not found", fullpath);
            }
        }

        private Task<FsItemSize> CalculateDirectorySize(DirectoryInfo di)
        {
            Stack<DirectoryInfo> stack = new Stack<DirectoryInfo>();
            stack.Push(di);

            FsItemSize result = new FsItemSize();

            while (stack.Count > 0)
            {
                var dir = stack.Pop();
                result.DirectoryCount++;
                foreach (var file in dir.EnumerateFiles())
                {
                    result.Size += file.Length;
                    result.FileCount++;
                }

                foreach (var item in dir.EnumerateDirectories())
                {
                    //result.DirectoryCount++;
                    stack.Push(item);
                }
            }

            return Task.FromResult(result);
        }

        private string GetRelativePath(string relativeTo, string path)
        {
            if (path.StartsWith(relativeTo))
            {
                return path.Replace(relativeTo, "");
            }
            else
            {
                return null;
            }
        }

        private string GetFullPath(string relativePath)
        {
            if (relativePath.StartsWith("\\") || relativePath.StartsWith("/"))
            {
                relativePath = relativePath.Substring(1);
            }

            return Path.Combine(Root, relativePath);
        }

        private FileEntry Convert(FileInfo file)
        {
            string md5hash;

            using (var stream = file.OpenRead())
            {
                md5hash = GetMD5(stream);
            }

            return new FileEntry
            {
                CanRead = true,
                CanWrite = true,
                Name = file.Name,
                Path = GetRelativePath(Root, file.FullName),
                ParentPath = GetRelativePath(Root, file.DirectoryName),
                Size = file.Length,
                LastModified = file.LastWriteTime,
                Md5Hash = md5hash
            };
        }

        private DirectoryEntry Convert(DirectoryInfo dir)
        {
            return new DirectoryEntry
            {
                CanRead = true,
                CanWrite = true,
                HasSubDirectories = dir.GetDirectories().Length > 0,
                Name = dir.Name,
                Path = GetRelativePath(Root, dir.FullName),
                ParentPath = GetRelativePath(Root, dir.Parent?.FullName),
                LastModified = dir.LastWriteTime
            };
        }

        private string GetMD5(System.IO.Stream data)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                data.Seek(0, SeekOrigin.Begin);
                var hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

    }
}

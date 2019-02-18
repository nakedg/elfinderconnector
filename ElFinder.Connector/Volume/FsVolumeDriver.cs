using System;
using System.Collections.Generic;
using System.Text;
using ElFinder.Connector.Models;
using System.IO;

namespace ElFinder.Connector.Volume
{
    public class FsVolumeDriver : IVolumeDriver
    {

        public FsVolumeDriver(string root)
        {
            Root = root;
        }

        public string Prefix { get; set; } = "l";

        public string Root { get; set; }

        public DirectoryEntry GetCurrentWorkingDirectory(string path)
        {
            string fullPath = GetFullPath(path);// Path.Combine(Root, path);

            if (Directory.Exists(fullPath))
            {
                DirectoryInfo di = new DirectoryInfo(fullPath);

                var cwd = new DirectoryEntry
                {
                    CanRead = true,
                    CanWrite = true,
                    HasSubDirectories = 
                    di.GetDirectories().Length > 0,
                    Name = di.Name,
                    Path = GetRelativePath(Root, di.FullName),
                    ParentPath = GetRelativePath(Root, di.Parent?.FullName),
                    LastModified = di.LastWriteTime
                };

                

                return cwd;
            }
            else
            {
                throw new DirectoryNotFoundException($"Path: {fullPath}");
            }
        }


        public BaseFsEntry[] GetDirectoryItems(string path)
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

                        var dir = new DirectoryEntry
                        {
                            CanRead = true,
                            CanWrite = true,
                            HasSubDirectories =
                            d.GetDirectories().Length > 0,
                            Name = d.Name,
                            Path = GetRelativePath(Root, d.FullName),
                            ParentPath = GetRelativePath(Root, d.Parent?.FullName),
                            LastModified = d.LastWriteTime
                        };
                        baseEntrys.Add(dir);
                    }
                    else
                    {
                        var f = item as FileInfo;

                        var file = new FileEntry
                        {
                            CanRead = true,
                            CanWrite = true,
                            Name = f.Name,
                            Path = GetRelativePath(Root, f.FullName),
                            ParentPath = GetRelativePath(Root, f.DirectoryName),
                            Size = f.Length,
                            LastModified = f.LastWriteTime
                        };

                        baseEntrys.Add(file);
                    }
                }
                return baseEntrys.ToArray();
            }
            else
            {
                throw new DirectoryNotFoundException("Каталог не найден");
            }
            
        }

        public DirectoryEntry CreateDirectory(string path, string name)
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

        public (Stream, string) GetFile(string path)
        {
            var fullPath = GetFullPath(path);

            if (File.Exists(fullPath))
            {
                return (File.OpenRead(fullPath), Path.GetFileName(fullPath));
              
            }
            else
            {
                throw new FileNotFoundException($"Path: {fullPath}");
            }
        }

        public BaseFsEntry[] GetParents(string path, string untilPath)
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
                        result.Add(new DirectoryEntry
                        {
                            CanRead = true,
                            CanWrite = true,
                            HasSubDirectories = d.GetDirectories().Length > 0,
                            Name = d.Name,
                            Path = GetRelativePath(Root, d.FullName),
                            ParentPath = GetRelativePath(Root, d.Parent?.FullName),
                            LastModified = d.LastWriteTime
                        });

                    }

                    stack.Push(parent);
                }
                else
                {
                    result.Add(new DirectoryEntry
                    {
                        CanRead = true,
                        CanWrite = true,
                        HasSubDirectories = item.GetDirectories().Length > 0,
                        Name = item.Name,
                        Path = GetRelativePath(Root, item.FullName),
                        ParentPath = GetRelativePath(Root, item.Parent?.FullName),
                        LastModified = item.LastWriteTime
                    });
                }

            }


            return result.ToArray();
        }

        public BaseFsEntry Upload(string path, string name, System.IO.Stream stream)
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
                    stream.CopyTo(fs);
                }
            }
            else
            {
                //todo
            }

            var f = new FileInfo(filepath);

            var file = new FileEntry
            {
                CanRead = true,
                CanWrite = true,
                Name = f.Name,
                Path = GetRelativePath(Root, f.FullName),
                ParentPath = GetRelativePath(Root, f.DirectoryName),
                Size = f.Length,
                LastModified = f.LastWriteTime
            };

            return file;
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

    }
}

using ElFinder.Connector.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;


namespace ElFinder.Connector.Volume
{

    public class Volume
    {
        private static int _counter = 0;
        private readonly string _volumeId;
        private readonly IVolumeDriver _volumeDriver;

        public Volume(IVolumeDriver volumeDriver)
            :this(volumeDriver, null)
        {
            
        }

        public Volume(IVolumeDriver volumeDriver, string volumeId)
        {
            _volumeId = volumeId ?? (_counter++).ToString();
            _volumeDriver = volumeDriver;
            Options = new Fs.RootOptions();
        }

        public ElFinder ElFinder { get; set; }

        public Fs.RootOptions Options { get; set; }

        public string VolumeId => $"{_volumeDriver.Prefix}{_volumeId}_";

        public bool CanRead { get; set; } = true;

        public string DefaultPath => GetHash("");

        public string ThumbnailPath { get; set; }

        private Fs.FsRoot _root;
        public Fs.FsRoot Root {
            get
            {
                if (_root == null)
                {
                    _root = (Fs.FsRoot)GetCurrentWorkingDirectory(GetHash(""));
                }

                return _root;
            }
        }

        public Fs.FsBase GetCurrentWorkingDirectory(string hash)
        {
            CheckVolumeIdInHash(hash);
            
            var cwd = _volumeDriver.GetCurrentWorkingDirectory(GetPath(hash));

            return CreateElFinderFsItem(cwd);
           
        }

        public Fs.FsBase[] GetDirectoryItems(string hash)
        {
            CheckVolumeIdInHash(hash);

            var items = _volumeDriver.GetDirectoryItems(GetPath(hash));

            List<Fs.FsBase> result = new List<Fs.FsBase>(items.Length);

            foreach (var item in items)
            {
                result.Add(CreateElFinderFsItem(item));
            }

            return result.ToArray();
        }

        public Fs.FsDirectory CreateDirectory(string hash, string name)
        {
            CheckVolumeIdInHash(hash);

            return (Fs.FsDirectory)CreateElFinderFsItem(_volumeDriver.CreateDirectory(GetPath(hash), name));
        }

        public (System.IO.Stream, string) GetFile(string hash)
        {
            CheckVolumeIdInHash(hash);

            return _volumeDriver.GetFile(GetPath(hash));
        }

        public Fs.FsBase[] GetParents(string hash, string until)
        {
            CheckVolumeIdInHash(hash);

            string untilPath = null;

            if (!string.IsNullOrEmpty(until))
            {
                untilPath = GetPath(until);
            }

            var items = _volumeDriver.GetParents(GetPath(hash), untilPath);

            return items.Select(i => CreateElFinderFsItem(i)).ToArray();
        }

        public Fs.FsBase Upload(string hashPath, string name, System.IO.Stream stream)
        {
            CheckVolumeIdInHash(hashPath);

            return CreateElFinderFsItem(_volumeDriver.Upload(GetPath(hashPath), name, stream));
        }

        public Fs.FsBase Rename(string hash, string newName)
        {
            CheckVolumeIdInHash(hash);

            var added = _volumeDriver.Rename(GetPath(hash), newName);

            return CreateElFinderFsItem(added);
        }

        public void Delete(string hash)
        {
            CheckVolumeIdInHash(hash);

            _volumeDriver.Delete(GetPath(hash));
        }

        public FsItemSize GetSize(string hash)
        {
            CheckVolumeIdInHash(hash);

            return _volumeDriver.GetSize(GetPath(hash));
        }

        public string CreateTmb(System.IO.Stream stream, string filename)
        {
            if (!System.IO.Directory.Exists(ThumbnailPath))
            {
                System.IO.Directory.CreateDirectory(ThumbnailPath);
            }

            string md5 = GetMD5(stream);
            string tmbName = $"{md5}{System.IO.Path.GetExtension(filename)}";

            using (var image = ElFinder.ImageProcessor.Load(stream))
            using (var ms = image.Resize(80, 80))
            using (System.IO.FileStream fs = new System.IO.FileStream(System.IO.Path.Combine(ThumbnailPath, tmbName), System.IO.FileMode.Create))
            {
                ms.Seek(0, System.IO.SeekOrigin.Begin);

                ms.CopyTo(fs);

                return tmbName;
            }
        }

        private bool IsImage(string mime)
        {
            return mime.ToLower().Contains("image");
        }

        private string GetMimeType(BaseFsEntry fsEntry)
        {
            if (fsEntry.IsDirectory())
            {
                return "directory";
            }

            return MimeTypeMap.GetMimeType(System.IO.Path.GetExtension(fsEntry.Name));
        }

        private string Encode(string str)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            return Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(buffer);
        }

        private string Decode(string hash)
        {
            byte[] buffer = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(hash);
            return Encoding.UTF8.GetString(buffer);
        }

        private string GetPath(string hash)
        {
            return Decode(hash.Replace(VolumeId, ""));
        }

        private string GetHash(string path)
        {
            if (path == null)
            {
                return string.Empty;
            }
            return VolumeId + Encode(path);
        }

        private void CheckVolumeIdInHash(string hash)
        {
            if (!hash.StartsWith(VolumeId))
            {
                throw new InvalidOperationException();
            }
        }

        private string GetMD5(System.IO.Stream data)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                data.Seek(0, System.IO.SeekOrigin.Begin);
                var hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        private string GetTmb(System.IO.Stream stream, string filename)
        {
            if (!System.IO.Directory.Exists(ThumbnailPath))
            {
                System.IO.Directory.CreateDirectory(ThumbnailPath);
                return "1";
            }

            string md5 = GetMD5(stream);
            string tmbName = $"{md5}{System.IO.Path.GetExtension(filename)}";

            if (System.IO.File.Exists(System.IO.Path.Combine(ThumbnailPath, tmbName)))
            {
                return tmbName;
            }
            else
            {
                return "1";
            }

        }

        

        private Fs.FsBase CreateElFinderFsItem(BaseFsEntry fsEntry)
        {
            if (fsEntry == null)
            {
                throw new ArgumentNullException(nameof(fsEntry));
            }

            string mimeType = GetMimeType(fsEntry);

            if (fsEntry is FileEntry)
            {

                var file = fsEntry as FileEntry;
                if (IsImage(mimeType))
                {

                    var (stream, filename) = GetFile(GetHash(file.Path));

                    using (stream)
                    using (var image = ElFinder.ImageProcessor.Load(stream))
                    {

                        

                        return new Fs.FsImageFile
                        {
                            Hash = GetHash(file.Path),
                            Locked = 0,
                            Mime = mimeType,
                            Name = file.Name,
                            ParentHash = GetHash(file.ParentPath),
                            Read = file.CanRead ? 1 : 0,
                            Size = file.Size,
                            Timestamp = file.LastModified.ToUnixTimeSeconds(),
                            Write = file.CanWrite ? 1 : 0,
                            Tmb = GetTmb(stream, filename),
                            Deminisation = image.GetDeminisation()
                        };
                    }
                }
                else
                {
                    return new Fs.FsFile
                    {
                        Hash = GetHash(file.Path),
                        Locked = 0,
                        Mime = mimeType,
                        Name = file.Name,
                        ParentHash = GetHash(file.ParentPath),
                        Read = file.CanRead ? 1 : 0,
                        Size = file.Size,
                        Timestamp = file.LastModified.ToUnixTimeSeconds(),
                        Write = file.CanWrite ? 1 : 0
                    };
                }
            }
            else {
                var directory = fsEntry as DirectoryEntry;

                if (directory.Path == string.Empty)
                {
                    //Root
                    return new Fs.FsRoot
                    {
                        HasDirs = directory.HasSubDirectories ? 1 : 0,
                        Hash = GetHash(directory.Path),
                        Locked = 1,
                        Mime = mimeType,
                        Name = directory.Name,
                        Options = Options,
                        ParentHash = GetHash(directory.ParentPath),
                        Read = directory.CanRead ? 1 : 0,
                        Timestamp = directory.LastModified.ToUnixTimeSeconds(),
                        VolimeId = VolumeId,
                        Write = directory.CanWrite ? 1 : 0
                    };
                }
                else
                {
                    return new Fs.FsDirectory
                    {
                        HasDirs = directory.HasSubDirectories ? 1 : 0,
                        Hash = GetHash(directory.Path),
                        Locked = 0,
                        Mime = mimeType,
                        Name = directory.Name,
                        ParentHash = GetHash(directory.ParentPath),
                        Read = directory.CanRead ? 1 : 0,
                        Timestamp = directory.LastModified.ToUnixTimeSeconds(),
                        VolimeId = VolumeId,
                        Write = directory.CanWrite ? 1 : 0
                    };
                }
            }
        }
    }
}

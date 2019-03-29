using ElFinder.Connector.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Threading.Tasks;

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
                    _root = (Fs.FsRoot)Task.Run(() => GetCurrentWorkingDirectory(GetHash(""))).GetAwaiter().GetResult();
                    /*var awaiter = GetCurrentWorkingDirectory(GetHash("")).GetAwaiter();
                    _root = (Fs.FsRoot)awaiter.GetResult();*/
                }

                return _root;
            }
        }

        public async Task<Fs.FsBase> GetCurrentWorkingDirectory(string hash)
        {
            CheckVolumeIdInHash(hash);
            
            var cwd = await _volumeDriver.GetCurrentWorkingDirectory(GetPath(hash));

            return await CreateElFinderFsItem(cwd);
           
        }

        public async Task<Fs.FsBase[]> GetDirectoryItems(string hash)
        {
            CheckVolumeIdInHash(hash);

            var items = await _volumeDriver.GetDirectoryItems(GetPath(hash));

            List<Fs.FsBase> result = new List<Fs.FsBase>(items.Length);

            foreach (var item in items)
            {
                result.Add(await CreateElFinderFsItem(item));
            }

            return result.ToArray();
        }

        public async Task<Fs.FsDirectory> CreateDirectory(string hash, string name)
        {
            CheckVolumeIdInHash(hash);

            return (Fs.FsDirectory) await CreateElFinderFsItem(await _volumeDriver.CreateDirectory(GetPath(hash), name));
        }

        public async Task<(System.IO.Stream, string)> GetFile(string hash)
        {
            CheckVolumeIdInHash(hash);

            return await _volumeDriver.GetFile(GetPath(hash));
        }

        public async Task<Fs.FsBase[]> GetParents(string hash, string until)
        {
            CheckVolumeIdInHash(hash);

            string untilPath = null;

            if (!string.IsNullOrEmpty(until))
            {
                untilPath = GetPath(until);
            }

            var items = await _volumeDriver.GetParents(GetPath(hash), untilPath);

            return items.Select(async i => await CreateElFinderFsItem(i)).Select(t => t.Result).ToArray();
        }

        public async Task<Fs.FsBase> Upload(string hashPath, string name, System.IO.Stream stream)
        {
            CheckVolumeIdInHash(hashPath);

            return await CreateElFinderFsItem(await _volumeDriver.Upload(GetPath(hashPath), name, stream));
        }

        public async Task<Fs.FsBase> Rename(string hash, string newName)
        {
            CheckVolumeIdInHash(hash);

            var added = await _volumeDriver.Rename(GetPath(hash), newName);

            return await CreateElFinderFsItem(added);
        }

        public async Task Delete(string hash)
        {
            CheckVolumeIdInHash(hash);

            await _volumeDriver.Delete(GetPath(hash));
        }

        public async Task<FsItemSize> GetSize(string hash)
        {
            CheckVolumeIdInHash(hash);

            return await _volumeDriver.GetSize(GetPath(hash));
        }

        public string CreateTmb(System.IO.Stream stream, string filename)
        {
            if (!System.IO.Directory.Exists(ThumbnailPath))
            {
                System.IO.Directory.CreateDirectory(ThumbnailPath);
            }

            string md5 = GetMD5(stream);
            

            using (var image = ElFinder.ImageProcessor.Load(stream))
            
            {
                var deminisation = image.GetDeminisation();

                string tmbName = $"{md5}_{deminisation}{System.IO.Path.GetExtension(filename)}";

                using (var ms = image.Resize(80, 80))
                using (System.IO.FileStream fs = new System.IO.FileStream(System.IO.Path.Combine(ThumbnailPath, tmbName), System.IO.FileMode.Create))
                {
                    ms.Seek(0, System.IO.SeekOrigin.Begin);

                    ms.CopyTo(fs);

                    return tmbName;
                }
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

        

        private string GetTmb(string md5)
        {
            if (!System.IO.Directory.Exists(ThumbnailPath))
            {
                System.IO.Directory.CreateDirectory(ThumbnailPath);
                return "1";
            }

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(ThumbnailPath);

            var serarchPattern = $"{md5}*";

            var files = di.GetFiles(serarchPattern);

            if (files.Length > 0)
            {
                return files[0].Name;
            }
            else
            {
                return "1";
            }

            /*string tmbName = $"{md5}{System.IO.Path.GetExtension(filename)}";

            if (System.IO.File.Exists(System.IO.Path.Combine(ThumbnailPath, tmbName)))
            {
                return tmbName;
            }
            else
            {
                return "1";
            }*/

        }

        

        private  Task<Fs.FsBase> CreateElFinderFsItem(BaseFsEntry fsEntry)
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
                    var tmb = GetTmb(file.Md5Hash);
                    string deminisation = string.Empty;

                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"_(\d*x\d*)");
                    var m = regex.Match(tmb);

                    if (m.Success)
                    {
                        deminisation = m.Groups[1].Value;
                    }


                    return Task.FromResult<Fs.FsBase>(new Fs.FsImageFile
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
                        Tmb = tmb,
                        Deminisation = deminisation

                    });
                    //var (stream, filename) = await GetFile(GetHash(file.Path));

                    // using (stream)
                    //using (var image = ElFinder.ImageProcessor.Load(stream))
                    //{

                    //}
                }
                else
                {
                    return Task.FromResult<Fs.FsBase>(new Fs.FsFile
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
                    });
                }
            }
            else {
                var directory = fsEntry as DirectoryEntry;

                if (directory.Path == string.Empty)
                {
                    //Root
                    return Task.FromResult<Fs.FsBase>(new Fs.FsRoot
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
                    });
                }
                else
                {
                    return Task.FromResult<Fs.FsBase>(new Fs.FsDirectory
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
                    });
                }
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
    }
}

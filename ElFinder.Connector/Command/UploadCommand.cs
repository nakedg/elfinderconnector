using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using ElFinder.Connector.ResponseWriter;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ElFinder.Connector.Command
{
    public class UploadCommand: Command
    {

        private static Dictionary<string, ChunkStorageItem> chunkStorage = new Dictionary<string, ChunkStorageItem>();

        public IFormFileCollection Files { get; set; }
        public string Target { get; set; }
        public string[] UploadPaths { get; set; }
        public string[] MTimes { get; set; }
        public string[] Names { get; set; }
        public string[] Renames { get; set; }
        public string Suffix { get; set; }
        public Dictionary<string, string> Hashes { get; set; }
        public bool Overwrite { get; set; }

        public string Chunk { get; set; }
        public string ChunkId { get; set; }
        public string Range { get; set; }

        public UploadCommand(NameValueCollection cmdParams, ElFinder elFinder, IFormFileCollection files)
            :base(cmdParams, elFinder)
        {
            Files = files;
        }

        protected override void InitCommand()
        {
            Target = CmdParams.Get("target");
            UploadPaths = CmdParams.GetValues("upload_path[]");
            MTimes = CmdParams.GetValues("mtime[]");
            Names = CmdParams.GetValues("name[]");
            Renames = CmdParams.GetValues("renames[]");
            Suffix = CmdParams.Get("suffix");
            Hashes = ParseDict("hashes", CmdParams);
            Overwrite = ParseBoolParameter(CmdParams.Get("overwrite"));

            Chunk = CmdParams.Get("chunk");
            ChunkId = CmdParams.Get("cid");
            Range = CmdParams.Get("range");
        }

        public override IResponseWriter Execute()
        {
            if (IsChunkUpload())
            {
                return ChunkUpload();
            }
            var volume = ElFinder.GetVolume(Target);

            List<Fs.FsBase> uploadedFiles = new List<Fs.FsBase>();

            for (int i = 0; i < Files.Count; i++)
            {
                var file = Files[i];
                var hashPath = UploadPaths != null && UploadPaths.Length > i ? UploadPaths[i] : Target;
                System.IO.MemoryStream ms = new System.IO.MemoryStream((int)file.Length);
                file.CopyTo(ms);
                uploadedFiles.Add(volume.Upload(hashPath, file.FileName, ms));
            }

            return new JsonResponseWriter(new { added = uploadedFiles.ToArray() });
        }

        private IResponseWriter ChunkUpload()
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^(.*).(\d+)_(\d+).part$");
            var match = regex.Match(Chunk);
            if (match.Success)
            {
                string filename = match.Groups[1].Value;
                int part = Convert.ToInt32(match.Groups[2].Value);
                int total = Convert.ToInt32(match.Groups[3].Value);

                string[] ranges = Range.Split(',');
                int startByte = Convert.ToInt32(ranges[0]);
                int chunkLenght = Convert.ToInt32(ranges[1]);
                int totalBytes = Convert.ToInt32(ranges[2]);

                byte[] chunkContent;
                using (var stream = Files[0].OpenReadStream())
                using (var sr = new System.IO.BinaryReader(stream))
                {
                    chunkContent = sr.ReadBytes(chunkLenght);
                }

                bool chunkMerged = StoreChunk(filename, startByte, chunkLenght, totalBytes, chunkContent);
                if (chunkMerged)
                {
                    return new JsonResponseWriter(new { added = new int[0], _chunkmerged = filename, _name = filename });
                }
                else
                {
                    return new JsonResponseWriter(new { added = new int[0] });
                }
            }
            else
            {
                var volume = ElFinder.GetVolume(Target);
                var hashPath = UploadPaths != null && UploadPaths.Length > 0 ? UploadPaths[0] : Target;
                var item = chunkStorage[Chunk];
                chunkStorage.Remove(Chunk);

                var added = volume.Upload(hashPath, Chunk, item.Stream);

                return new JsonResponseWriter(new { added = new Fs.FsBase[] { added } });
            }
        }

        private bool StoreChunk(string filename, int startByte, int chunkLenght, int totalBytes, byte[] content)
        {
            ChunkStorageItem item;

            if (chunkStorage.ContainsKey(filename))
            {
                item = chunkStorage[filename];
            }
            else
            {
                item = new ChunkStorageItem
                {
                    TotalUploaded = 0,
                    Stream = new System.IO.MemoryStream(totalBytes)
                };
                chunkStorage.Add(filename, item);
            }

            item.Stream.Seek(startByte, System.IO.SeekOrigin.Begin);
            using (var sw = new System.IO.BinaryWriter(item.Stream, System.Text.Encoding.UTF8, true))
            {
                sw.Write(content);
            }
            item.TotalUploaded += content.Length;

            return item.TotalUploaded == totalBytes;
        }

        private bool IsChunkUpload()
        {
            return !string.IsNullOrEmpty(Chunk);
        }
    }

    class ChunkStorageItem {
        public int TotalUploaded { get; set; }
        public System.IO.MemoryStream Stream { get; set; }
    }
    
}

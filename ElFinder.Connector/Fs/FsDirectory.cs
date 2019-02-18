using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector.Fs
{
    public class FsDirectory: FsBase
    {
        /// <summary>
        /// Only for directories. Marks if directory has child directories inside it. 0 (or not set) - no, 1 - yes. Do not need to calculate amount.
        /// </summary>
        [JsonProperty("dirs")]
        public int HasDirs { get; set; }

        /// <summary>
        /// Volume id. For directory only. It can include to options.
        /// </summary>
        [JsonProperty("volumeid")]
        public string VolimeId { get; set; }
    }

    public class FsRoot: FsDirectory
    {
        /// <summary>
        /// Is root
        /// </summary>
        [JsonProperty("isroot")]
        public int IsRoot { get; set; } = 1;

        /// <summary>
        /// For volume root only. This value is same to cwd.options.
        /// </summary>
        [JsonProperty("options")]
        public RootOptions Options { get; set; } = new RootOptions();

    }

    public class RootOptions
    {
        /// <summary>
        /// Current folder path
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        /// <summary>
        /// Current folder URL
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; } = "/files";

        /// <summary>
        /// Thumbnails folder URL
        /// </summary>
        [JsonProperty("tmbURL")]
        public string TmbUrl { get; set; } = "self";

        /// <summary>
        /// Path separator for the current volume
        /// </summary>
        [JsonProperty("separator")]
        public string Separator { get; set; } = System.IO.Path.DirectorySeparatorChar.ToString();

        /// <summary>
        /// List of commands not allowed (disabled) on this volume
        /// </summary>
        [JsonProperty("disabled")]
        public string[] Disabled { get; set; } = new string[] { "chmod" };

        /// <summary>
        /// Whether or not to overwrite files with the same name on the current volume when copy
        /// </summary>
        [JsonProperty("copyOverwrite")]
        public int CopyOverwrite { get; set; } = 1;

        /// <summary>
        /// Whether or not to overwrite files with the same name on the current volume when upload
        /// </summary>
        [JsonProperty("uploadOverwrite")]
        public int UploadOverwrite { get; set; } = 1;

        /// <summary>
        /// Upload max file size per file
        /// </summary>
        [JsonProperty("uploadMaxSize")]
        public int UploadMaxSize { get; set; } = int.MaxValue;

        /// <summary>
        /// Maximum number of chunked upload connection. `-1` to disable chunked upload
        /// </summary>
        [JsonProperty("uploadMaxConn")]
        public int UploadMaxConn { get; set; } = 3;

        /// <summary>
        /// MIME type checker for upload
        /// </summary>
        [JsonProperty("uploadMime")]
        public UploadMimeOptions UploadMime { get; set; } = new Fs.UploadMimeOptions
        {
            Allow = new string[] { "image" },//{ "image/x-ms-bmp", "image/gif", "image/jpeg", "image/png", "image/x-icon", "text/plain" },
            Deny = new string[] {  },
            FirstOrder = "deny"
        };

        /// <summary>
        /// Regular expression of MIME types that can be displayed inline with the `file` command
        /// </summary>
        [JsonProperty("dispInlineRegex")]
        public string DispInlineRegex { get; set; } = "^(?:(?:video|audio)|image/(?!.+\\+xml)|application/(?:ogg|x-mpegURL|dash\\+xml)|(?:text/plain|application/pdf)$)";

        /// <summary>
        /// JPEG quality to image resize / crop / rotate (1-100)
        /// </summary>
        [JsonProperty("jpgQuality")]
        public int JpgQuality { get; set; } = 100;

        /// <summary>
        /// Whether or not to current volume can detect update by the time stamp of the directory
        /// </summary>
        [JsonProperty("syncChkAsTs")]
        public int SyncChkAsTs { get; set; } = 1;

        /// <summary>
        /// Minimum inteval Milliseconds for auto sync
        /// </summary>
        [JsonProperty("syncMinMs")]
        public int SyncMinMs { get; set; } = 10000;

        /// <summary>
        /// Command conversion map for the current volume (e.g. chmod(ui) to perm(connector))
        /// </summary>
        [JsonProperty("uiCmdMap")]
        public Dictionary<string, string> UiCmdMap { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Is enabled i18n folder name that convert name to elFinderInstance.messages['folder_'+name]
        /// </summary>
        [JsonProperty("i18nFolderName")]
        public int I18nFolderName { get; set; } = 0;

        [JsonProperty("substituteImg")]
        public bool SubstituteImg { get; set; } = true;

        [JsonProperty("onetimeUrl")]
        public bool OnetimeUrl { get; set; } = false;
    }

    public class UploadMimeOptions
    {
        /// <summary>
        /// Allowed MIME type
        /// </summary>
        [JsonProperty("allow")]
        public string[] Allow { get; set; }

        /// <summary>
        /// Denied MIME type
        /// </summary>
        [JsonProperty("deny")]
        public string[] Deny { get; set; }

        /// <summary>
        /// First order to check ("deny" or "allow")
        /// </summary>
        [JsonProperty("firstOrder")]
        public string FirstOrder { get; set; }
    }
}

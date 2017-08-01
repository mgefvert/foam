using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetCommons;
using Foam.API.Configuration;
using Foam.API.Files;
using Foam.API.Transactions;

namespace Foam.API.Providers
{
    public class FileSystemProvider : IProvider
    {
        public bool Readonly => false;
        public bool CanHandleUri(Uri source) => false; // Catch-all in the CanHandleProtocol instead
        public bool CanHandleProtocol(string protocol) => protocol == "file";

        public void Dispose()
        {
        }

        public void Delete(string filespec)
        {
            if (File.Exists(filespec))
            {
                File.Delete(filespec);
                Logger.Log($"{GetType().Name}: File {filespec} deleted");
            }
            else
                Logger.Warn($"{GetType().Name}.Delete: File {filespec} does not exist");
        }

        public FileList Fetch(Uri location, string mask, CommitBuffer commitBuffer)
        {
            var path = location.LocalPath;
            var result = new FileList();

            if (string.IsNullOrEmpty(mask))
                mask = "*";

            foreach (var m in mask.Split(',').TrimAndFilter())
            {
                var files = Directory.EnumerateFiles(path, m, SearchOption.TopDirectoryOnly)
                    .Select(f => new FileItem(f))
                    .ToList();

                foreach (var file in files)
                {
                    commitBuffer.DeleteOnSuccess(this, file.OriginalFullName);
                    result.Add(file);
                }
            }

            return result;
        }

        public void Write(Uri location, List<FileItem> files, CommitBuffer commitBuffer, OverwriteMode overwrite)
        {
            var path = location.LocalPath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            foreach (var file in files)
            {
                var filename = Path.Combine(path, file.Name);
                Logger.Log($"Writing {filename} ({file.Length} bytes)");

                using (var fs = new FileStream(filename, FileMode.Create))
                using (var stream = file.GetStream(false))
                    stream.CopyTo(fs);

                commitBuffer.DeleteOnRollback(this, filename);
            }
        }
    }
}

using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace kangarup
{
    public class FileInfo
    {
        [YamlMember(Alias = "Url")]
        public string RelativeUrl { get; set; }
        public string Checksum { get; set; }

        public static async Task<FileInfo> FromAbsolutePathAsync(string absPath, string basePath)
        {
            var fileInfo = new FileInfo();

            // calc checksum
            fileInfo.Checksum = await Task.Run(() =>
            {
                using (var fileStream = File.OpenRead(absPath))
                {
                    var sha256 = SHA256.Create();
                    var hash = sha256.ComputeHash(fileStream);
                    return BitConverter.ToString(hash).Replace("-", string.Empty);
                }
            }).ConfigureAwait(false);

            // strip base bath
            fileInfo.RelativeUrl = Helper.GetRelativePath(basePath, absPath);

            return fileInfo;
        }
    }
}

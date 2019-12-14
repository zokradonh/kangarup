using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace kangarup
{
    /// <summary>
    /// This class contains all functions needed for deploying an application to a WebDAV Server.
    /// </summary>
    public class Deployment
    {
        public Uri UpdateUri { get; set; }
        public X509Certificate2 SignaturePrivateCertificate { get; set; }

        public ILogger Logger { get; set; }

        public Deployment(Uri updateUri)
        {
            UpdateUri = updateUri;
        }
        
        public async Task<UpdateInfo> CreateUpdateInfoAsync(string pathOfFiles, string patchTitle,
            Version newVersion, Func<string, bool> pathFilter = null)
        {
            if (!Directory.Exists(pathOfFiles))
            {
                throw new ArgumentException($"Specified path '{pathOfFiles}' does not exist.", nameof(pathOfFiles));
            }

            var updateInfo = new UpdateInfo()
            {
                PatchTitle = patchTitle,
                Version = newVersion.ToString()
            };

            // search for application files
            var files = Directory.EnumerateFiles(Path.GetFullPath(pathOfFiles), "*", SearchOption.AllDirectories)
                                        .Where(s => Path.GetFileName(s) != "kdeploy.ps1")
                                        .Where(s => !Path.GetFileName(s).EndsWith(".snk"));

            // filter files
            if (pathFilter != null) files = files.Where(pathFilter);

            // create FileInfo objects
            var hashComputeOperations = files.Select(filePath => FileInfo.FromAbsolutePathAsync(filePath, Path.GetFullPath(pathOfFiles)));

            // execute query => calc hashes
            updateInfo.Files = await Task.WhenAll(hashComputeOperations);

            return updateInfo;
        }

        public async Task<bool> DeployAsync(UpdateInfo info, NetworkCredential credentials)
        {
            try
            {
                var rsa = SignaturePrivateCertificate.GetRSAPrivateKey();

                // generate manifest bytes
                var serializer = new YamlDotNet.Serialization.Serializer();
                var yaml = serializer.Serialize(info);
                var yamlBytes = Encoding.UTF8.GetBytes(yaml);

                // generate signature for yaml manifest
                var signature = rsa.SignData(yamlBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                try
                {
                    // upload manifest and signature
                    var webClient = new WebClient { Credentials = credentials };
                    
                    await webClient.UploadDataTaskAsync(new Uri(UpdateUri, "update.yml"), "PUT", yamlBytes)
                        .ConfigureAwait(false);
                    await webClient.UploadDataTaskAsync(new Uri(UpdateUri, "update.sig"), "PUT", signature)
                        .ConfigureAwait(false);
                    Logger?.Info("Uploaded update manifest.");

                    // upload all application files
                    foreach (var infoFile in info.Files)
                    {
                        await webClient
                            .UploadFileTaskAsync(new Uri(UpdateUri, infoFile.RelativeUrl), "PUT", infoFile.RelativeUrl)
                            .ConfigureAwait(false);
                        Logger?.Info($"Uploaded {Path.GetFileName(infoFile.RelativeUrl)}.");
                    }

                    return true;
                }
                catch (WebException e)
                {
                    Logger?.Error("Unable to upload file.", e);
                    return false;
                }
            }
            catch (CryptographicException e)
            {
                Logger?.Error($"Failed to use private key: {e.Message}", e);
                return false;
            }
        }


    }
}

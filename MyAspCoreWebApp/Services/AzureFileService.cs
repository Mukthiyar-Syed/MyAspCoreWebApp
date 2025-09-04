using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System.IO;

namespace MyAspCoreWebApp.Services
{
    public class AzureFileService
    {
        private readonly string _connectionString;
        private readonly string _shareName;

        public AzureFileService(string connectionString, string shareName)
        {
            _connectionString = connectionString;
            _shareName = shareName;
        }

        public async Task UploadFileAsync(string fileName, Stream fileStream)
        {
            var shareClient = new ShareClient(_connectionString, _shareName);
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);

            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadAsync(fileStream);
        }

        public async Task<List<string>> ListFilesAsync()
        {
            var fileNames = new List<string>();
            var shareClient = new ShareClient(_connectionString, _shareName);
            var directoryClient = shareClient.GetRootDirectoryClient();

            // Iterate through the files in the directory
            await foreach (ShareFileItem fileItem in directoryClient.GetFilesAndDirectoriesAsync())
            {
                // Only add files, not subdirectories
                if (!fileItem.IsDirectory)
                {
                    fileNames.Add(fileItem.Name);
                }
            }
            return fileNames;
        }

        public async Task<ShareFileDownloadInfo> DownloadFileAsync(string fileName)
        {
            var shareClient = new ShareClient(_connectionString, _shareName);
            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);

            return await fileClient.DownloadAsync();
        }
    }
}

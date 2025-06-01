using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Infrastructure.Services
{
    public class GoogleDriveBackupService
    {
        private DriveService _service;

        public GoogleDriveBackupService()
        {
            InitializeService();
        }

        private void InitializeService()
        {
            GoogleCredential credential;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(new[] { DriveService.Scope.DriveFile });
            }

            _service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "WPF Backup App"
            });
        }


        public async Task<string> EnsureHotelFolderExistsAsync(string hotelName)
        {
            var request = _service.Files.List();
            request.Q = $"mimeType='application/vnd.google-apps.folder' and name='{hotelName}' and trashed=false";
            request.Fields = "files(id, name)";

            var result = await request.ExecuteAsync();
            var folder = result.Files.FirstOrDefault();

            if (folder != null)
            {
                return folder.Id; // Folder already exists, return its ID
            }

            // Create new folder
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = hotelName,
                MimeType = "application/vnd.google-apps.folder"
            };

            var createRequest = _service.Files.Create(fileMetadata);
            createRequest.Fields = "id";
            var newFolder = await createRequest.ExecuteAsync();

            return newFolder.Id;
        }

        public async Task<(bool isSuccess, string? responseId )> UploadBackupAsync(string filePath, string hotelName)
        {
            string folderId = await EnsureHotelFolderExistsAsync(hotelName);

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath),
                Parents = new List<string> { folderId }
            };

            using var fileStream = new FileStream(filePath, FileMode.Open);
            var request = _service.Files.Create(fileMetadata, fileStream, "application/octet-stream");
            request.Fields = "id";
            request.ChunkSize = Google.Apis.Upload.ResumableUpload.MinimumChunkSize * 2; // Enable chunked upload

            var progress = await request.UploadAsync();
            if (progress.Status == Google.Apis.Upload.UploadStatus.Failed)
            {
                return (false, null);
                throw new Exception($"Upload failed: Might be caused by network issues. {progress.Exception.Message}");
            }

            return (true, request.ResponseBody.Id);
        }
    }
}

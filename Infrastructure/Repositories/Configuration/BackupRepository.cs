#nullable disable

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ESMART.Infrastructure.Repositories.Configuration
{
    public class BackupRepository
    {
        public static string CreateBackup()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");
                var databaseName = "ESMART";

                using SqlConnection conn = new(connectionString);
                conn.Open();

                string defaultBackupDir;
                using (var cmmd = new SqlCommand(
                    @"DECLARE @bdir NVARCHAR(260);
                    EXEC master.dbo.xp_instance_regread
                    N'HKEY_LOCAL_MACHINE',
                    N'Software\Microsoft\MSSQLServer\MSSQLServer',
                    N'BackupDirectory',
                    @bdir OUTPUT;
                    SELECT @bdir;", conn))
                {
                    defaultBackupDir = (string)cmmd.ExecuteScalar();
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                string backupFile = Path.Combine(defaultBackupDir, $"ESMART_{timestamp}.bak");

                string query =
                    $@"BACKUP DATABASE [{databaseName}]
                    TO DISK = N'{backupFile}' 
                    WITH FORMAT, MEDIANAME = 'DbBackups', NAME = 'Full Backup of {databaseName}'";

                using SqlCommand cmd = new(query, conn);
                cmd.ExecuteNonQuery();

                return backupFile;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating the backup: " + ex.Message);
            }
        }

        public static async Task<GoogleDriveUploadResponse> UploadBackupAsync(string filePath, string hotelName)
        {
            using var http = new HttpClient();
            using var form = new MultipartFormDataContent();

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            form.Add(fileContent, "file", Path.GetFileName(filePath)); // ✅ Field name fixed

            http.DefaultRequestHeaders.Add("x-hotel-name", hotelName);

            string url = "https://esmart-api.vercel.app/api/google";
            var response = await http.PostAsync(url, form);

            string json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Upload failed: {response.StatusCode} - {json}");
            }

            return JsonSerializer.Deserialize<GoogleDriveUploadResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
        }


        public static string ZipFiles(string filePath)
        {
            string zipFilePath = Path.Combine(Path.GetDirectoryName(filePath)!, Path.GetFileNameWithoutExtension(filePath) + ".zip");

            using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(filePath, Path.GetFileName(filePath), CompressionLevel.Optimal);
            }

            return zipFilePath;
        }
    }

    public class GoogleDriveUploadResponse
    {
        public bool Success { get; set; }
        public string FileId { get; set; }
        public string Name { get; set; }
        public string ViewLink { get; set; }
        public string DownloadLink { get; set; }
    }

}

#nullable disable

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ESMART.Presentation.Utils
{
    public class PinataUploadResponse
    {
        public bool Success { get; set; }
        public string IpfsHash { get; set; }
        public long PinSize { get; set; }
        public DateTime Timestamp { get; set; }
        public string FileName { get; set; }
        public string Error { get; set; }
    }

    public class PinataUploader
    {
        private readonly HttpClient _http;

        public PinataUploader(string baseUrl)
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public async Task<PinataUploadResponse> UploadFileAsync(string localFilePath)
        {
            if (!File.Exists(localFilePath))
                throw new FileNotFoundException("File to upload not found", localFilePath);

            using var fileStream = File.OpenRead(localFilePath);
            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(fileStream);
            var ext = Path.GetExtension(localFilePath).TrimStart('.').ToLowerInvariant();
            var mime = ext switch
            {
                "png" => "image/png",
                "jpg" or "jpeg" => "image/jpeg",
                "pdf" => "application/pdf",
                _ => "application/octet-stream"
            };

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mime);
            content.Add(fileContent, "file", Path.GetFileName(localFilePath));

            using var resp = await _http.PostAsync("/api/pinata", content);
            string body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                return new PinataUploadResponse
                {
                    Success = false,
                    Error = $"Server returned {(int)resp.StatusCode}: {body}"
                };
            }

            var result = JsonSerializer.Deserialize<PinataUploadResponse>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result!;
        }
    }
}

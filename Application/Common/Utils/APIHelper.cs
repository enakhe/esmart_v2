using System.Text;
using System.Text.Json;

public class ApiService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<HttpResponseMessage> PostAsync<T>(string url, T requestBody)
    {
        try
        {
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            return response;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"An error ocurred when sending code. This might be caused by network related issues: {ex.Message}");
        }
    }
}

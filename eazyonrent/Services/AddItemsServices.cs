
using eazyonrent.Connection;
using eazyonrent.Model;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace eazyonrent.Services
{
    public class AddItemsServices
    {
        private readonly HttpClient _httpClient;
        public AddItemsServices()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(AppSettings.BaseApiUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }
        public async Task<AddItmApiResponse?> AddItem(ListerItem listerItem)
        {
            try
            {
                var url = $"{Endpoints.AddItems}";
                var json = JsonSerializer.Serialize(listerItem);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AddItmApiResponse>();
                    return result;
                }
                else
                {
                    return new AddItmApiResponse
                    {
                        ResponseCode = "999",
                        ResponseMessage = $"Failed: {response.ReasonPhrase}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AddItmApiResponse
                {
                    ResponseCode = "999",
                    ResponseMessage = $"Exception: {ex.Message}"
                };
            }
        }



        // Updated Image Upload Method - Direct file upload

        public async Task<AddItemImagesResponse?> UploadItemImages(
            int listerItemId,
            List<Stream> imageFiles,
            List<string>? fileNames = null)
        {
            try
            {
                var url = $"{Endpoints.AddItmeImages}";
                using var content = new MultipartFormDataContent();

                // Remove hardcoded values or make them dynamic
                content.Add(new StringContent("0"), "ImageId");
                content.Add(new StringContent(listerItemId.ToString()), "ListerItemId");

                string imageName = fileNames != null && fileNames.Count > 0
                    ? string.Join(",", fileNames)
                    : "string";
                content.Add(new StringContent(imageName), "ImageName");

                for (int i = 0; i < imageFiles.Count; i++)
                {
                    var stream = imageFiles[i];
                    if (stream.CanSeek)
                        stream.Position = 0;

                    var streamContent = new StreamContent(stream);

                    string fileName = fileNames != null && i < fileNames.Count
                        ? fileNames[i]
                        : $"upload_{i}.jpg";  

                    string extension = Path.GetExtension(fileName)?.ToLower() ?? ".jpg";
                    string mimeType = extension switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".bmp" => "image/bmp",
                        ".webp" => "image/webp",
                        _ => "image/jpeg"  
                    };

                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
                    content.Add(streamContent, "ImageFiles", fileName);
                }

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AddItemImagesResponse>();
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new AddItemImagesResponse
                    {
                        ResponseCode = "999",
                        ResponseMessage = $"Image Upload Failed: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AddItemImagesResponse
                {
                    ResponseCode = "999",
                    ResponseMessage = $"Image Upload Exception: {ex.Message}"
                };
            }
        }

    }
}



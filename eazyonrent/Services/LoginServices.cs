using eazyonrent.Connection;
using eazyonrent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace eazyonrent.Services
{
    class LoginServices
    {
        private readonly HttpClient _httpClient;
        public LoginServices()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(AppSettings.BaseApiUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }
        public async Task<ListerModel> LoginAsync(string usermobile)
        {
            try
            {
                var url = $"{Endpoints.UserLogin}";
                var json = JsonSerializer.Serialize(new { Mobile = usermobile });
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<ListerModel>(responseBody, options);

                return result ?? new ListerModel
                {
                    ResponseCode = "500",
                    ResponseMessage = "Empty response",
                    ListerId = null,
                    ExistUser = null
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login API Exception: " + ex.Message);
                return new ListerModel
                {
                    ResponseCode = "500",
                    ResponseMessage = "Exception occurred",
                    ListerId = null,
                    ExistUser = null
                };
            }
        }
        



public async Task<ListerModel> UpdateProfileAsync(ListerUpdateRequest request)
    {
        try
        {
            using var form = new MultipartFormDataContent();


            // Normal fields (match server model property names exactly)
            form.Add(new StringContent(request.ListerId?.ToString() ?? ""), "ListerId");
            form.Add(new StringContent(request.CompanyName ?? ""), "CompanyName");
            form.Add(new StringContent(request.Address ?? ""), "Address");
            form.Add(new StringContent(request.Email ?? ""), "Email");
            form.Add(new StringContent(request.Name ?? ""), "Name");
            form.Add(new StringContent(request.City ?? ""), "City");
            form.Add(new StringContent( request.Mobile ?? ""), "Mobile");

            // File upload (safe: read into memory first)
            if (request.ImageFile is FileResult file && !string.IsNullOrWhiteSpace(file.FileName))
            {
                var fileName = Path.GetFileName(file.FileName);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = $"image_{Guid.NewGuid()}";
                }

                // Read file bytes into memory
                using var inStream = await file.OpenReadAsync();
                using var ms = new MemoryStream();
                await inStream.CopyToAsync(ms);
                var bytes = ms.ToArray();

                var fileContent = new ByteArrayContent(bytes);

                // determine mime type by extension (fallback to octet-stream)
                var ext = Path.GetExtension(fileName)?.ToLowerInvariant() ?? ".jpg";
                var mimeType = ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    ".bmp" => "image/bmp",
                    _ => "application/octet-stream"
                };

                fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
                // optional: explicit content-disposition
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"ImageFile\"",
                    FileName = $"\"{fileName}\""
                };

                // name must match API property: "ImageFile"
                form.Add(fileContent, "ImageFile", fileName);
            }

            // Send request
            var response = await _httpClient.PostAsync(Endpoints.EditProfile, form);

            // Read response body for both success and error (helps debugging)
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Log full error details for debugging (console / debug window)
                Console.WriteLine($"UpdateProfile failed: {(int)response.StatusCode} {response.ReasonPhrase}");
                Console.WriteLine("Response body: " + responseBody);

                return new ListerModel
                {
                    ResponseCode = ((int)response.StatusCode).ToString(),
                    ResponseMessage = !string.IsNullOrWhiteSpace(responseBody) ? responseBody : response.ReasonPhrase
                };
            }

            // Deserialize success response
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<ListerModel>(responseBody, options);

            return result ?? new ListerModel
            {
                ResponseCode = "500",
                ResponseMessage = "Empty response",
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("UpdateProfile API Exception: " + ex.ToString());
            return new ListerModel
            {
                ResponseCode = "500",
                ResponseMessage = "Exception occurred: " + ex.Message
            };
        }
    }



    public async Task<ListerItemProfileResponse> OnLoadProfileItem(int? listerId)
        {
            try
            {
                var url = $"{Endpoints.ProfileItem}?ListerId={listerId}";

                var response = await _httpClient.GetAsync(url); 
                response.EnsureSuccessStatusCode();  

                var responseBody = await response.Content.ReadAsStringAsync();     


                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<ListerItemProfileResponse>(responseBody, options);
                return result ?? new ListerItemProfileResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Empty response",
                    ItemList = new List<ListerItemProfileResult>()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Profile API Exception: " + ex.Message);
                return new ListerItemProfileResponse
                {
                    ResponseCode = "500",
                    ResponseMessage = "Exception occurred",
                    ItemList = new List<ListerItemProfileResult>()
                };
            }
        }

        public async Task<BookedItemHistoryResponse> GetBookingHistoryAsync(int? listerId)
        {
            try
            {
                if (!listerId.HasValue)
                {
                    return new BookedItemHistoryResponse
                    {
                        responseCode = "400",
                        responseMessage = "ListerId is required"
                    };
                }

                var url = $"{Endpoints.HistoryItme}?ListerId={listerId}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // JsonSerializerOptions use karein jo case-insensitive ho
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var historyResponse = JsonSerializer.Deserialize<BookedItemHistoryResponse>(content, options);
                    return historyResponse ?? new BookedItemHistoryResponse
                    {
                        responseCode = "500",
                        responseMessage = "Failed to deserialize response"
                    };
                }

                return new BookedItemHistoryResponse
                {
                    responseCode = ((int)response.StatusCode).ToString(),
                    responseMessage = "Failed to load history"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading booking history: {ex.Message}");
                return new BookedItemHistoryResponse
                {
                    responseCode = "500",
                    responseMessage = ex.Message
                };
            }
        }

    }
}

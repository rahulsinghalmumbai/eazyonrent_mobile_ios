using eazyonrent.Connection;
using eazyonrent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;


namespace eazyonrent.Services
{
    public class GuestServices
    {
        private readonly HttpClient _httpClient;
        public GuestServices()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(AppSettings.BaseApiUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }
        public async Task<ApiResponseItem<List<ListerItemResult>>?> GetGuestItemsAsync()
        {
            try
            {
                var url = $"{Endpoints.GetGuestItem}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var jsonContent = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonSerializer.Deserialize<ApiResponseItem<List<ListerItemResult>>>(
                    jsonContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                
                return apiResponse;
            }
            catch
            {
                return null;
            }
        }
        public async Task<ApiResponseCat<List<Categorie>>?> GetAllCategoriesAsync()
        {
            try
            {
                //var response = await _httpClient.GetAsync("Categories/GellALLCategories");
                var url = $"{Endpoints.GetAllCat}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var jsonContent = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonSerializer.Deserialize<ApiResponseCat<List<Categorie>>>(
                    jsonContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return apiResponse;
            }
            catch
            {
                return null;
            }
        }
        public async Task<ApiResponseItem<List<ListerItemResult>>?> GetItemDetailsAsync(int listerId, int itemId)
        {
            try
            {
                var url = $"{Endpoints.GetItemDetailsById}?ListerId={listerId}&ItemId={itemId}";
                var response = await _httpClient.GetStringAsync(url);

                // Directly deserialize to single object
                var apiResponse = JsonSerializer.Deserialize<ApiResponseItem<List<ListerItemResult>>>(response,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse == null)
                    return null;

                // Agar aapko convert karna hai toh kar lo
                //var result = new ListerItemResult
                //{
                //    ListerItemId = apiItem.ListerItemId,
                //    ItemName = apiItem.ItemName,
                //    CompanyName = apiItem.CompanyName,
                //    ListerId = apiItem.ListerId,
                //    viewCount = apiItem.viewCount,
                //    ItemCost = apiItem.ItemCost,
                //    ItemDescriptions = apiItem.ItemDescriptions,
                //    Availablefrom = apiItem.Availablefrom,
                //    Status = apiItem.Status,
                //    CategoryId = apiItem.CategoryId,
                //    Images = apiItem.Images ?? new List<string>(),
                //    Location = apiItem.Location ?? "Noida" // default if null
                //};

                return apiResponse;
            }
            catch (Exception ex)
            {
                // Debug ke liye
                Console.WriteLine($"Error in GetItemDetailsAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<ApiResponseItem<List<ListerItemResult>>> LoadSimilarItemsAsync(int? cat)
        {
            try
            {
                var url = $"{Endpoints.SimilarItems}?CategoryId={cat}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var jsonContent = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonSerializer.Deserialize<ApiResponseItem<List<ListerItemResult>>>(
                    jsonContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return apiResponse;
            }
            catch
            {
                return null;
            }
        }

    }

}

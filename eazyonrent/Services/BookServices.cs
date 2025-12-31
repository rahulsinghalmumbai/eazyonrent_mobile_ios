using eazyonrent.Connection;
using eazyonrent.Model;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace eazyonrent.Services
{
    public class BookServices : IDisposable
    {
        private readonly HttpClient _httpClient;

        public BookServices()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(AppSettings.BaseApiUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<BookingResponse> FinalBooking(RenterItem item)
        {
            try
            {
                var url = $"{Endpoints.BookItem}";

                // Serialize request body
                var json = JsonSerializer.Serialize(item, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize API response to BookingResponse
                    var bookingResult = JsonSerializer.Deserialize<BookingResponse>(responseBody, options);

                    if (bookingResult != null)
                    {
                        //bookingResult.IsSuccess = true;
                        return bookingResult;
                    }

                    // fallback
                    return new BookingResponse
                    {
                        
                        ResponseCode = "200",
                        ResponseMessage = "Booking successful",
                        Data = null
                    };
                }
                else
                {
                    return new BookingResponse
                    {
                        
                        ResponseCode = response.StatusCode.ToString(),
                        ResponseMessage = $"API Error: {response.ReasonPhrase}. Response: {responseBody}",
                        Data = null
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine("Booking API HTTP Exception: " + httpEx.Message);
                return new BookingResponse
                {
                   
                    ResponseCode = "HttpError",
                    ResponseMessage = "Network error occurred. Please check your connection.",
                    Data = null
                };
            }
            catch (TaskCanceledException timeoutEx)
            {
                Console.WriteLine("Booking API Timeout Exception: " + timeoutEx.Message);
                return new BookingResponse
                {
                 
                    ResponseCode = "Timeout",
                    ResponseMessage = "Request timeout. Please try again.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Booking API Exception: " + ex.Message);
                return new BookingResponse
                {
                   
                    ResponseCode = "500",
                    ResponseMessage = "An unexpected error occurred. Please try again.",
                    Data = null
                };
            }
        }

        // Optional: Get booking status
        public async Task<BookingResponse> GetBookingStatus(int bookingId)
        {
            try
            {
                var url = $"{Endpoints.BookItem}/{bookingId}";
                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (response.IsSuccessStatusCode)
                {
                    var bookingResult = JsonSerializer.Deserialize<BookingResponse>(responseBody, options);

                    if (bookingResult != null)
                    {
                        //bookingResult.IsSuccess = true;
                        return bookingResult;
                    }

                    return new BookingResponse
                    {
                      
                        ResponseCode = "200",
                        ResponseMessage = "Booking retrieved successfully",
                        Data = null
                    };
                }
                else
                {
                    return new BookingResponse
                    {
                       
                        ResponseCode = response.StatusCode.ToString(),
                        ResponseMessage = $"Failed to retrieve booking: {response.ReasonPhrase}",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get Booking Status Exception: " + ex.Message);
                return new BookingResponse
                {
                    
                    ResponseCode = "500",
                    ResponseMessage = "An error occurred while retrieving booking status.",
                    Data = null
                };
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // Response model for booking operations
    public class BookingResponse
    {
       
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public RenterItem Data { get; set; }
    }
}

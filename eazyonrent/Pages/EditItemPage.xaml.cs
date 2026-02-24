using eazyonrent.Connection;
using eazyonrent.Model;
using eazyonrent.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace eazyonrent.Pages;

public partial class EditItemPage : ContentPage
{
    private readonly int _listerItemId;
    private readonly int _listerId;
    private readonly HttpClient _httpClient;
    private readonly GuestServices _guestServices;
    public List<Categorie> Categories { get; set; } = new();

    public EditItemPage(int listerItemId, int listerId)
    {
        InitializeComponent();
        _listerItemId = listerItemId;
        _httpClient = new HttpClient();
        _listerId = listerId;
        _guestServices = new GuestServices();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCategoriesAsync();
        await LoadItemDetails();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var apiResponse = await _guestServices.GetAllCategoriesAsync();
            if (apiResponse != null && apiResponse.ResponseCode == "000" && apiResponse.CategoriesList != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Categories.Clear();

                    // ✅ Yeh line uncomment karo — bina iske list empty rahegi
                    foreach (var category in apiResponse.CategoriesList)
                        Categories.Add(category);

                    CategoryPicker.ItemsSource = Categories;
                    CategoryPicker.ItemDisplayBinding = new Binding("CategoriesName");

                    System.Diagnostics.Debug.WriteLine($"Categories loaded: {Categories.Count}");
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadCategoriesAsync Error: {ex.Message}");
        }
    }


    private async Task LoadItemDetails()
    {
        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            UpdateButton.IsEnabled = false;

            // ✅ GuestServices se call karo (HttpClient hatao)
            var response = await _guestServices.GetItemDetailsAsync(_listerId, _listerItemId);

            if (response?.ItemList == null || !response.ItemList.Any())
            {
                await DisplayAlert("Error", "No item details found.", "OK");
                return;
            }

            var item = response.ItemList.FirstOrDefault();

            if (item == null)
            {
                await DisplayAlert("Error", "No item details available.", "OK");
                return;
            }

            // ✅ Bind all fields (NO image code)
            NameEntry.Text = item.ItemName ?? "";
            CompanyNameEntry.Text = item.CompanyName ?? "";
            PriceEntry.Text = item.ItemCost.ToString();
            DescriptionEntry.Text = item.ItemDescriptions ?? "";

            // Date
            if (item.Availablefrom.HasValue)
                AvailableFromPicker.Date = item.Availablefrom.Value;

            if (item.Status.HasValue)
            {
                StatusPicker.SelectedItem = item.Status.Value switch
                {
                    1 => "Active",
                    2 => "Inactive",
                    3 => "Pending",
                    _ => "Active"
                };
            }
            else
            {
                StatusPicker.SelectedItem = "Active"; 
            }

            var matchedCat = Categories.FirstOrDefault(c => c.Id == item.CategoryId);
            if (matchedCat != null)
                CategoryPicker.SelectedItem = matchedCat;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load item: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            UpdateButton.IsEnabled = true;
        }
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        try
        {
            // ✅ Validation
            if (string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                await DisplayAlert("Validation", "Item name required.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(PriceEntry.Text))
            {
                await DisplayAlert("Validation", "Price required.", "OK");
                return;
            }

            UpdateButton.IsEnabled = false;
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            // ✅ Selected Category se CategoryId lo
            var selectedCat = CategoryPicker.SelectedItem as Categorie;

            var payload = new
            {
                listerItemId = _listerItemId,
                itemName = NameEntry.Text?.Trim(),
                listerId = _listerId,
                itemCost = decimal.TryParse(PriceEntry.Text, out var cost) ? cost : 0,
                itemDescriptions = DescriptionEntry.Text?.Trim(),
                // ✅ Alternative fix
                availablefrom = ((DateTime)AvailableFromPicker.Date).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                status = GetStatusValue(),   
                availabilityType = true,
                createdDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                updatedOn = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                categoryId = selectedCat?.Id ?? 0
            };

            var url = $"{AppSettings.BaseApiUrl}{Endpoints.UpdateItem}?ListerItemId={_listerItemId}&ListerId={_listerId}";

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content); 

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Item updated successfully!", "OK");
                await Navigation.PopAsync(); 
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"Update failed: {error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Something went wrong: {ex.Message}", "OK");
        }
        finally
        {
            UpdateButton.IsEnabled = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
    private int GetStatusValue()
    {
        return StatusPicker.SelectedIndex switch
        {
            0 => 1,
            1 => 2,
            2 => 3,
            _ => 1
        };
    }
}


//public class ItemDetailResponse
//{
//    public List<ItemListModel> ItemList { get; set; }
//}

//public class ItemListModel
//{
//    public int ListerItemId { get; set; }
//    public string? ItemName { get; set; }
//    public string? CompanyName { get; set; }
//    public int ListerId { get; set; }
//    public decimal ItemCost { get; set; }
//    public string? ItemDescriptions { get; set; }
//    public string? Availablefrom { get; set; }
//    public string? Status { get; set; }
//    public int CategoryId { get; set; }
//    public string? CategoryName { get; set; }
//}

using eazyonrent.Model;
using eazyonrent.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace eazyonrent.Pages;

public partial class AddItemPage : ContentPage, INotifyPropertyChanged
{
    private readonly HttpClient _httpClient;
    private ObservableCollection<Categorie> _categories;
    private Categorie _selectedCategory;
    private readonly GuestServices _guestServices;
    private readonly AddItemsServices addItemsServices;


    private List<Stream> _selectedImageStreams = new List<Stream>();
    private List<string> _selectedImageNames = new List<string>();
    //private readonly AddItemsServices addItemsServices;
    public ObservableCollection<Categorie> Categories
    {
        get => _categories;
        set
        {
            _categories = value;
            OnPropertyChanged();
        }
    }

    public Categorie SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            _selectedCategory = value;
            OnPropertyChanged();
        }
    }

    public AddItemPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
        _guestServices = new GuestServices();
        addItemsServices = new AddItemsServices();
        InitializeForm();
        BindingContext = this;

        // Load categories after UI is initialized
        Loaded += async (s, e) => await LoadCategoriesAsync();
    }

    private void InitializeForm()
    {
        // Initialize ObservableCollection first
        Categories = new ObservableCollection<Categorie>();

        // Set default values
        AvailableFromPicker.Date = DateTime.Today;
        StatusPicker.SelectedIndex = 0;
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            // Show loading if needed
            // await DisplayAlert("Info", "Loading categories...", "OK");

            var apiResponse = await _guestServices.GetAllCategoriesAsync();

            if (apiResponse != null && apiResponse.ResponseCode == "000" && apiResponse.CategoriesList != null)
            {
                // Clear and add on UI thread
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Categories.Clear();
                    foreach (var category in apiResponse.CategoriesList)
                    {
                        Categories.Add(category);
                    }

                    // Debug: Check if categories loaded
                    System.Diagnostics.Debug.WriteLine($"Categories loaded: {Categories.Count}");

                    // Set default category if available
                    if (Categories.Any())
                    {
                        SelectedCategory = Categories.First();
                    }
                });
            }
            else
            {
                // Load fallback if API fails
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LoadFallbackCategories();
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadCategoriesAsync Error: {ex.Message}");
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                LoadFallbackCategories();
            });
        }
    }

    private void LoadFallbackCategories()
    {
        Categories.Clear();
        Categories.Add(new Categorie { Id = 1, CategoriesName = "Laptop/Desktop", Status = true });
        Categories.Add(new Categorie { Id = 2, CategoriesName = "Others", Status = true });
        Categories.Add(new Categorie { Id = 3, CategoriesName = "Drone", Status = true });
        Categories.Add(new Categorie { Id = 4, CategoriesName = "Clothes", Status = true });

        // Set default selection
        if (Categories.Any())
        {
            SelectedCategory = Categories.First();
        }

        System.Diagnostics.Debug.WriteLine($"Fallback categories loaded: {Categories.Count}");
    }

    private async void OnCameraClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await DisplayActionSheet("Select Image", "Cancel", null, "Camera", "Gallery");

            switch (result)
            {
                case "Camera":
                    await TakePhoto();
                    break;
                case "Gallery":
                    await PickSinglePhoto();
                    break;
               
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Camera error: {ex.Message}", "OK");
        }
    }

    
    private async Task TakePhoto()
    {
        try
        {
            // Check if camera is available
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await DisplayAlert("Error", "Camera not supported on this device", "OK");
                return;
            }

            // Request camera permission
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Camera permission is required to take photos", "OK");
                return;
            }

            // Capture photo
            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Take a photo"
            });

            if (photo != null)
            {
                await ProcessSelectedImage(photo);
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Error", "Camera feature not supported on this device", "OK");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Permission Error", "Camera permission is required", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to take photo: {ex.Message}", "OK");
        }
    }
    private async Task PickSinglePhoto()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Denied", "Storage permission is required to access gallery", "OK");
                return;
            }

            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select a photo"
            });

            if (photo != null)
            {
                await ProcessSelectedImage(photo);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to pick photo: {ex.Message}", "OK");
        }
    }

    private async Task ProcessSelectedImage(FileResult photo)
    {
        try
        {
            var stream = await photo.OpenReadAsync();

            // Store stream and filename directly
            _selectedImageStreams.Add(stream);
            _selectedImageNames.Add(photo.FileName);

            await DisplayAlert("Success",
                $"Image added successfully!\nTotal images: {_selectedImageStreams.Count}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to process image: {ex.Message}", "OK");
        }
    }



    private async void OnSaveClicked(object sender, EventArgs e) 
    {
        try
        {
            // Show loading indicator
            SaveButton.Text = "Saving...";
            SaveButton.IsEnabled = false;

            // Validate inputs
            if (!await ValidateInputs())
            {
                SaveButton.Text = "Save Item";
                SaveButton.IsEnabled = true;
                return;
            }

            // Get selected category ID
            int categoryId = SelectedCategory?.Id ?? 1;
            int listerId = 0;

            // First time login id
            var listerIdFirst = await SecureStorage.GetAsync("ListerIdFirst");
            // Regular id
            var listerIdNormal = await SecureStorage.GetAsync("ListerId");

            if (!string.IsNullOrEmpty(listerIdFirst))
            {
                // agar first time wala id exist karta hai use karo
                listerId = int.Parse(listerIdFirst);
                // ek baar use karne ke baad hata bhi sakte ho (optional)
                await SecureStorage.SetAsync("ListerIdFirst", "");
            }
            else if (!string.IsNullOrEmpty(listerIdNormal))
            {
                // otherwise normal wala use karo
                listerId = int.Parse(listerIdNormal);
            }

            // Create ListerItem object
            var listerItem = new ListerItem
            {
                ListerItemId = 0,
                ItemName = NameEntry.Text?.Trim(),
                ListerId = listerId,
                ItemCost = decimal.Parse(PriceEntry.Text),
                ItemDescriptions = HomeCostEntry.Text?.Trim(),
                Availablefrom = AvailableFromPicker.Date,
                Status = GetStatusValue(),
                AvailabilityType = true,
                CreatedDate = DateTime.Now,
                CategoryId = categoryId
            };

            // Call API to save item
            var response = await addItemsServices.AddItem(listerItem);

            await SecureStorage.SetAsync("responseListerItemId",response.ListerItemId.ToString());
            if (response != null && response.ResponseCode == "000")
            {

                if (_selectedImageStreams.Count > 0)
                {
                    SaveButton.Text = "Uploading Images...";

                    // Get ListerItemId from SecureStorage
                    var savedListerItemId = await SecureStorage.GetAsync("responseListerItemId");
                    if (!string.IsNullOrEmpty(savedListerItemId))
                    {
                        await UploadSelectedImages(int.Parse(savedListerItemId));
                    }
                }
                    await DisplayAlert("Success", "Item saved successfully!", "OK");
                ClearForm();
            }
            else
            {
                string msg = response?.ResponseMessage ?? "Unknown error occurred.";
                await DisplayAlert("Error", msg, "OK");
            }
        }
        catch (Exception ex)
        {
               await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            SaveButton.Text = "Save Item";
            SaveButton.IsEnabled = true;
        }
    }
    // Upload images using stored ListerItemId
    private async Task UploadSelectedImages(int listerItemId)
    {
        try
        {
            if (_selectedImageStreams.Count > 0)
            {
                var uploadResponse = await addItemsServices.UploadItemImages(
                    listerItemId: listerItemId,
                    imageFiles: _selectedImageStreams,
                    fileNames: _selectedImageNames  // IMPORTANT: Pass fileNames
                );

                if (uploadResponse?.ResponseCode != "000")
                {
                    await DisplayAlert("Warning",
                        $"Item saved but image upload failed: {uploadResponse?.ResponseMessage}", "OK");
                }
                else
                {
                    await DisplayAlert("Success", "Images uploaded successfully!", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Warning",
                $"Item saved but image upload failed: {ex.Message}", "OK");
        }
    }

    private async Task<bool> ValidateInputs()
    {
        // Validate Category Selection
        if (SelectedCategory == null)
        {
            await DisplayAlert("Validation Error", "Please select a category", "OK");
            return false;
        }

        // Validate Item Name
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
           await DisplayAlert("Validation Error", "Please enter item name", "OK");
            return false;
        }

        // Validate Price
        if (string.IsNullOrWhiteSpace(PriceEntry.Text))
        {
           await DisplayAlert("Validation Error", "Please enter price", "OK");
            return false;
        }

        if (!decimal.TryParse(PriceEntry.Text, out decimal price) || price <= 0)
        {
            await DisplayAlert("Validation Error", "Please enter valid price", "OK");
            return false;
        }

        // Validate Status Selection
        if (StatusPicker.SelectedIndex == -1)
        {
            await DisplayAlert("Validation Error", "Please select status", "OK");
            return false;
        }

        // Validate Available From Date
        if (AvailableFromPicker.Date < DateTime.Today)
        {
            await DisplayAlert("Validation Error", "Available from date cannot be in the past", "OK");
            return false;
        }

        return true;
    }

    private int GetStatusValue()
    {
        return StatusPicker.SelectedIndex switch
        {
            0 => 1, 
            1 => 0, 
            2 => 2, 
            _ => 1  
        };
    }

    //private int GetCurrentUserId()
    //{
    //    return 1; // Default value for now
    //}

    //private async Task<bool> SaveItemToAPI(ListerItem item)
    //{
    //    try
    //    {
    //        var apiEndpoint = "https://your-api-endpoint.com/api/listeritems";

    //        var options = new JsonSerializerOptions
    //        {
    //            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //            WriteIndented = true
    //        };

    //        var json = JsonSerializer.Serialize(item, options);
    //        var content = new StringContent(json, Encoding.UTF8, "application/json");

    //        var response = await _httpClient.PostAsync(apiEndpoint, content);

    //        if (response.IsSuccessStatusCode)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            var errorContent = await response.Content.ReadAsStringAsync();
    //            await DisplayAlert("API Error", $"Failed to save item: {response.StatusCode}", "OK");
    //            return false;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        await DisplayAlert("Error", $"Unexpected error: {ex.Message}", "OK");
    //        return false;
    //    }
    //}

    private void ClearForm()
    {
        NameEntry.Text = string.Empty;
        PriceEntry.Text = string.Empty;
        HomeCostEntry.Text = string.Empty;
        AvailableFromPicker.Date = DateTime.Today;
        StatusPicker.SelectedIndex = 0;

        // Reset to first category
        if (Categories.Any())
        {
            SelectedCategory = Categories.First();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _httpClient?.Dispose();
    }

    // INotifyPropertyChanged implementation
    public new event PropertyChangedEventHandler PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
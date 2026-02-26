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
    private List<string> _selectedImagePaths = new List<string>();
    private int _replaceIndex = -1;


    public ObservableCollection<Categorie> Categories
    {
        get => _categories;
        set { _categories = value; OnPropertyChanged(); }
    }

    public Categorie SelectedCategory
    {
        get => _selectedCategory;
        set { _selectedCategory = value; OnPropertyChanged(); }
    }


    public AddItemPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
        _guestServices = new GuestServices();
        addItemsServices = new AddItemsServices();
        InitializeForm();
        BindingContext = this;

        Loaded += async (s, e) => await LoadCategoriesAsync();
    }

    private void InitializeForm()
    {
        Categories = new ObservableCollection<Categorie>();
        AvailableFromPicker.Date = DateTime.Today;
        StatusPicker.SelectedIndex = 0;
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
                    foreach (var category in apiResponse.CategoriesList)
                        Categories.Add(category);

                    System.Diagnostics.Debug.WriteLine($"Categories loaded: {Categories.Count}");

                    if (Categories.Any())
                        SelectedCategory = Categories.First();
                });
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(LoadFallbackCategories);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadCategoriesAsync Error: {ex.Message}");
            await MainThread.InvokeOnMainThreadAsync(LoadFallbackCategories);
        }
    }

    private void LoadFallbackCategories()
    {
        Categories.Clear();
        Categories.Add(new Categorie { Id = 1, CategoriesName = "Laptop/Desktop", Status = true });
        Categories.Add(new Categorie { Id = 2, CategoriesName = "Others", Status = true });
        Categories.Add(new Categorie { Id = 3, CategoriesName = "Drone", Status = true });
        Categories.Add(new Categorie { Id = 4, CategoriesName = "Clothes", Status = true });

        if (Categories.Any())
            SelectedCategory = Categories.First();

        System.Diagnostics.Debug.WriteLine($"Fallback categories loaded: {Categories.Count}");
    }


    private async void OnCameraClicked(object sender, EventArgs e)
    {
        _replaceIndex = -1; // Add mode
        await OpenImagePicker();
    }


    private async void OnReplaceImageClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int index)
        {
            _replaceIndex = index;
            await OpenImagePicker();
        }
    }


    private void OnDeleteImageClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is int index)
        {
            if (index >= 0 && index < _selectedImageStreams.Count)
            {
                _selectedImageStreams[index]?.Dispose();
                _selectedImageStreams.RemoveAt(index);
                _selectedImageNames.RemoveAt(index);
                _selectedImagePaths.RemoveAt(index);
            }

            RefreshImagePreviewUI();
        }
    }


    private async Task OpenImagePicker()
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
            await DisplayAlert("Error", $"Image picker error: {ex.Message}", "OK");
        }
    }

    private async Task TakePhoto()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await DisplayAlert("Error", "Camera not supported on this device", "OK");
                return;
            }

            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                bool open = await DisplayAlert(
                    "Permission Denied",
                    "Camera permission is required to take photos. Open Settings to enable?",
                    "Open Settings",
                    "Cancel");

                if (open)
                    Microsoft.Maui.ApplicationModel.AppInfo.ShowSettingsUI();

                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Take a photo"
            });

            if (photo != null)
                await ProcessSelectedImage(photo);
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
                await ProcessSelectedImage(photo);
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
            var uploadStream = await photo.OpenReadAsync();

            if (_replaceIndex >= 0 && _replaceIndex < _selectedImageStreams.Count)
            {
                // REPLACE MODE — usi index ki image replace karo
                _selectedImageStreams[_replaceIndex]?.Dispose();
                _selectedImageStreams[_replaceIndex] = uploadStream;
                _selectedImageNames[_replaceIndex] = photo.FileName;
                _selectedImagePaths[_replaceIndex] = photo.FullPath;
                _replaceIndex = -1;
            }
            else
            {
                // ADD MODE — list ke end mein add karo
                _selectedImageStreams.Add(uploadStream);
                _selectedImageNames.Add(photo.FileName);
                _selectedImagePaths.Add(photo.FullPath);
            }

            await MainThread.InvokeOnMainThreadAsync(RefreshImagePreviewUI);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to process image: {ex.Message}", "OK");
        }
    }

    private void RefreshImagePreviewUI()
    {
        // Puri strip clear karo
        ImagePreviewStack.Children.Clear();

        int count = _selectedImagePaths.Count;

        if (count == 0)
        {
            EmptyImagePlaceholder.IsVisible = true;
            ImageScrollView.IsVisible = false;
            ImageCountBadge.IsVisible = false;
            return;
        }

        // Images hain
        EmptyImagePlaceholder.IsVisible = false;
        ImageScrollView.IsVisible = true;
        ImageCountBadge.IsVisible = true;
        ImageCountLabel.Text = $"{count} image{(count > 1 ? "s" : "")} selected";

        for (int i = 0; i < count; i++)
        {
            int capturedIndex = i;
            string path = _selectedImagePaths[i];

            // ── Outer Grid container ──────────────────────────────────────
            var container = new Grid
            {
                WidthRequest = 110,
                HeightRequest = 120,
                Margin = new Thickness(0, 4, 0, 4)
            };

            // ── Image Frame ───────────────────────────────────────────────
            var imageFrame = new Frame
            {
                CornerRadius = 10,
                Padding = new Thickness(0),
                HasShadow = true,
                IsClippedToBounds = true,
                HeightRequest = 100,
                WidthRequest = 110,
                VerticalOptions = LayoutOptions.Start,
                BackgroundColor = Color.FromArgb("#E0E0E0")
            };

            var img = new Image
            {
                Aspect = Aspect.AspectFill,
                HeightRequest = 100,
                WidthRequest = 110
            };

            try
            {
                var previewStream = File.OpenRead(path);
                img.Source = ImageSource.FromStream(() => previewStream);
            }
            catch
            {
                img.Source = "placeholder.png"; 
            }

            imageFrame.Content = img;

            var numberFrame = new Frame
            {
                BackgroundColor = Color.FromArgb("#4a6fc7"),
                CornerRadius = 10,
                Padding = new Thickness(6, 2),
                HasShadow = false,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(4, 4, 0, 0)
            };
            numberFrame.Content = new Label
            {
                Text = $"{i + 1}",
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            };

            var deleteBtn = new Button
            {
                Text = "✕",
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Color.FromArgb("#E53935"),
                TextColor = Colors.White,
                CornerRadius = 12,
                WidthRequest = 26,
                HeightRequest = 26,
                Padding = new Thickness(0),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 4, 4, 0),
                CommandParameter = capturedIndex
            };
            deleteBtn.Clicked += OnDeleteImageClicked;

            // ── Replace button (🔄 Change) — bottom strip ─────────────────
            var replaceBtn = new Button
            {
                Text = "🔄 Change",
                FontSize = 10,
                BackgroundColor = Color.FromArgb("#CC000000"),
                TextColor = Colors.White,
                CornerRadius = 0,
                HeightRequest = 28,
                Padding = new Thickness(0),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.End,
                CommandParameter = capturedIndex
            };
            replaceBtn.Clicked += OnReplaceImageClicked;

            container.Children.Add(imageFrame);
            container.Children.Add(replaceBtn);
            container.Children.Add(numberFrame);
            container.Children.Add(deleteBtn);

            ImagePreviewStack.Children.Add(container);
        }
        var addMoreFrame = new Frame
        {
            WidthRequest = 90,
            HeightRequest = 100,
            CornerRadius = 10,
            BackgroundColor = Color.FromArgb("#E8EAF6"),
            BorderColor = Color.FromArgb("#4a6fc7"),
            HasShadow = false,
            Margin = new Thickness(0, 4, 0, 4),
            VerticalOptions = LayoutOptions.Start
        };

        var addMoreStack = new StackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 4
        };
        addMoreStack.Children.Add(new Label
        {
            Text = "＋",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#4a6fc7"),
            HorizontalOptions = LayoutOptions.Center
        });
        addMoreStack.Children.Add(new Label
        {
            Text = "Add More",
            FontSize = 11,
            TextColor = Color.FromArgb("#4a6fc7"),
            HorizontalOptions = LayoutOptions.Center
        });

        addMoreFrame.Content = addMoreStack;
        addMoreFrame.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                _replaceIndex = -1;
                await OpenImagePicker();
            })
        });

        ImagePreviewStack.Children.Add(addMoreFrame);
    }


    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            SaveButton.Text = "Saving...";
            SaveButton.IsEnabled = false;

            if (!await ValidateInputs())
            {
                SaveButton.Text = "Save Item";
                SaveButton.IsEnabled = true;
                return;
            }

            int categoryId = SelectedCategory?.Id ?? 1;
            int listerId = 0;

            var listerIdFirst = await SecureStorage.GetAsync("ListerIdFirst");
            var listerIdNormal = await SecureStorage.GetAsync("ListerId");

            if (!string.IsNullOrEmpty(listerIdFirst))
            {
                listerId = int.Parse(listerIdFirst);
                await SecureStorage.SetAsync("ListerIdFirst", "");
            }
            else if (!string.IsNullOrEmpty(listerIdNormal))
            {
                listerId = int.Parse(listerIdNormal);
            }

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

            var response = await addItemsServices.AddItem(listerItem);
            await SecureStorage.SetAsync("responseListerItemId", response.ListerItemId.ToString());

            if (response != null && response.ResponseCode == "000")
            {
                if (_selectedImageStreams.Count > 0)
                {
                    SaveButton.Text = "Uploading Images...";

                    var savedListerItemId = await SecureStorage.GetAsync("responseListerItemId");
                    if (!string.IsNullOrEmpty(savedListerItemId))
                        await UploadSelectedImages(int.Parse(savedListerItemId));
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

    private async Task UploadSelectedImages(int listerItemId)
    {
        try
        {
            if (_selectedImageStreams.Count > 0)
            {
                var uploadResponse = await addItemsServices.UploadItemImages(
                    listerItemId: listerItemId,
                    imageFiles: _selectedImageStreams,
                    fileNames: _selectedImageNames
                );

                if (uploadResponse?.ResponseCode != "000")
                    await DisplayAlert("Warning", $"Item saved but image upload failed: {uploadResponse?.ResponseMessage}", "OK");
                else
                    await DisplayAlert("Success", "Images uploaded successfully!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Warning", $"Item saved but image upload failed: {ex.Message}", "OK");
        }
    }

    private async Task<bool> ValidateInputs()
    {
        if (SelectedCategory == null)
        {
            await DisplayAlert("Validation Error", "Please select a category", "OK");
            return false;
        }
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Validation Error", "Please enter item name", "OK");
            return false;
        }
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
        if (StatusPicker.SelectedIndex == -1)
        {
            await DisplayAlert("Validation Error", "Please select status", "OK");
            return false;
        }
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
            1 => 2,
            2 => 3,
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

        // Image lists clear karo
        foreach (var s in _selectedImageStreams) s?.Dispose();
        _selectedImageStreams.Clear();
        _selectedImageNames.Clear();
        _selectedImagePaths.Clear();
        _replaceIndex = -1;

        // UI reset
        RefreshImagePreviewUI();

        if (Categories.Any())
            SelectedCategory = Categories.First();
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

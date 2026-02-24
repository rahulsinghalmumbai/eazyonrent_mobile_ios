using eazyonrent.Connection;
using eazyonrent.Model;
using eazyonrent.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace eazyonrent.Pages;

public partial class EditItemPage : ContentPage, INotifyPropertyChanged
{
    private readonly int _listerItemId;
    private readonly int _listerId;
    private readonly HttpClient _httpClient;
    private readonly GuestServices _guestServices;
    private readonly AddItemsServices _addItemsServices;
   

    private ObservableCollection<Categorie> _categories = new();
    private Categorie _selectedCategory;

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

    private List<ItemImageResult> _existingImages = new();
    private List<int?> _selectedImageIds = new();
    private List<Stream> _selectedImageStreams = new();
    private List<string> _selectedImageNames = new();
    private List<string> _selectedImagePaths = new();
    private List<(bool IsExisting, string Path, int? ImageId)> _displayImages = new();
    private ObservableCollection<ImageDisplayItem> _imageItems = new();
    private int _replaceIndex = -1;

    public EditItemPage(int listerItemId, int listerId)
    {
        InitializeComponent();
        _listerItemId = listerItemId;
        _listerId = listerId;
        _httpClient = new HttpClient();
        _guestServices = new GuestServices();
        _addItemsServices = new AddItemsServices();
        BindingContext = this;
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
            if (apiResponse?.ResponseCode == "000" && apiResponse.CategoriesList != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Categories.Clear();
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

            // ✅ Text fields
            NameEntry.Text = item.ItemName ?? "";
            CompanyNameEntry.Text = item.CompanyName ?? "";
            PriceEntry.Text = item.ItemCost.ToString();
            DescriptionEntry.Text = item.ItemDescriptions ?? "";

            // ✅ Date
            if (item.Availablefrom.HasValue)
                AvailableFromPicker.Date = item.Availablefrom.Value;

            // ✅ Status
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

            // ✅ Category
            var matchedCat = Categories.FirstOrDefault(c => c.Id == item.CategoryId.GetValueOrDefault());
            if (matchedCat != null)
                CategoryPicker.SelectedItem = matchedCat;

            // ✅ Images load karo
            _existingImages.Clear();
            _displayImages.Clear();

            if (item.ItemImageList?.Any() == true)
            {
                foreach (var img in item.ItemImageList)
                {
                    if (!string.IsNullOrEmpty(img.ImageName))
                    {
                        _existingImages.Add(new ItemImageResult
                        {
                            ImageId = img.ImageId,
                            ListerItemId = img.ListerItemId,
                            ImageName = img.ImageName
                        });

                        _displayImages.Add((IsExisting: true, Path: img.ImageName, ImageId: img.ImageId));
                    }
                }
            }

            await MainThread.InvokeOnMainThreadAsync(RefreshImagePreviewUI);
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

    private void RefreshImagePreviewUI()
    {
        int count = _displayImages.Count;

        _imageItems.Clear();
        for (int i = 0; i < count; i++)
        {
            _imageItems.Add(new ImageDisplayItem
            {
                DisplayPath = _displayImages[i].Path,
                IsExisting = _displayImages[i].IsExisting,
                ImageId = _displayImages[i].ImageId,
                Index = i + 1
            });
        }

        ImageCardsCollection.ItemsSource = _imageItems;

        if (count == 0)
        {
            EmptyImagePlaceholder.IsVisible = true;
            ImageCardsCollection.IsVisible = false;
            ImageCountBadge.IsVisible = false;
        }
        else
        {
            EmptyImagePlaceholder.IsVisible = false;
            ImageCardsCollection.IsVisible = true;
            ImageCountBadge.IsVisible = true;
            ImageCountLabel.Text = $"{count} image{(count > 1 ? "s" : "")} selected";
        }
    }

    private async void OnCameraClicked(object sender, EventArgs e)
    {
        _replaceIndex = -1;
        await OpenImagePicker();
    }

    private async void OnAddMoreTapped(object sender, EventArgs e)
    {
        _replaceIndex = -1;
        await OpenImagePicker();
    }

    private async void OnReplaceImageClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is ImageDisplayItem item)
        {
            _replaceIndex = item.Index - 1;
            await OpenImagePicker();
        }
    }

    private void OnDeleteImageClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is ImageDisplayItem item)
        {
            int index = item.Index - 1;
            if (index < 0 || index >= _displayImages.Count) return;

            if (item.IsExisting)
            {
                _existingImages.RemoveAll(x => x.ImageId == item.ImageId);
            }
            else
            {
                int newIndex = GetNewImageIndex(index);
                if (newIndex >= 0 && newIndex < _selectedImageStreams.Count)
                {
                    _selectedImageStreams[newIndex]?.Dispose();
                    _selectedImageStreams.RemoveAt(newIndex);
                    _selectedImageNames.RemoveAt(newIndex);
                    _selectedImagePaths.RemoveAt(newIndex);
                    _selectedImageIds.RemoveAt(newIndex); // ✅ sync karo
                }
            }

            _displayImages.RemoveAt(index);
            RefreshImagePreviewUI();
        }
    }

    private int GetNewImageIndex(int displayIndex)
    {
        int newCount = 0;
        for (int i = 0; i < displayIndex; i++)
        {
            if (!_displayImages[i].IsExisting)
                newCount++;
        }
        return newCount;
    }

    private async Task OpenImagePicker()
    {
        try
        {
            var result = await DisplayActionSheet("Select Image", "Cancel", null, "Camera", "Gallery");
            switch (result)
            {
                case "Camera": await TakePhoto(); break;
                case "Gallery": await PickSinglePhoto(); break;
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
                await DisplayAlert("Permission Denied", "Camera permission required.", "OK");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Take a photo"
            });

            if (photo != null)
                await ProcessSelectedImage(photo);
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

            if (_replaceIndex >= 0 && _replaceIndex < _displayImages.Count)
            {
                var existing = _displayImages[_replaceIndex];

                if (existing.IsExisting)
                {
                    // ✅ Existing image replace — ImageId save karo
                    _existingImages.RemoveAll(x => x.ImageId == existing.ImageId);
                    _selectedImageStreams.Add(uploadStream);
                    _selectedImageNames.Add(photo.FileName);
                    _selectedImagePaths.Add(photo.FullPath);
                    _selectedImageIds.Add(existing.ImageId); // ✅ ImageId pass karo
                    _displayImages[_replaceIndex] = (IsExisting: false, Path: photo.FullPath, ImageId: existing.ImageId);
                }
                else
                {
                    // New image replace
                    int newIndex = GetNewImageIndex(_replaceIndex);
                    if (newIndex >= 0 && newIndex < _selectedImageStreams.Count)
                    {
                        _selectedImageStreams[newIndex]?.Dispose();
                        _selectedImageStreams[newIndex] = uploadStream;
                        _selectedImageNames[newIndex] = photo.FileName;
                        _selectedImagePaths[newIndex] = photo.FullPath;
                        // ✅ ImageId same rakho jo pehle tha
                        // _selectedImageIds[newIndex] same rahega
                    }
                    _displayImages[_replaceIndex] = (IsExisting: false, Path: photo.FullPath, ImageId: null);
                }

                _replaceIndex = -1;
            }
            else
            {
                // ✅ ADD MODE — ImageId null
                _selectedImageStreams.Add(uploadStream);
                _selectedImageNames.Add(photo.FileName);
                _selectedImagePaths.Add(photo.FullPath);
                _selectedImageIds.Add(null); // ✅ null = naya image
                _displayImages.Add((IsExisting: false, Path: photo.FullPath, ImageId: null));
            }

            await MainThread.InvokeOnMainThreadAsync(RefreshImagePreviewUI);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to process image: {ex.Message}", "OK");
        }
    }

    private async Task UploadSelectedImages(int listerItemId, int listerId)
    {
        try
        {
            if (_selectedImageStreams.Count > 0)
            {
                var uploadResponse = await _addItemsServices.UpdateItemImages(
                    listerItemId: listerItemId,
                    listerId: listerId,
                    imageFiles: _selectedImageStreams,
                    fileNames: _selectedImageNames,
                    imageIds: _selectedImageIds  
                );

                if (uploadResponse?.ResponseCode != "000")
                    await DisplayAlert("Warning", $"Item updated but image upload failed: {uploadResponse?.ResponseMessage}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Warning", $"Item updated but image upload failed: {ex.Message}", "OK");
        }
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        try
        {
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

            UpdateButton.Text = "Updating...";
            UpdateButton.IsEnabled = false;
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            var selectedCat = CategoryPicker.SelectedItem as Categorie;
            var selectedDate = AvailableFromPicker.Date;
           

            var payload = new
            {
                listerItemId = _listerItemId,
                itemName = NameEntry.Text?.Trim(),
                listerId = _listerId,
                itemCost = decimal.TryParse(PriceEntry.Text, out var cost) ? cost : 0,
                itemDescriptions = DescriptionEntry.Text?.Trim(),
                availablefrom = selectedDate,
                status = GetStatusValue(),
                availabilityType = true,
                createdDate = DateTime.UtcNow.ToString("o"),
                updatedOn = DateTime.UtcNow.ToString("o"),
                categoryId = selectedCat?.Id ?? 0
            };

            var url = $"{AppSettings.BaseApiUrl}{Endpoints.UpdateItem}?ListerItemId={_listerItemId}&ListerId={_listerId}";
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                if (_selectedImageStreams.Count > 0)
                {
                    UpdateButton.Text = "Uploading Images...";
                    await UploadSelectedImages(_listerItemId, _listerId);
                }

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
            UpdateButton.Text = "Update Item";
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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        foreach (var s in _selectedImageStreams) s?.Dispose();
        _httpClient?.Dispose();
    }

    public new event PropertyChangedEventHandler PropertyChanged;
    protected new virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ImageDisplayItem
{
    public string DisplayPath { get; set; }
    public bool IsExisting { get; set; }
    public int? ImageId { get; set; }
    public int Index { get; set; }
}
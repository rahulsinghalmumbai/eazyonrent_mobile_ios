using eazyonrent.Model;
using eazyonrent.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace eazyonrent.Pages;

public partial class ItemDetails : ContentPage, INotifyPropertyChanged
{
    private ListerItemResult _itemData;
    private ObservableCollection<string> _itemImages;

    private ObservableCollection<ListerItemResult> _similarItems;
    private string _currentImage;
    private int _currentImageIndex = 0;
    private double _imageZoom = 1.0;
 
    private bool _isLoading;

    private readonly GuestServices _guestServices;
    private int _listerId;
    private int _itemId;

    public ListerItemResult ItemData
    {
        get => _itemData;
        set
        {
            _itemData = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> ItemImages
    {
        get => _itemImages;
        set
        {
            _itemImages = value;
            OnPropertyChanged();
        }
    }


    public ObservableCollection<ListerItemResult> SimilarItems
    {
        get => _similarItems;
        set
        {
            _similarItems = value;
            OnPropertyChanged();
        }
    }

    public string CurrentImage
    {
        get => _currentImage;
        set
        {
            _currentImage = value;
            OnPropertyChanged();
        }
    }

    public string ImageCounterText => ItemImages?.Count > 0 ? $"{_currentImageIndex + 1} / {ItemImages.Count}" : "0 / 0";

    public string CompanyName => _itemData?.CompanyName ?? "";
    public string ItemName => _itemData?.ItemName ?? "";
    public string ItemDescription => _itemData?.ItemDescriptions ?? "";
    public string FormattedCost => _itemData?.ItemCost != null ? $"₹{_itemData.ItemCost.Value:N0}" : "Price not available";
    public string FormattedAvailableDate => _itemData?.Availablefrom?.ToString("dd MMM yyyy") ?? "Date not available";
    public int ViewCount => _itemData?.viewCount ?? 0;

   
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public ItemDetails()
    {
        InitializeComponent();
        InitializeData();
    }

    public ItemDetails(int listerId, int itemId)
    {
        InitializeComponent();
        _guestServices = new GuestServices();
        _listerId = listerId;
        _itemId = itemId;
        InitializeData();
       
    }

    private void InitializeData()
    {
        ItemImages = new ObservableCollection<string>();
        
        SimilarItems = new ObservableCollection<ListerItemResult>();
       
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_listerId <= 0 || _itemId <= 0)
        {
            await DisplayAlert("Error", "Invalid item information.", "OK");
            await Navigation.PopAsync();
            return;
        }

        try
        {
            await LoadItemDetailsAsync();
            await LoadSimilarItemsAsync();
            BindingContext = this;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load page: {ex.Message}", "OK");
        }
    }

    private async Task LoadItemDetailsAsync()
    {
        try
        {
            IsLoading = true;
            // Get API response
            var response = await _guestServices.GetItemDetailsAsync(_listerId, _itemId);
            if (response?.ItemList == null || !response.ItemList.Any())
            {
                await DisplayAlert("Error", "No item details found.", "OK");
                LoadFallbackItemData();
                return;
            }

            var item = response.ItemList.FirstOrDefault();
            if (item == null)
            {
                await DisplayAlert("Error", "No item details available.", "OK");
                LoadFallbackItemData();
                return;
            }

            ItemImages.Clear();

            // Extract images from itemImageList instead of Images property
            if (item.ItemImageList?.Any() == true)
            {
                foreach (var imageItem in item.ItemImageList)
                {
                    if (!string.IsNullOrEmpty(imageItem.ImageName))
                    {
                        ItemImages.Add(imageItem.ImageName);
                    }
                }
                CurrentImage = ItemImages.FirstOrDefault();
            }
            else
            {
                var fallback = GetDummyImagesForCategory(item.CategoryId ?? 0);
                foreach (var img in fallback)
                    ItemImages.Add(img);
                CurrentImage = ItemImages.FirstOrDefault();
            }

            if (item.ItemImageList?.Any() == true)
            {
                item.Images = item.ItemImageList
                    .Where(img => !string.IsNullOrEmpty(img.ImageName))
                    .Select(img => img.ImageName)
                    .ToList();
            }

            this.ItemData = item;
            RefreshAllProperties();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load item details: {ex.Message}", "OK");
            LoadFallbackItemData();
        }
        finally
        {
            IsLoading = false;
        }
    }


    private async Task LoadSimilarItemsAsync()
    {
        try
        {
            if (ItemData == null || !ItemData.CategoryId.HasValue)
            {
                LoadFallbackSimilarItems();
                return;
            }
            // Load similar items from your existing guest service
            // var apiResponse = await _guestServices.LoadSimilarItemsAsync(SelectedCategory.Id);
            var apiResponse = await _guestServices.LoadSimilarItemsAsync(ItemData.CategoryId.Value);

            if (apiResponse != null && apiResponse.ResponseCode == "000" && apiResponse.ItemList != null)
            {
                SimilarItems.Clear();

                // Filter items from same category (excluding current item)
                var similarItems = apiResponse.ItemList
                    .Where(item => item.CategoryId == ItemData?.CategoryId && item.ListerItemId != ItemData?.ListerItemId)
                    .Take(5)
                    .ToList();

                
                foreach (var item in similarItems)
                {
                    if (string.IsNullOrEmpty(item.Location))
                        item.Location = "Noida";

                    if (item.ItemImageList != null && item.ItemImageList.Count > 0)
                    {
                        item.Images = item.ItemImageList
                            .Where(img => !string.IsNullOrEmpty(img.ImageName))
                            .Select(img => img.ImageName)
                            .ToList();
                    }
                    else
                    {
                        item.Images = GetDummyImagesForCategory(item.CategoryId ?? 0);
                    }

                    SimilarItems.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            LoadFallbackSimilarItems();
        }
    }

    
    private void LoadFallbackItemData()
    {
        ItemData = new ListerItemResult
        {
            ListerItemId = _itemId,
            ItemName = "Sample Item",
            CompanyName = "Sample Company",
            ListerId = _listerId,
            viewCount = 10,
            ItemCost = 1500,
            ItemDescriptions = "This is a sample item description.",
            Availablefrom = DateTime.Now,
            Status = 1,
            CategoryId = 1,
            Location = "Noida",
            Images = GetDummyImagesForCategory(1)
        };

        ItemImages.Clear();
        foreach (var image in ItemData.Images)
        {
            ItemImages.Add(image);
        }
        CurrentImage = ItemImages.FirstOrDefault();
        RefreshAllProperties();
    }

    private void LoadFallbackSimilarItems()
    {
        SimilarItems.Clear();
        SimilarItems.Add(new ListerItemResult
        {
            ListerItemId = 1,
            ItemName = "Similar Item 1",
            CompanyName = "Company 1",
            ItemCost = 1200,
            Images = GetDummyImagesForCategory(1)
        });
        SimilarItems.Add(new ListerItemResult
        {
            ListerItemId = 2,
            ItemName = "Similar Item 2",
            CompanyName = "Company 2",
            ItemCost = 1800,
            Images = GetDummyImagesForCategory(1)
        });
    }

    private List<string> GetDummyImagesForCategory(int categoryId)
    {
        return categoryId switch
        {
            1 => new List<string> // Laptop/Desktop
            {
                "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=400",
                "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=400",
                "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=400"
            },
            2 => new List<string> // Others
            {
                "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=400",
                "https://images.unsplash.com/photo-1506439773649-6e0eb8cfb237?w=400"
            },
            3 => new List<string> // Drone
            {
                "https://images.unsplash.com/photo-1558618047-3c8c76ca7d13?w=400",
                "https://images.unsplash.com/photo-1571068316344-75bc76f77890?w=400"
            },
            4 => new List<string> // Clothes
            {
                "https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=400",
                "https://images.unsplash.com/photo-1434389677669-e08b4cac3105?w=400"
            },
            _ => new List<string>
            {
                "https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=400",
                "https://images.unsplash.com/photo-1593062096033-9a26b09da705?w=400"
            }
        };
    }

    private void OnPrevImageClicked(object sender, EventArgs e)
    {
        NavigateToPreviousImage();
    }

    private void OnNextImageClicked(object sender, EventArgs e)
    {
        NavigateToNextImage();
    }

    private void OnZoomInClicked(object sender, EventArgs e)
    {
        _imageZoom = Math.Min(_imageZoom + 0.2, 3.0);
        // Note: Actual zoom requires custom implementation
    }

    private void OnZoomOutClicked(object sender, EventArgs e)
    {
        _imageZoom = Math.Max(_imageZoom - 0.2, 0.5);
        // Note: Actual zoom requires custom implementation
    }

    private void OnThumbnailSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string selectedImage)
        {
            SelectImage(selectedImage);
        }
    }

    private async void OnChatClicked(object sender, EventArgs e)
    {
        try
        {
            IsBusy = true;
           // await DisplayAlert("Chat", $"Opening chat with {CompanyName}...", "OK");
            await Navigation.PushAsync(new UserChatPage());
            // Navigate to chat page: await Shell.Current.GoToAsync($"//chat?listerId={_listerId}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Unable to open chat. Please try again.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnBookNowClicked(object sender, EventArgs e)
    {
        try
        {
            IsBusy = true;
            //await DisplayAlert("Book Now", $"Booking {ItemName}...", "OK");
            await Navigation.PushAsync(new BookItemPage(_itemId, _listerId, ItemName));
            
            // Navigate to booking page: await Shell.Current.GoToAsync($"//booking?itemId={_itemId}&listerId={_listerId}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Unable to proceed with booking. Please try again.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnSimilarItemTapped(object sender, EventArgs e)
    {
        var tappedEventArgs = e as TappedEventArgs;
        if (tappedEventArgs?.Parameter is ListerItemResult similarItem)
        {
            IsBusy = true;
            // Navigate to similar item details
            await Navigation.PushAsync(new ItemDetails(similarItem.ListerId, similarItem.ListerItemId));
            IsBusy = false;
        }
    }

    public void NavigateToNextImage()
    {
        if (ItemImages?.Count > 0)
        {
            _currentImageIndex = (_currentImageIndex + 1) % ItemImages.Count;
            CurrentImage = ItemImages[_currentImageIndex];
            OnPropertyChanged(nameof(ImageCounterText));
        }
    }
    public void NavigateToPreviousImage()
    {
        if (ItemImages?.Count > 0)
        {
            _currentImageIndex = _currentImageIndex == 0 ? ItemImages.Count - 1 : _currentImageIndex - 1;
            CurrentImage = ItemImages[_currentImageIndex];
            OnPropertyChanged(nameof(ImageCounterText));
        }
    }

    public void SelectImage(string image)
    {
        var index = ItemImages?.IndexOf(image) ?? -1;
        if (index >= 0)
        {
            _currentImageIndex = index;
            CurrentImage = image;
            OnPropertyChanged(nameof(ImageCounterText));
        }
    }

    private void RefreshAllProperties()
    {
        OnPropertyChanged(nameof(ItemData));
        OnPropertyChanged(nameof(CompanyName));
        OnPropertyChanged(nameof(ItemName));
        OnPropertyChanged(nameof(ItemDescription));
        OnPropertyChanged(nameof(FormattedCost));
        OnPropertyChanged(nameof(FormattedAvailableDate));
        OnPropertyChanged(nameof(ViewCount));
        OnPropertyChanged(nameof(ImageCounterText));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}




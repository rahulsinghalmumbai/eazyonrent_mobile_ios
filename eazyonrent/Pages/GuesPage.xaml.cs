using eazyonrent.Model;
using eazyonrent.Services;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;

namespace eazyonrent.Pages;

public partial class GuesPage : ContentPage, INotifyPropertyChanged
{
    private ObservableCollection<ListerItemResult> _items;
    private ObservableCollection<Categorie> _categories;
    private bool _isLoading;
    private int? _selectedCategoryId;

    private readonly GuestServices _guestServices;
    public ObservableCollection<ListerItemResult> Items
    {
        get => _items;
        set
        {
            _items = value;
            OnPropertyChanged();
        }
    }
    public ObservableCollection<Categorie> Categories
    {
        get => _categories;
        set
        {
            _categories = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public GuesPage()
    {
        InitializeComponent();

        _guestServices = new GuestServices();
        Items = new ObservableCollection<ListerItemResult>();
        Categories = new ObservableCollection<Categorie>();
        BindingContext = this;
        LoadItemsFromAPI();
        LoadCategoriesFromAPI();
    }

    private async Task LoadItemsFromAPI()
    {
        try
        {
            IsLoading = true;
            var apiResponse = await _guestServices.GetGuestItemsAsync();
            if (apiResponse != null && apiResponse.ResponseCode == "000" && apiResponse.ItemList != null)
            {
                Items.Clear();
                foreach (var item in apiResponse.ItemList)
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

                    Items.Add(item);
                }
            }
            else
            {
                await DisplayAlert("Error", apiResponse?.ResponseMessage ?? "Failed to load items", "OK");
                LoadFallbackData();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error loading data: {ex.Message}", "OK");
            LoadFallbackData();
        }
        finally
        {
            IsLoading = false;
        }
    }
    private async void OnCategorySelected(object sender, EventArgs e)
    {
        var tappedEventArgs = e as TappedEventArgs;
        if (tappedEventArgs?.Parameter is Categorie selectedCategory)
        {
            // Update selection state
            foreach (var category in Categories)
            {
                category.IsSelected = category.Id == selectedCategory.Id;
            }

            _selectedCategoryId = selectedCategory.Id == 0 ? null : selectedCategory.Id;

            // Hide filter dropdown
            CategoryFilterLayout.IsVisible = false;
            isFilterVisible = false;
            CategoryFilterBtn.Text = "🔽 Filter";

            // Filter items based on selection
           // await FilterItemsByCategory();

            await DisplayAlert("Filter Applied", $"Showing items for: {selectedCategory.CategoriesName}", "OK");
        }
    }
    private async Task LoadFallbackCategories()
    {
        Categories.Clear();
        Categories.Add(new Categorie { Id = 0, CategoriesName = "All Categories", Status = true, IsSelected = true });
        Categories.Add(new Categorie { Id = 1, CategoriesName = "Laptop/Desktop", Status = true });
        Categories.Add(new Categorie { Id = 2, CategoriesName = "Others", Status = true });
        Categories.Add(new Categorie { Id = 3, CategoriesName = "Drone", Status = true });
        Categories.Add(new Categorie { Id = 4, CategoriesName = "Clothes", Status = true });
        try
        {
            IsLoading = true;
            await LoadItemsFromAPI();
        }
        catch (Exception ex)
        {
             DisplayAlert("Error", $"Error loading data: {ex.Message}. Loading sample data.", "OK");
            LoadFallbackData();
        }
        finally
        {
            IsLoading = false;
        }
    }
    private void LoadFallbackData()
    {
        Items.Clear();
        Items.Add(new ListerItemResult
        {
            ListerItemId = 1,
            ItemName = "Sample MacBook Pro",
            CompanyName = "TechRent Solutions",
            Location = "Noida",
            ItemCost = 2500,
            CategoryId = 1,
            Images = GetDummyImagesForCategory(1)
        });

        Items.Add(new ListerItemResult
        {
            ListerItemId = 2,
            ItemName = "Office Chair",
            CompanyName = "FurniRent Pro",
            Location = "Noida",
            ItemCost = 150,
            CategoryId = 2,
            Images = GetDummyImagesForCategory(2)
        });
    }

    private List<string> GetDummyImagesForCategory(int? categoryId)
    {
        return categoryId switch
        {
            1 => new List<string> // Electronics
            {
                "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=400",
                "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=400",
                "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=400"
            },
            2 => new List<string> // Furniture
            {
                "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=400",
                "https://images.unsplash.com/photo-1506439773649-6e0eb8cfb237?w=400",
                "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=400"
            },
            3 => new List<string> // Vehicles
            {
                "https://images.unsplash.com/photo-1558618047-3c8c76ca7d13?w=400",
                "https://tse4.mm.bing.net/th/id/OIP.WRJhX-iK8WZcXn4HSa1-iwHaEW?pid=ImgDet&w=474&h=278&rs=1&o=7&rm=3"
            },
            _ => new List<string> // Default
            {
                "https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=400",
                "https://images.unsplash.com/photo-1593062096033-9a26b09da705?w=400",
                "https://images.unsplash.com/photo-1502920917128-1aa500764cbd?w=400"
            }
        };
    }

    private bool isFilterVisible = false;

    private async Task LoadCategoriesFromAPI()
    {
        try
        {
            var apiResponse = await _guestServices.GetAllCategoriesAsync();

            if (apiResponse != null && apiResponse.ResponseCode == "000" && apiResponse.CategoriesList != null)
            {
                Categories.Clear();

                foreach (var category in apiResponse.CategoriesList)
                {
                    Categories.Add(category);
                }
            }
            else
            {
                LoadFallbackCategories();
            }
        }
        catch (Exception ex)
        {
            LoadFallbackCategories();
        }
    }
    private void OnCategoryFilterClicked(object sender, EventArgs e)
    {
        isFilterVisible = !isFilterVisible;
        CategoryFilterLayout.IsVisible = isFilterVisible;

        var button = sender as Button;
        button.Text = isFilterVisible ? "🔼 Filter" : "🔽 Filter";
    }

    private async void OnItemTapped(object sender, EventArgs e)
    {
        var tappedEventArgs = e as TappedEventArgs;
        if (tappedEventArgs?.Parameter is ListerItemResult item)
        {

            //await DisplayAlert("Item Details",
            //    $"Item: {item.ItemName}\nCompany: {item.CompanyName}\nLocation: {item.Location}\nPrice: ₹{item.ItemCost}/day\nDescription: {item.ItemDescriptions}",
            //    "OK");
            //await Task.Delay(100);
            await Navigation.PushAsync(new ItemDetails(item.ListerId, item.ListerItemId));
            }
    }
    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GuesPage());
    }
    private async void OnSearchClicked(object sender, EventArgs e)
    {
        SearchEntry.Focus();
        await DisplayAlert("Navigation", "Search clicked!", "OK");
           
    }
    private async void OnAddItemClicked(object sender, EventArgs e)
    {
        //await DisplayAlert("Navigation", "Add Item clicked!", "OK");
        IsBusy = true;
        await Navigation.PushAsync(new AddItemPage());
        IsBusy = false;

    }
    private async void OnProfileClicked(object sender, EventArgs e)
    {
        //    await DisplayAlert("Navigation", "Profile clicked!", "OK");
        await Navigation.PushAsync(new UserProfilePage());

    }
    private async void OnLoadMoreItems(object sender, EventArgs e)
    {
        // Implement pagination if needed
        await Task.Delay(100);
    }
    private async void OnOrdersClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Navigation", "Orders clicked!", "OK");

    }
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


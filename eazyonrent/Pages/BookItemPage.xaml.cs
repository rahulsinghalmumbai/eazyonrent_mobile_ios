using eazyonrent.Model;
using eazyonrent.Services;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace eazyonrent.Pages;

[QueryProperty(nameof(ItemId), "itemId")]
[QueryProperty(nameof(ListerId), "listerId")]
[QueryProperty(nameof(ItemName), "itemName")]
public partial class BookItemPage : ContentPage
{
    private string _itemName;
    private int _itemId;
    private int _listerId;
    private int _selectedRating = 0;
    private readonly BookServices _bookServices;

    // Query Properties
    public string ItemId
    {
        set => _itemId = int.Parse(value ?? "0");
    }

    public string ListerId
    {
        set => _listerId = int.Parse(value ?? "0");
    }
    public string ItemName  
    {
        set => _itemName = value;
    }
    public BookItemPage(int itemId, int listerId, string itemName = null) 
    {
        InitializeComponent();
        _bookServices = new BookServices();
        _itemId = itemId;
        _listerId = listerId;
        _itemName = itemName;
        // Set minimum dates to today
        RentFromDatePicker.MinimumDate = DateTime.Today;
        RentToDatePicker.MinimumDate = DateTime.Today;

        // Set default dates
        RentFromDatePicker.Date = DateTime.Today;
        RentToDatePicker.Date = DateTime.Today.AddDays(1);

        // Subscribe to date change events
        RentFromDatePicker.DateSelected += OnRentFromDateSelected;
        RentToDatePicker.DateSelected +=  OnRentToDateSelected;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Update title with item name
        if (!string.IsNullOrEmpty(_itemName))
        {
            Title = $"Book #{_itemName}";
        }
        else
        {
            Title = $"Book Item #{_itemId}";
        }

       
    }
    // Star Rating Event Handler
    private void OnStarClicked(object sender, EventArgs e)
    {
        if (sender is Button clickedStar)
        {
            // Determine which star was clicked
            var stars = new[] { Star1, Star2, Star3, Star4, Star5 };
            var clickedIndex = Array.IndexOf(stars, clickedStar);

            if (clickedIndex >= 0)
            {
                _selectedRating = clickedIndex + 1;
                UpdateStarDisplay();
            }
        }
    }

    private void UpdateStarDisplay()
    {
        var stars = new[] { Star1, Star2, Star3, Star4, Star5 };

        for (int i = 0; i < stars.Length; i++)
        {
            if (i < _selectedRating)
            {
                stars[i].TextColor = Color.FromArgb("#FFD700"); 
            }
            else
            {
                stars[i].TextColor = Color.FromArgb("#CCCCCC"); 
            }
        }
    }

    // Date validation
    private void OnRentFromDateSelected(object sender, DateChangedEventArgs e)
    {
        // Ensure rent to date is after rent from date
        if (RentToDatePicker.Date <= e.NewDate)
        {
            RentToDatePicker.Date = e.NewDate.AddDays(1);
        }
        RentToDatePicker.MinimumDate = e.NewDate.AddDays(1);
    }

    private async void OnRentToDateSelected(object sender, DateChangedEventArgs e)
    {
        // Ensure rent to date is after rent from date
        if (e.NewDate <= RentFromDatePicker.Date)
        {
            await DisplayAlert("Invalid Date", "Rent to date must be after rent from date.", "OK");
            RentToDatePicker.Date = RentFromDatePicker.Date.AddDays(1);
        }
    }

    // Main booking method
    private async void OnFinalBookClicked(object sender, EventArgs e)
    {
        if (!await ValidateForm())
            return;

        try
        {
            // Show loading overlay
            LoadingOverlay.IsVisible = true;
            BookNowButton.IsEnabled = false;

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

            // Prepare booking request model
            var renterItem = new RenterItem
            {
                RenterID = listerId,   
                ItemId = _itemId,
                RentFromDate = RentFromDatePicker.Date,
                RentToDate = RentToDatePicker.Date,
                Review = ReviewEditor.Text,
                Rating = 0,
                Status = true
            };

          
            var response = await _bookServices.FinalBooking(renterItem);

            if (response != null && response.Data != null)
            {
                await DisplayAlert("✅ Success", response.ResponseMessage ?? "Your booking has been confirmed!", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("❌ Error", response?.ResponseMessage ?? "Failed to process booking. Please try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            // Hide loading overlay
            LoadingOverlay.IsVisible = false;
            BookNowButton.IsEnabled = true;
        }
    }

    private async Task<bool> ValidateForm()
    {
        // Validate item and lister IDs
        if (_itemId <= 0 || _listerId <= 0)
        { 
            await DisplayAlert("Error", "Invalid item or lister information.", "OK");
            return false;
        }

        // Validate dates
        if (RentFromDatePicker.Date < DateTime.Today)
        {
            await DisplayAlert("Invalid Date", "Rent from date cannot be in the past.", "OK");
            return false;
        }

        if (RentToDatePicker.Date <= RentFromDatePicker.Date)
        {
            await DisplayAlert("Invalid Date", "Rent to date must be after rent from date.", "OK");
            return false;
        }

        // Check if rental period is reasonable (not more than 365 days)
        var rentalDays = (RentToDatePicker.Date - RentFromDatePicker.Date).Days;
        if (rentalDays > 365)
        {
            await DisplayAlert("Invalid Period", "Rental period cannot exceed 365 days.", "OK");
            return false;
        }

        return true;
    }

   

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Cancel Booking", "Are you sure you want to cancel this booking?", "Yes", "No");

        if (confirm)
        {
            await Navigation.PopAsync();
        }
    }

    // Animation for better user experience
    private async void AnimateButton(Button button)
    {
        await button.ScaleTo(0.95, 100);
        await button.ScaleTo(1, 100);
    }
}
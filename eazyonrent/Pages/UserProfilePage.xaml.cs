using eazyonrent.Model;
using eazyonrent.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace eazyonrent.Pages
{
    public partial class UserProfilePage : TabbedPage
    {
        private readonly LoginServices loginServices;
        private int? _currentListerId;
        private FileResult _selectedImageFile;

        public UserProfilePage()
        {
            InitializeComponent();
            loginServices = new LoginServices();

            // Subscribe to tab selection change event
            this.CurrentPageChanged += OnCurrentPageChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await OnLoadProfileDta(this, EventArgs.Empty);
        }

        private async void OnCurrentPageChanged(object sender, EventArgs e)
        {
            // Check if current page is Item tab
            if (this.CurrentPage != null && this.CurrentPage.Title == "Item")
            {
                await OnLoadItems(this, EventArgs.Empty);
            }
            else if (this.CurrentPage != null && this.CurrentPage.Title == "History")
            {
                await OnLoadHistory(this, EventArgs.Empty);
            }
        }

        private async Task OnLoadProfileDta(object sender, EventArgs e)
        {
            try
            {
                var mobile = await SecureStorage.GetAsync("mobile");
                var response = await loginServices.LoginAsync(mobile);

                if (response != null && response.ExistUser != null)
                {
                    _currentListerId = response.ExistUser.ListerId;
                    NameEntry.Text = response.ExistUser.Name;
                    EmailEntry.Text = response.ExistUser.Email;
                    MobileEntry.Text = response.ExistUser.Mobile;
                    CompanyEntry.Text = response.ExistUser.CompanyName;
                    AddressEditor.Text = response.ExistUser.Address;
                    CityEntry.Text = response.ExistUser.City;

                    // Profile Image
                    if (!string.IsNullOrEmpty(response.ExistUser.DefaultImage))
                    {
                        ProfileImage.Source = response.ExistUser.DefaultImage;
                    }
                }
                else
                {
                    await DisplayAlert("Info", response?.ResponseMessage ?? "No data found", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load profile: {ex.Message}", "OK");
            }
        }

        private async void OnSaveProfileData(object sender, EventArgs e)
        {
            try
            {
                IsBusy = true;
                var updateRequest = new ListerUpdateRequest
                {
                    ListerId = _currentListerId,
                    Name = NameEntry.Text,
                    Email = EmailEntry.Text,
                    Mobile = MobileEntry.Text,
                    CompanyName = CompanyEntry.Text,
                    Address = AddressEditor.Text,
                    City = CityEntry.Text,
                    ImageFile = _selectedImageFile
                };

                var result = await loginServices.UpdateProfileAsync(updateRequest);

                if (result != null && (result.ResponseCode == "000" || result.ResponseCode == "200"))
                {
                    await DisplayAlert("✅ Success", "Profile updated successfully", "OK");
                }
                else
                {
                    await DisplayAlert("❌ Error", result?.ResponseMessage ?? "Update failed", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("⚠️ Error", $"Failed to save profile: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OnLoadItems(object sender, EventArgs e)
        {
            try
            {
                IsBusy = true;
                var response = await loginServices.OnLoadProfileItem(_currentListerId);

                if (response.ResponseCode == "000" && response.ItemList?.Count > 0)
                {
                    // Convert file paths to URLs for each item's images
                    foreach (var item in response.ItemList)
                    {
                        if (item.ItemImageList != null)
                        {
                            foreach (var image in item.ItemImageList)
                            {
                                if (!string.IsNullOrEmpty(image.ImageName))
                                {
                                    image.ImageName = image.ImageName;
                                }
                            }
                        }
                    }

                    MyCollectionView.ItemsSource = response.ItemList;
                }
                else
                {
                    await DisplayAlert("Info", response.ResponseMessage ?? "No items found", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load profile: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OnLoadHistory(object sender, EventArgs e)
        {
            try
            {
                IsBusy = true;

                var historyResponse = await loginServices.GetBookingHistoryAsync(_currentListerId);

                if (historyResponse != null && historyResponse.responseCode == "000" && historyResponse.data?.Count > 0)
                {
                    // Directly use the data from response - no need to create new list
                    HistoryCollectionView.ItemsSource = historyResponse.data;
                    NoHistoryLabel.IsVisible = false;
                    HistoryCollectionView.IsVisible = true;
                }
                else
                {
                    NoHistoryLabel.IsVisible = true;
                    HistoryCollectionView.IsVisible = false;
                    if (historyResponse != null && !string.IsNullOrEmpty(historyResponse.responseMessage))
                    {
                        await DisplayAlert("Info", historyResponse.responseMessage, "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load history: {ex.Message}", "OK");
                NoHistoryLabel.IsVisible = true;
                HistoryCollectionView.IsVisible = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnHistoryItemTapped(object sender, EventArgs e)
        {
            try
            {
                IsBusy = true;
                var tappedEventArgs = e as TappedEventArgs;
                if (tappedEventArgs?.Parameter is BookedItemHistory item)
                {
                    if (item.ListerId.HasValue && item.ItemId.HasValue)
                    {
                        await Navigation.PushAsync(new ItemDetails(item.ListerId.Value, item.ItemId.Value));
                    }
                    else
                    {
                        await DisplayAlert("Info", "Item details not available", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to open item details: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnUploadImageClicked(object sender, EventArgs e)
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.Photos>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Required", "Please grant permission to access your photos", "OK");
                    return;
                }

                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select Profile Picture"
                });

                if (result != null)
                {
                    _selectedImageFile = result;

                    using var stream = await result.OpenReadAsync();
                    ProfileImage.Source = ImageSource.FromStream(() => stream);

                    await DisplayAlert("Success", "Profile image selected successfully!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to upload image: {ex.Message}", "OK");
            }
        }

        private async void OnItemTapped(object sender, EventArgs e)
        {
            try
            {
                IsBusy = true;
                var tappedEventArgs = e as TappedEventArgs;
                if (tappedEventArgs?.Parameter is ListerItemProfileResult item)
                {
                    await Navigation.PushAsync(new ItemDetails(item.ListerId, item.ListerItemId));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to open item details: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
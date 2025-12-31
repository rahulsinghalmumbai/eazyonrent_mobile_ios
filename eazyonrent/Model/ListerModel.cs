using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eazyonrent.Model
{
    public class ListerModel
    {
        public string? ResponseCode { get; set; }
        public string? ResponseMessage { get; set; }
        public int? ListerId { get; set; }   
        public ExistUser? ExistUser { get; set; }  
    }

    public class ExistUser
    {
        public int ListerId { get; set; }
        public string? CompanyName { get; set; }
        public string? Tags { get; set; }
        public string? Address { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? DefaultImage { get; set; }
        public string? Descriptions { get; set; }
        public bool Status { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? City { get; set; }
        public double? Lat { get; set; }
        public double? Long { get; set; }
        public string? LatLongAddress { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
    public class MyItem : INotifyPropertyChanged
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }
        public string ItemImage { get; set; }
        public decimal ItemCost { get; set; }
        public string Status { get; set; } // Available, Rented, Maintenance
        public DateTime CreatedDate { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Model for rental history
    public class HistoryItem : INotifyPropertyChanged
    {
        public string HistoryId { get; set; }
        public string ItemName { get; set; }
        public string ItemImage { get; set; }
        public string TransactionType { get; set; } // "Rented from", "Lent to"
        public string Status { get; set; } // Completed, Active, Cancelled
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OtherUserName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Main ViewModel for the Profile Page using your existing ListerModel
    public class UserProfileViewModel : INotifyPropertyChanged
    {
        private ExistUser _currentUser;
        private ObservableCollection<MyItem> _myItems;
        private ObservableCollection<HistoryItem> _historyItems;
        private ObservableCollection<HistoryItem> _filteredHistoryItems;

        public ExistUser CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(UserName));
                OnPropertyChanged(nameof(UserEmail));
                OnPropertyChanged(nameof(UserMobile));
                OnPropertyChanged(nameof(UserAddress));
                OnPropertyChanged(nameof(UserProfileImage));
                OnPropertyChanged(nameof(CompanyName));
            }
        }

        // Properties for easy binding to UI
        public string UserName
        {
            get => _currentUser?.Name ?? "";
            set
            {
                if (_currentUser != null)
                {
                    _currentUser.Name = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        public string UserEmail
        {
            get => _currentUser?.Email ?? "";
            set
            {
                if (_currentUser != null)
                {
                    _currentUser.Email = value;
                    OnPropertyChanged(nameof(UserEmail));
                }
            }
        }

        public string UserMobile
        {
            get => _currentUser?.Mobile ?? "";
            set
            {
                if (_currentUser != null)
                {
                    _currentUser.Mobile = value;
                    OnPropertyChanged(nameof(UserMobile));
                }
            }
        }

        public string UserAddress
        {
            get => _currentUser?.Address ?? "";
            set
            {
                if (_currentUser != null)
                {
                    _currentUser.Address = value;
                    OnPropertyChanged(nameof(UserAddress));
                }
            }
        }

        public string UserProfileImage
        {
            get => _currentUser?.DefaultImage ?? "profile_placeholder.png";
            set
            {
                if (_currentUser != null)
                {
                    _currentUser.DefaultImage = value;
                    OnPropertyChanged(nameof(UserProfileImage));
                }
            }
        }

        public string CompanyName
        {
            get => _currentUser?.CompanyName ?? "";
            set
            {
                if (_currentUser != null)
                {
                    _currentUser.CompanyName = value;
                    OnPropertyChanged(nameof(CompanyName));
                }
            }
        }

        public ObservableCollection<MyItem> MyItems
        {
            get => _myItems;
            set
            {
                _myItems = value;
                OnPropertyChanged(nameof(MyItems));
            }
        }

        public ObservableCollection<HistoryItem> HistoryItems
        {
            get => _filteredHistoryItems ?? _historyItems;
            set
            {
                _historyItems = value;
                _filteredHistoryItems = value;
                OnPropertyChanged(nameof(HistoryItems));
            }
        }

        public UserProfileViewModel()
        {
            LoadSampleData();
        }

        public UserProfileViewModel(ExistUser user)
        {
            CurrentUser = user;
            LoadUserItems();
            LoadUserHistory();
        }

        private void LoadSampleData()
        {
            // Sample user data
            CurrentUser = new ExistUser
            {
                ListerId = 1,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Mobile = "+91 9876543210",
                Address = "Sector 18, Noida, UP",
                City = "Noida",
                CompanyName = "EazyRent Solutions",
                DefaultImage = "profile_placeholder.png",
                Status = true,
                CreatedDate = DateTime.Now.AddMonths(-6)
            };

            LoadUserItems();
            LoadUserHistory();
        }

        private void LoadUserItems()
        {
            MyItems = new ObservableCollection<MyItem>
            {
                new MyItem
                {
                    ItemId = "1",
                    ItemName = "Canon DSLR Camera",
                    Category = "Electronics",
                    ItemImage = "camera_placeholder.jpg",
                    ItemCost = 500,
                    Status = "Available",
                    CreatedDate = DateTime.Now.AddDays(-30)
                },
                new MyItem
                {
                    ItemId = "2",
                    ItemName = "Mountain Bike",
                    Category = "Sports",
                    ItemImage = "bike_placeholder.jpg",
                    ItemCost = 200,
                    Status = "Rented",
                    CreatedDate = DateTime.Now.AddDays(-15)
                },
                new MyItem
                {
                    ItemId = "3",
                    ItemName = "Party Tent",
                    Category = "Events",
                    ItemImage = "tent_placeholder.jpg",
                    ItemCost = 800,
                    Status = "Available",
                    CreatedDate = DateTime.Now.AddDays(-7)
                }
            };
        }

        private void LoadUserHistory()
        {
            HistoryItems = new ObservableCollection<HistoryItem>
            {
                new HistoryItem
                {
                    HistoryId = "1",
                    ItemName = "Sony PlayStation 5",
                    ItemImage = "ps5_placeholder.jpg",
                    TransactionType = "Rented from John",
                    Status = "Completed",
                    StartDate = DateTime.Now.AddDays(-10),
                    EndDate = DateTime.Now.AddDays(-7),
                    TotalAmount = 600,
                    OtherUserName = "John"
                },
                new HistoryItem
                {
                    HistoryId = "2",
                    ItemName = "Wedding Decoration",
                    ItemImage = "decoration_placeholder.jpg",
                    TransactionType = "Lent to Sarah",
                    Status = "Active",
                    StartDate = DateTime.Now.AddDays(-2),
                    EndDate = DateTime.Now.AddDays(1),
                    TotalAmount = 1500,
                    OtherUserName = "Sarah"
                },
                new HistoryItem
                {
                    HistoryId = "3",
                    ItemName = "Gaming Chair",
                    ItemImage = "chair_placeholder.jpg",
                    TransactionType = "Rented from Mike",
                    Status = "Completed",
                    StartDate = DateTime.Now.AddDays(-20),
                    EndDate = DateTime.Now.AddDays(-15),
                    TotalAmount = 350,
                    OtherUserName = "Mike"
                }
            };
        }

        public void FilterHistory(string filterType)
        {
            if (filterType == "All")
            {
                _filteredHistoryItems = _historyItems;
            }
            else if (filterType == "Rented")
            {
                _filteredHistoryItems = new ObservableCollection<HistoryItem>(
                    _historyItems.Where(h => h.TransactionType.StartsWith("Rented from"))
                );
            }
            else if (filterType == "Lent")
            {
                _filteredHistoryItems = new ObservableCollection<HistoryItem>(
                    _historyItems.Where(h => h.TransactionType.StartsWith("Lent to"))
                );
            }

            OnPropertyChanged(nameof(HistoryItems));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
